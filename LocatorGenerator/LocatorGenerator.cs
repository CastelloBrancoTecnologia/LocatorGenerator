using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using AutoRegisterInject;

namespace CastelloBranco.LocatorGenerator;

[Generator]
public class LocatorSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ClassCollector());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        string[] attributes =
        [
            typeof(RegisterTransientAttribute).FullName!,
            typeof(RegisterSingletonAttribute).FullName!,
            typeof(RegisterScopedAttribute).FullName!,
            typeof(RegisterHostedServiceAttribute).FullName!
        ];

        if (context.SyntaxReceiver is not ClassCollector receiver)
            return;

        string nameSpace = GetRootNamespace(context);

        List<string> markedClasses = [];

        foreach (var classDeclaration in receiver.CandidateClasses)
        {
            var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (classSymbol?.GetAttributes().Any(attr =>
                attr.AttributeClass is not null && 
                attributes.Contains(attr.AttributeClass.ToDisplayString())) == true)
            {
                markedClasses.Add($"{classSymbol.ContainingNamespace.ToDisplayString()}.{classSymbol.Name}");
            }
        }

        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        string generatedCode1  = GenerateSingletonClass(markedClasses, nameSpace, version);
        string generatedCode2  = AutoWrireSorceCode.Replace("@namespace", nameSpace).Replace("@version", version);
        
        context.AddSource("ViewLocator.g.cs", SourceText.From(generatedCode1, Encoding.UTF8));
        context.AddSource("AutoWrire.g.cs",   SourceText.From(generatedCode2, Encoding.UTF8));
    }
    
    
    private const string AutoWrireSorceCode = @"
            using Avalonia;
            using Avalonia.Controls;
            using CommunityToolkit.Mvvm.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;
            using System;
            using System.Linq;
            using System.CodeDom.Compiler;

            namespace @namespace;

            [GeneratedCode(""CastelloBranco.LocatorGenerator"", ""@version"")]
            public class AutoWireViewModel : AvaloniaObject
            {
                public static readonly AttachedProperty<bool> EnabledProperty =
                   AvaloniaProperty.RegisterAttached<AutoWireViewModel, Control, bool>(""Enabled"");

                public static bool GetEnabled(Control element)
                {
                    return element.GetValue(EnabledProperty);
                }

                public static void SetEnabled(Control element, bool value)
                {
                    element.SetValue(EnabledProperty, value);
                }

                static AutoWireViewModel()
                {
                    EnabledProperty.Changed.AddClassHandler<Control>(HandleEnabledPropertyChanged);
                }

                private static void HandleEnabledPropertyChanged(Control controlElem, AvaloniaPropertyChangedEventArgs args)
                {
                    if (controlElem is Window || controlElem is UserControl)
                    {
                        if (((bool?)args.NewValue ?? false))
                        {
                            Type vmType = ViewLocator
                                          .ViewModelsViewsDictionary
                                          .FirstOrDefault(x => x.Value == controlElem.GetType())
                                          .Key;

                            controlElem.DataContext = (Ioc.Default.GetRequiredService(vmType) ?? throw new InvalidOperationException($""{vmType.Name} is not registered in services collection.""));
                        }
                    }
                }
            }
          ";

    private static string GetRootNamespace(GeneratorExecutionContext context)
    {
        return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns)
            ? ns
            : context.Compilation.AssemblyName ?? "GlobalNamespace";
    }

    private static string GetClassnameWithoutNamespace(string fullNmae)
    {
        int lastDotIndex = fullNmae.LastIndexOf('.');
        int afterLastDotIndex = lastDotIndex + 1;

        if (lastDotIndex < 0)
        {
            return fullNmae;
        }
        else if (afterLastDotIndex >= fullNmae.Length)
        {
            return "";
        }
        else
        {
            return fullNmae.Substring(afterLastDotIndex);
        }
    }

    private static string GenerateSingletonClass(List<string> classNames, string nameSpace, string version)
    {
        string[] viewModels = classNames.Where(x => x.EndsWith("ViewModel", StringComparison.InvariantCulture)).ToArray();

        var sb = new StringBuilder();

        sb.AppendLine("#nullable enable ");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.CodeDom.Compiler;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Avalonia.Controls;");
        sb.AppendLine("using Avalonia.Controls.Templates;");
        sb.AppendLine("using CommunityToolkit.Mvvm;");                    
        sb.AppendLine("using CommunityToolkit.Mvvm.ComponentModel;");
        sb.AppendLine("using CommunityToolkit.Mvvm.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine($"namespace {nameSpace};");
        sb.AppendLine();
        sb.AppendLine($"[GeneratedCode(\"CastelloBranco.Locator\", \"{version}\")]");
        sb.AppendLine("public class ViewLocator : IDataTemplate");
        sb.AppendLine("{");
        sb.AppendLine("    public static bool SupportsRecycling => false;");
        sb.AppendLine();
        sb.AppendLine("    public static readonly Dictionary<Type, Type?> ViewModelsViewsDictionary = new()");
        sb.AppendLine("    {");

        for (int i = 0; i < viewModels.Length; i++)
        {
            string viewModelName = viewModels[i];

            string viewName = GetClassnameWithoutNamespace(viewModelName.Substring(0, viewModelName.Length - "Model".Length));

            string alternateViewName = GetClassnameWithoutNamespace(viewModelName.Substring(0, viewModelName.Length - "ViewModel".Length));

            if (classNames.Any (x => GetClassnameWithoutNamespace(x) == viewName))
            {
                string s = classNames.First(x => GetClassnameWithoutNamespace(x) == viewName);

                sb.AppendLine($"        {{ typeof({viewModelName}), typeof({s}) }}, ");
            }
            else if (classNames.Any (x => GetClassnameWithoutNamespace(x) == alternateViewName))
            {
                string s = classNames.First(x => GetClassnameWithoutNamespace(x) == alternateViewName);

                sb.AppendLine($"        {{ typeof({viewModelName}), typeof({s}) }}, ");
            }
            else
            {
                sb.AppendLine($"        {{ typeof({viewModelName}), null }}, ");
            }
        }

        sb.AppendLine("    };");
        sb.AppendLine();
        sb.AppendLine("    public Control Build(object? data)");
        sb.AppendLine("    {");
        sb.AppendLine();
        sb.AppendLine("        if (data == null)");
        sb.AppendLine("            return new TextBlock { Text = \"Not Found: ViewModel is null\" };");
        sb.AppendLine();
        sb.AppendLine("        string viewModelTypeName = data.GetType().Name; ");
        sb.AppendLine();
        sb.AppendLine("        if (!ViewModelsViewsDictionary.TryGetValue(data.GetType(), out var viewType))");
        sb.AppendLine("            return new TextBlock { Text = $\"Not Found: {viewModelTypeName} has no mapped View for ViewModel {viewModelTypeName} \" };");
        sb.AppendLine();
        sb.AppendLine("        if (viewType == null)");
        sb.AppendLine("            return new TextBlock { Text = $\"Not Found: {viewModelTypeName} has no mapped View for ViewModel {viewModelTypeName} \" };");
        sb.AppendLine();
        sb.AppendLine("        return (Ioc.Default.GetService(viewType) as Control ?? throw new Exception($\"Registered View {viewType.Name} for ViewModel {viewModelTypeName} is not an Control \"))");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public bool Match(object? data) => data is ObservableObject  ;");
        sb.AppendLine("}");
        sb.AppendLine();

        return sb.ToString();
    }

    private sealed class ClassCollector : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = [];

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.AttributeLists.Count > 0)
            {
                CandidateClasses.Add(classDeclaration);
            }
        }
    }
}

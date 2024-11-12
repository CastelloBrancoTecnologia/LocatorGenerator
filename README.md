# CastelloBranco.LocatorGenerator

**CastelloBranco.LocatorGenerator** is a tool designed to automatically generate the necessary code to locate and instantiate ViewModels marked with a special attribute in WPF, Avalonia, WinUI, UWP, and UNO projects.

## Usage

`[RegisterOnLocatorAttribute]` is used to mark ViewModel classes for inclusion in the locator.

### Code Generator

This tool scans projects for classes with the `RegisterOnLocatorAttribute` and generates a `ViewModelLocator.g.cs` file to resolve and inject dependencies automatically using the CommunityToolkit.Mvvm library.

A standalone MSBuild tool was chosen instead of a source generator because the C# code needs to be ready and available to XAML before compilation. Since XAML processing in these frameworks occurs before a Source Generator executes, the locator class would not be available for binding, leading to compilation errors.

## Project Structure

- **LocatorGenerator.exe**: The generator tool that loads and analyzes a user-specified MSBuild project, identifies classes decorated with `[RegisterOnLocatorAttribute]`, and generates a `ViewModelLocator.g.cs` file. This file exposes each identified ViewModel as a static property, enabling automatic dependency injection.

### Setup

1. **Build**: Compile the `LocatorGeneratorTool` project to obtain the executable.

2. **NuGet Package**: Add the `CastelloBranco.LocatorGenerator` package to the project that will use the generated locator.

3. **MSBuild Task**: In the project where you will use the generator, add an MSBuild task to call the executable, using the following syntax:
   
   Run `LocatorGeneratorTool.exe [SolutionPath] [ProjectFile] [OutputDirectory]`

   ```xml
   <Target Name="PreBuild" BeforeTargets="PreBuildEvent">
     <Exec Command="C:\LocatorGenerator\LocatorGenerator.exe $(ProjectDir) $(ProjectName).csproj Generated" />
   </Target>
   ```

## Output

After execution, a `ViewModelLocator.g.cs` file will be generated in the specified directory (in this example, `Generated\`), containing static properties for each detected ViewModel.

The generated `ViewModelLocator.g.cs` file will have a structure similar to the following:

```csharp
public partial class ViewModelLocator 
{
    public ViewModelLocator() { }

    public static namespace.ViewModel1? ViewModel1 => Ioc.Default.GetRequiredService<namespace.ViewModel1>(); 
    public static namespace.ViewModel2? ViewModel2 => Ioc.Default.GetRequiredService<namespace.ViewModel2>(); 
    // etc.
}
```

### Registering a ViewModel Class

To register a class as a locatable ViewModel, mark it with `[RegisterOnLocator]`:

```csharp
[RegisterOnLocator]
public class MyViewModel { }
```

## License

MIT

## Contribution

Contributions are welcome! Please follow the pull request process and review our contribution policy.
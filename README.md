# CastelloBranco.LocatorGenerator

**CastelloBranco.LocatorGenerator** is a source generator designed to automatically generate the necessary code to locate and instantiate ViewModels marked with a special attribute in Avalonia projects.

## Usage

Inports nuget Package AutoRegisterInject and decorate yours views and viewmodels with  

[RegisterTransient]
[RegisterSingleton]
[RegisterScoped]
[RegisterHostedService]

### Code Generator

This tool scans projects for classes and generates a `ViewLocator.g.cs` file to resolve and inject dependencies automatically using the CommunityToolkit.Mvvm library.

### Setup

**NuGet Package**: 

    1) Add nuget packeges to the project that will use the generated locator.
       Microsoft.Extensions.DependencyInjection;
	   CommunityToolkit.Mvvm 
	   AutoRegisterInject 

    2) Add the `CastelloBranco.LocatorGenerator` package to the project that will use the generated locator.
  
## Usage

1) Decorate yours views and viewmodels with  

[RegisterTransient]
[RegisterSingleton]
[RegisterScoped]
[RegisterHostedService]

2) in xaml files add the following namespaces and AutoWrireViewModel.Enabled="True" 

```xml

<UserControl 
    ...
    xmlns:l="using:your_root_namespace"
    xmlns:views="using:your_Views_namespace_"
    xmlns:vm="using:your_ViewModels_namespace_"
    l:AutoWireViewModel.Enabled="True"
    x:DataType="vm:MainViewViewModel"
    >
```

3) Add the following code to the App.xaml file

```xml

<Application ...
             xmlns:l="using:your_root_namespace"
             ... >
    <Application.DataTemplates>
        <l:ViewLocator />
    </Application.DataTemplates>

```

## License

MIT

## Contribution

Contributions are welcome! Please follow the pull request process and review our contribution policy.
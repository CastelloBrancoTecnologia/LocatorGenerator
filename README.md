#CastelloBranco.LocatorGenerator

CastelloBranco.LocatorGenerator é uma ferramenta para gerar automaticamente o código necessário para localizar e instanciar ViewModels marcados com um atributo especial em projetos WPF, Avalonia, WinUI, UWP e UNO.

#Uso:

[RegisterOnLocatorAttribute] é usado para marcar classes ViewModel para inclusão no localizador.

Gerador de Código: Uma ferramenta que analisa projetos em busca de classes com o atributo RegisterOnLocatorAttribute e gera um arquivo ViewModelLocator.g.cs para resolver e injetar dependências automaticamente usando a biblioteca CommunityToolkit.Mvvm.
Estrutura do Projeto

LocatorGenerator.exe: O gerador que carrega e analisa um projeto MSBuild fornecido pelo usuário, identifica classes decoradas com [RegisterOnLocatorAttribute] e gera um arquivo ViewModelLocator.g.cs que expõe cada ViewModel identificado como uma propriedade estática, permitindo a injeção de dependência automática.

1) Build: Compile o projeto LocatorGeneratorTool para obter o executavel

2) NuGet Package: Adicione o pacote CastelloBranco.LocatorGenerator ao projeto que utilizará o localizador gerado.

3) No projeto que utilizara o gerador adicione uma tarefa MSBUILD chamando o executavel onde: 

   execute LocatorGeneratorTool.exe [Caminho da Solução] [Arquivo de Projeto] [Diretório de Saída]

``` xml

<Target Name="PreBuild" BeforeTargets="PreBuildEvent">
  <Exec Command="C:\LocatorGenerator\LocatorGenerator.exe $(ProjectDir) $(ProjectName).csproj Generated" />
</Target>

```

#Saída: 

Após a execução, um arquivo ViewModelLocator.g.cs será gerado no diretório especificado (neste exemplo, Generated\), contendo propriedades estáticas para cada ViewModel detectado.

O arquivo ViewModelLocator.g.cs gerado terá uma estrutura semelhante a:

``` csharp

public partial class ViewModelLocator 
{
    public ViewModelLocator() { }

    public static namespace.ViewModel1? ViewModel1 => Ioc.Default.GetRequiredService<namespace.ViewModel1>(); 
    public static namespace.ViewModel2? ViewModel2 => Ioc.Default.GetRequiredService<namespace.ViewModel2>(); 
    // etc.
}

```

Para registrar uma classe como um ViewModel localizável:

``` csharp

[RegisterOnLocator]
public class MeuViewModel { }

````

#Licença: 

MIT

#Contribuição:

Contribuições são bem-vindas! Siga o fluxo de pull requests e confira nossa política de contribuições.


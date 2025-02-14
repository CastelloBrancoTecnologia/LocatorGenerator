using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace CastelloBranco.LocatorGeneratorTest;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterSingleton]
public class FooViewModel : ObservableObject
{
    public void Bar()
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterTransient]
public class BarViewModel : ObservableObject
{
    public void Foo()
    {                                                                       
        Console.WriteLine("Hello World!");
    }
}

[RegisterSingleton]
public class FooView
{
    public void Bar()
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterTransient]
public class BarView
{
    public void Foo()
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterTransient]
public class ZoomView
{
    public void Foo()
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterTransient]
public class ZoomViewViewModel : ObservableObject
{
    public void Foo()
    {
        Console.WriteLine("Hello World!");
    }
}

[RegisterSingleton]
public class BlaBlaViewModel : ObservableObject
{
    public void Foo()
    {
        Console.WriteLine("Hello World!");
    }
}






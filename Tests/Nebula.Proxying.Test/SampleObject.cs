using System.Reflection;
using Nebula.Proxying.Utilities;


namespace Nebula.Proxying.Test;

public class SampleObject : IProxiedObject
{
    public string GetEchoStringBody(string text) => text;

    [Utilities.ProxyEntrance("GetEchoStringBody")]
    public string GetEchoString(string text)
    {
        return (string)_proxies.GetMethodProxy(GetType().GetMethod(nameof(GetEchoString))!)!
            .Invoke(text)!;
    }

    public int NumberBody { get; set; }
    
    [Utilities.ProxyEntrance("NumberBody")]
    public int Number {
        get => (int)_proxies.GetPropertyProxy(GetType().GetProperty(nameof(Number))!)!.Get()!;
        set => _proxies.GetPropertyProxy(GetType().GetProperty(nameof(Number))!)!.Set(value);
    }

    public int Tick { get; set; }
    
    public int AddTickBody(int number)
    {
        Tick += number;
        return Tick;
    }
    
    [Utilities.ProxyEntrance("AddTickBody")]
    public int AddTick(int number)
    {
        return (int)_proxies.GetMethodProxy(GetType().GetMethod(nameof(AddTick))!)!
            .Invoke(number)!;
    }

    private readonly ProxyManager _proxies = new();
    
    public SampleObject()
    {
        _proxies.ScanObject(this);
    }

    public IMethodProxy? GetMethodProxy(MethodInfo method) => _proxies.GetMethodProxy(method);

    public IPropertyProxy? GetPropertyProxy(PropertyInfo property) => _proxies.GetPropertyProxy(property);
}
using Nebula.Proxying;

namespace Nebula.Prism;

public static class Refraction
{
    public static IProxiedObject GetProxy<TClass>() where TClass : class
        => GetProxy(typeof(TClass));

    public static IProxiedObject GetProxy(Type type)
    {
        throw new NotImplementedException();
    }
}
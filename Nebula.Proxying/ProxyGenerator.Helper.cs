namespace Nebula.Proxying;

public static class ProxyGeneratorHelper
{
    /// <summary>
    /// Acquire the proxy class of the given type.
    /// </summary>
    /// <typeparam name="TClass">Type to get proxy with.</typeparam>
    /// <returns>Proxy class type.</returns>
    public static Type GetProxy<TClass>(this ProxyGenerator generator) where TClass : class
    {
        return generator.GetProxy(typeof(TClass));
    }
}
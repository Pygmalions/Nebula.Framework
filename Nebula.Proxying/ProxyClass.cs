namespace Nebula.Proxying;

/// <summary>
/// Singleton access interface for <see cref="ProxyGenerator"/>.
/// </summary>
public static class ProxyClass
{
    private static readonly Lazy<ProxyGenerator> SingletonInstance = 
        new Lazy<ProxyGenerator>(() => new ProxyGenerator());

    /// <summary>
    /// Use the singleton instance to get the proxy of a proxied type.
    /// </summary>
    /// <param name="proxiedType">Type to get or generate proxy class of.</param>
    /// <returns>Proxy class.</returns>
    /// <seealso cref="ProxyGenerator.GetProxy"/>
    public static Type Get(Type proxiedType)
        => SingletonInstance.Value.GetProxy(proxiedType);

    /// <summary>
    /// Use the singleton instance to get the proxy of a proxied type.
    /// </summary>
    /// <typeparam name="TClass">Type to get or generate proxy class of.</typeparam>
    /// <returns>Proxy class.</returns>
    /// <seealso cref="ProxyGeneratorHelper.GetProxy{TClass}"/>
    public static Type Get<TClass>() where TClass : class
        => SingletonInstance.Value.GetProxy<TClass>();
}
using System.Reflection;

namespace Nebula.Proxying;

/// <summary>
/// Interface for objects which holds and provides proxy entries.
/// </summary>
public interface IProxyObject
{
    /// <summary>
    /// Acquire the corresponding proxy entry of a specific method.
    /// </summary>
    /// <param name="proxiedMethod">The proxy of this method will be queried.</param>
    /// <returns>Proxy entry of the given method.</returns>
    ProxyEntry? GetProxy(MethodInfo proxiedMethod);
}
using System.Collections.Concurrent;
using System.Reflection;

namespace Nebula.Proxying;

/// <summary>
/// A proxy manager provides basic querying, adding, and removing functions for proxies management.
/// </summary>
public class ProxyManager
{
    /// <summary>
    /// Registered proxies.
    /// </summary>
    private readonly ConcurrentDictionary<MethodInfo, Proxy> _proxies = new();

    /// <summary>
    /// Acquire the proxy of the corresponding method.
    /// </summary>
    /// <param name="proxiedMethod">Proxied method of which to get proxy.</param>
    /// <returns>Proxy of the given method, or null if no matching proxy found.</returns>
    public Proxy? GetProxy(MethodInfo proxiedMethod)
    {
        return _proxies.TryGetValue(proxiedMethod, out var proxy) ? proxy : null;
    }

    /// <summary>
    /// Add a proxy to this manager.
    /// </summary>
    /// <param name="proxiedMethod">Proxied method to which the proxy belongs.</param>
    /// <param name="proxy">Proxy to add.</param>
    public void AddProxy(MethodInfo proxiedMethod, Proxy proxy)
    {
        _proxies.TryAdd(proxiedMethod, proxy);
    }

    /// <summary>
    /// Remove a proxy from this manager.
    /// </summary>
    /// <param name="proxiedMethod">Proxied method of which the proxy will be removed.</param>
    public void RemoveProxy(MethodInfo proxiedMethod)
    {
        _proxies.TryRemove(proxiedMethod, out _);
    }
}
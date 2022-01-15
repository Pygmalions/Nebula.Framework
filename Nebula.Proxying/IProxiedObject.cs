using System.Reflection;

namespace Nebula.Proxying;

/// <summary>
/// The minimal interface to get method and property proxies of an object.
/// </summary>
public interface IProxiedObject
{
    /// <summary>
    /// Get the proxy of the given method.
    /// </summary>
    /// <param name="method">Method of which to get proxy.</param>
    /// <returns>Method proxy, or null if the given method has no proxy.</returns>
    IMethodProxy? GetMethodProxy(MethodInfo method);
    
    /// <summary>
    /// Get the proxy of the given method.
    /// </summary>
    /// <param name="property">Method of which to get proxy.</param>
    /// <returns>Method proxy, or null if the given method has no proxy.</returns>
    IPropertyProxy? GetPropertyProxy(PropertyInfo property);
}
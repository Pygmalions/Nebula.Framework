using System.Collections.Concurrent;
using System.Reflection;

namespace Nebula.Proxying;

public class ProxyManager : IProxiedObject
{ 
    private readonly ConcurrentDictionary<MethodInfo, IMethodProxy> _methodProxies = new();
    private readonly ConcurrentDictionary<string, IPropertyProxy> _propertyProxies = new();

    /// <summary>
    /// Add a method proxy to this manager.
    /// </summary>
    /// <param name="method">Proxy for which to add.</param>
    /// <param name="proxy">Proxy to add.</param>
    public void AddMethodProxy(MethodInfo method, IMethodProxy proxy)
        => _methodProxies[method] = proxy;

    /// <summary>
    /// Remove the proxy of the given method.
    /// </summary>
    /// <param name="method">Proxy for which to remove.</param>
    public void RemoveMethodProxy(MethodInfo method)
        => _methodProxies.TryRemove(method, out _);
    
    /// <summary>
    /// Add a property proxy to this manager.
    /// </summary>
    /// <param name="property">Property for which proxy is added.</param>
    /// <param name="proxy">Proxy to add.</param>
    public void AddPropertyProxy(PropertyInfo property, IPropertyProxy proxy)
        => _propertyProxies[property.Name] = proxy;
    
    /// <summary>
    /// Remove the proxy of the given property.
    /// </summary>
    /// <param name="property">Proxy for which to remove.</param>
    public void RemovePropertyProxy(PropertyInfo property)
        => _propertyProxies.TryRemove(property.Name, out _);

    /// <inheritdoc />
    public IMethodProxy? GetMethodProxy(MethodInfo method)
        => _methodProxies.TryGetValue(method, out var proxy) ? proxy : null;

    /// <inheritdoc />
    public IPropertyProxy? GetPropertyProxy(PropertyInfo property)
        => _propertyProxies.TryGetValue(property.Name, out var proxy) ? proxy : null;
    
    public IProxy? this [MemberInfo member]
    {
        get
        {
            return member switch
            {
                MethodInfo method => GetMethodProxy(method),
                PropertyInfo property => GetPropertyProxy(property),
                _ => throw new Exception("Proxy manager can only manage proxies of method and property " +
                                         $"rather than {member.MemberType}")
            };
        }
        set
        {
            switch (member)
            {
                case MethodInfo method:
                    if (value is not IMethodProxy methodProxy)
                        throw new Exception("Only objects of IMethodProxy can be registered to a method.");
                    AddMethodProxy(method, methodProxy);
                    break;
                case PropertyInfo property:
                    if (value is not IPropertyProxy propertyProxy)
                        throw new Exception("Only objects of IPropertyProxy can be registered to a property.");
                    AddPropertyProxy(property, propertyProxy);
                    break;
                default:
                    throw new Exception("Proxy manager can only manage proxies of method and property " +
                                        $"rather than {member.MemberType}");
            }
        }
    }
}
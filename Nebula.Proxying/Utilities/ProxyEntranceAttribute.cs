using System.Reflection;

namespace Nebula.Proxying.Utilities;

/// <summary>
/// This attribute marks on the methods or properties which is the 'entrance' to the method which do the real work,
/// by calling <see cref="DecoratedMethod"/> or <see cref="DecoratedProperty"/>.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Property)]
public class ProxyEntranceAttribute : Attribute
{
    /// <summary>
    /// The method or property which the new created proxy will be redirected to.
    /// </summary>
    public readonly string? Destination;
    
    /// <summary>
    /// This attribute will let the <see cref="ProxyManager"/> create a proxy to the destination,
    /// but under the entry of the method or property where this proxy is marked.
    /// </summary>
    /// <param name="destination"><inheritdoc cref="Destination"/>></param>
    public ProxyEntranceAttribute(string? destination = null)
    {
        Destination = destination;
    }
}

public static class ProxyEntranceHelper
{
    /// <summary>
    /// Scan and create proxies of <see cref="DecoratedMethod"/> and <see cref="DecoratedProperty"/>
    /// for an object with methods and properties marked with <see cref="ProxyEntranceAttribute"/>.
    /// </summary>
    /// <param name="manager">Manager to add proxies.</param>
    /// <param name="holder">Object to scan.</param>
    public static void ScanObject(this ProxyManager manager, object holder)
    {
        var holderType = holder.GetType();

        foreach (var method in holderType.GetMethods(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var attribute = method.GetCustomAttribute<ProxyEntranceAttribute>();
            switch (attribute)
            {
                case null:
                    continue;
                case { Destination: not null } methodAttribute:
                {
                    var destination = holderType.GetMethod(methodAttribute.Destination);
                    if (destination == null)
                        throw new Exception("Failed to create redirection proxy:" +
                                            $"Type {holderType} has no method named {methodAttribute.Destination}.");
                    manager.AddMethodProxy(method, new DecoratedMethod(holder, destination));
                    break;
                }
                default:
                    manager.AddMethodProxy(method, new DecoratedMethod(holder, method));
                    break;
            }
        }
        
        foreach (var property in holderType.GetProperties(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var attribute = property.GetCustomAttribute<ProxyEntranceAttribute>();
            switch (attribute)
            {
                case null:
                    continue;
                case { Destination: not null }:
                {
                    var destination = holderType.GetProperty(attribute.Destination);
                    if (destination == null)
                        throw new Exception("Failed to create redirection proxy:" +
                                            $"Type {holderType} has no property named {attribute.Destination}.");
                    manager.AddPropertyProxy(property, new DecoratedProperty(holder, destination));
                    break;
                }
                default:
                    manager.AddPropertyProxy(property, new DecoratedProperty(holder, property));
                    break;
            }
        }
    }
}
using System.Reflection;

namespace Nebula.Proxying.Utilities;

/// <summary>
/// This attribute marks on the methods or properties which is the 'entrance' to the method which do the real work,
/// by calling <see cref="ExtensibleMethod"/> or <see cref="ExtensibleProperty"/>.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Property)]
public class ProxyEntranceAttribute : ProxyAttribute
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
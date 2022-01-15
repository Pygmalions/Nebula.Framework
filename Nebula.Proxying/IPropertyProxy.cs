using System.Reflection;

namespace Nebula.Proxying;

public interface IPropertyProxy : IProxy
{
    /// <summary>
    /// Reflection information of the proxied property.
    /// </summary>
    PropertyInfo ProxiedProperty { get; }

    /// <summary>
    /// Get the value of the proxied proxy.
    /// </summary>
    /// <returns>Value gotten from this proxy.</returns>
    object? Get();
    
    /// <summary>
    /// Set the value of this proxied proxy.
    /// </summary>
    /// <param name="value">Value to set.</param>
    void Set(object? value);
}
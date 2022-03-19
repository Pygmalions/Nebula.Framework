using System;

namespace Nebula.Core;

/// <summary>
/// Attribute for service providers.
/// Service providers marked with this attribute can be automatically found by domains.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ServerAttribute : Attribute
{
    /// <summary>
    /// Mark this class as a server (a.k.a. service provider).
    /// </summary>
    /// <param name="service">Type of the service provided by this provider.</param>
    public ServerAttribute(Type service)
    {
        Service = service;
    }

    /// <summary>
    /// Reflection type of the service provided by this provider.
    /// </summary>
    public Type Service { get; }

    /// <summary>
    /// Description of this service provider.
    /// </summary>
    public string Description { get; set; } = "";
}
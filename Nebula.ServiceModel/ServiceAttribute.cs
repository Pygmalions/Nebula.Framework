using System;
using Nebula.Resource;

namespace Nebula.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class ServiceAttribute : Attribute
{
    public ServiceAttribute(Scopes location, Type? identity = null)
    {
        Location = location;
        Identity = identity;
    }

    /// <summary>
    /// Declares the default location preference to search and instantiate this service.
    /// Instance -> This service does not have a related provider.
    /// Program -> This service is provided by a provider in this program.
    /// Host -> This service is provided by a provider on this host.
    /// Workspace -> This service is provided by a provider in this workspace.
    /// </summary>
    public Scopes Location { get; }

    /// <summary>
    /// Token for the service to use.
    /// If null, the service will be registered as itself,
    /// otherwise it will be registered as the given service type.
    /// This token must be a class type inherited from service.
    /// </summary>
    public Type? Identity { get; }

    /// <summary>
    /// Description of this service.
    /// </summary>
    public string Description { get; set; } = "";
}
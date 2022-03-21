using Nebula.Resource;

namespace Nebula.Injecting;

/// <summary>
/// Members marked with this attribute will be auto discovered and injected by <see cref="Injector"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | 
                AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
public class InjectionAttribute : Attribute
{
    /// <summary>
    /// Name to locate the resource to inject.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Scopes to search for the injection.
    /// </summary>
    public Scopes Scopes { get; }

    /// <summary>
    /// Mark this member should use the specific identifier from the specific scopes to locate the resource to inject.
    /// </summary>
    /// <param name="scopes">Scopes to search in.</param>
    public InjectionAttribute(Scopes scopes = Scopes.Any)
    {
        Scopes = scopes;
    }
}
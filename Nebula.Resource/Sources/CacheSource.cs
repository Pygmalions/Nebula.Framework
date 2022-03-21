namespace Nebula.Resource.Sources;

/// <summary>
/// This container can by operated outsides itself, and only provides cache resource management.
/// </summary>
public class CacheSource : Source
{
    /// <summary>
    /// This source will not provide anything.
    /// </summary>
    public override object? Acquire(Type type, IIdentifier identifier) => null;

    /// <summary>
    /// Set the cache for an resource entry.
    /// </summary>
    /// <param name="scope">Scope to declare this resource in.</param>
    /// <param name="type">Type category of the resource.</param>
    /// <param name="identifier">Identifier of the resource.</param>
    /// <param name="value">Cached resource object.</param>
    public void SetCache(Scope scope, Type type, IIdentifier identifier, object value)
    {
        Declare(scope, type, identifier, value);
    }

    /// <summary>
    /// Set the cache for an resource entry.
    /// </summary>
    /// <typeparam name="TType">Type category of the resource.</typeparam>
    /// <param name="scope">Scope to declare this resource in.</param>
    /// <param name="identifier">Identifier to locate the resource.</param>
    /// <param name="value">Cached resource object.</param>
    public void SetCache<TType>(Scope scope, IIdentifier identifier, object value)
        => SetCache(scope, typeof(TType), identifier, value);

    /// <summary>
    /// Revoke a specific resource cache entry.
    /// The whole resource entry will be removed.
    /// </summary>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="type">Type category of the resource.</param>
    /// <param name="identifier">Identifier to locate the resource.</param>
    public void UnsetCache(Scope scope, Type type, IIdentifier identifier)
    {
        Revoke(scope, type, identifier);
    }
    
    /// <summary>
    /// Revoke a specific resource cache entry.
    /// The whole resource entry will be removed.
    /// </summary>
    /// <typeparam name="TType">Type category of the resource.</typeparam>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="identifier">Identifier to locate the resource.</param>
    public void UnsetCache<TType>(Scope scope, IIdentifier identifier)
        => UnsetCache(scope, typeof(TType), identifier);
}
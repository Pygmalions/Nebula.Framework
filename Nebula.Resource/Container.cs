using System.Collections.Concurrent;
using System.Net;
using Nebula.Exceptions;

namespace Nebula.Resource;

public partial class Container
{
    /// <summary>
    /// Lookup table of (Scope -> Type -> Identifier -> Declaration)
    /// </summary>
    private ConcurrentDictionary<Scope, ConcurrentDictionary<Type, ConcurrentDictionary<IIdentifier, Source>>>
        _declarations = new();

    /// <summary>
    /// Lookup table of (Source -> Declaration -> Declaration Information)
    /// </summary>
    private ConcurrentDictionary<Source, ConcurrentDictionary<Declaration, Scope>> 
        _sources = new();

    /// <summary>
    /// Acquire an object according to the given category and identifier from the specified scopes.
    /// </summary>
    /// <param name="type">Type category to looking into.</param>
    /// <param name="identifier">Identifier to locate the object.</param>
    /// <param name="scopes">Scopes to search in.</param>
    /// <returns>Object instance, or null if no matching resource found.</returns>
    public object? Acquire(Type type, IIdentifier identifier, Scopes scopes = Scopes.Any)
    {
        var scanningScopes = scopes.Split();
        
        object? instance = null;
        foreach (var scanningScope in scanningScopes)
        {
            if (!_declarations.TryGetValue(
                    scanningScope, out var categories))
                continue;
            if (!categories.TryGetValue(type, out var declarations))
                continue;
            if (!declarations.TryGetValue(identifier, out var provider))
                continue;
            instance = provider.Acquire(type, identifier);
            if (instance != null)
                break;
        }

        return instance;
    }

    /// <summary>
    /// Acquire an object according to the given category and identifier from the specified scopes,
    /// and try to convert into the type of the category.
    /// </summary>
    /// <typeparam name="TType">Type category to looking into.</typeparam>
    /// <param name="identifier">Identifier to locate the object.</param>
    /// <param name="scopes">Scopes to search in.</param>
    /// <returns>
    /// Object instance.
    /// If null, it means that no matching resource found or failed to convert into <typeparamref name="TType"/>.
    /// </returns>
    public TType? Acquire<TType>(IIdentifier identifier, Scopes scopes = Scopes.Any) where TType : class
        => Acquire(typeof(TType), identifier, scopes) as TType;

    /// <summary>
    /// Declare a resource.
    /// </summary>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="type">Type category of the resource.</param>
    /// <param name="identifier">Identifier of the resource.</param>
    /// <param name="provider">Source which declare this resource.</param>
    internal void DeclareResource(Scope scope, Type type, IIdentifier identifier, Source provider)
    {
        if (!_sources.TryGetValue(provider, out var resources))
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                "Failed to declare resource: the provide is not installed to this container.");
            return;
        }

        var layer = _declarations.GetOrAdd(scope, 
            _ => new ConcurrentDictionary<Type, ConcurrentDictionary<IIdentifier, Source>>());
        var category = layer.GetOrAdd(type, 
            _ => new ConcurrentDictionary<IIdentifier, Source>());
        if (!category.TryAdd(identifier, provider))
        {
            ErrorCenter.Report<RuntimeError>(Importance.Warning,
                "Failed to declare an resource: resource already declared.");
        }
        resources.TryAdd(new Declaration(type, identifier, scope), scope);
    }

    /// <summary>
    /// Revoke a resource declaration.
    /// </summary>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="type">Type category of the resource.</param>
    /// <param name="identifier">Identifier of the resource.</param>
    /// <param name="provider">Source which wants to revoke this resource.</param>
    internal void RevokeResource(Scope scope, Type type, IIdentifier identifier, Source provider)
    {
        if (!_sources.TryGetValue(provider, out var resources))
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                "Failed to revoke resource: the provide is not installed to this container.");
            return;
        }

        if (!_declarations.TryGetValue(scope, out var layer))
            return;
        if (!layer.TryGetValue(type, out var category))
            return;
        if (!category.TryGetValue(identifier, out var currentProvider))
            return;
        resources.TryRemove(new Declaration(type, identifier, scope), out _);
        if (currentProvider != provider)
        {
            ErrorCenter.Report<RuntimeError>(Importance.Warning,
                "Failed to revoke resource declaration: the resource is not provided by this source.");
            return;
        }
        category.TryRemove(identifier, out _);
    }

    /// <summary>
    /// Add a source to this container.
    /// </summary>
    /// <param name="source">Source to add.</param>
    /// <exception cref="UserError">
    /// Throw if this source has already been added to this container.
    /// </exception>
    public void AddSource(Source source)
    {

        if (_sources.ContainsKey(source))
        {
            ErrorCenter.Report<UserError>(Importance.Warning, 
                "Failed to add a source: it has already been added to this container.");
            return;
        }
        _sources.TryAdd(source, new ConcurrentDictionary<Declaration, Scope>());
        source.Install(this);
    }

    /// <summary>
    /// Remove a source from this container.
    /// </summary>
    /// <param name="source">Source to remove.</param>
    /// <exception cref="UserError">
    /// Throw if this source has not been removed from this container.
    /// </exception>
    public void RemoveSource(Source source)
    {
        if (!_sources.TryRemove(source, out var declarations))
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                "Failed to remove a source: it has not been added to this container.");
            return;
        }
        source.Uninstall();

        Parallel.ForEach(declarations, pair =>
        {
            RevokeResource(pair.Value, pair.Key.Category, pair.Key.Identifier, source);
        });
    }
}
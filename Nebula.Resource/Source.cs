using Nebula.Exceptions;

namespace Nebula.Resource;

public abstract class Source
{
    private Container? _container;

    internal void Install(Container container)
    {
        if (_container != null)
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                "A source can not be added to more than one container.");
            return;
        }
        _container = container;
        OnInstall(container);
    }

    internal void Uninstall()
    {
        _container = null;
        OnUninstall();
    }

    public abstract object? Acquire(Type type, IIdentifier identifier);

    /// <summary>
    /// Declare a resource in the bound container.
    /// </summary>
    /// <param name="scope">Scope of this resource.</param>
    /// <param name="type">Type category of this resource.</param>
    /// <param name="identifier">Identifier to locate this resource.</param>
    /// <param name="cache">Cached object to add to the resource entry.</param>
    /// <exception cref="UserError">
    /// Throw if this source has not been installed by <see cref="Container.AddSource"/>.
    /// </exception>
    protected void Declare(Scope scope, Type type, IIdentifier identifier, object? cache = null)
    {
        if (_container == null)
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                $"Source {GetType().Name} can not declare resource before it is installed.");
            return;
        }
        _container.DeclareResource(scope, type, identifier, this, cache);
    }

    /// <summary>
    /// Declare a resource in the bound container.
    /// </summary>
    /// <typeparam name="TType">Type category of this resource.</typeparam>
    /// <param name="scope">Scope of this resource.</param>
    /// <param name="identifier">Identifier to locate this resource.</param>
    /// <param name="cache">Cached object to add to the resource entry.</param>
    protected void Declare<TType>(Scope scope, IIdentifier identifier, object? cache = null) 
        => Declare(scope, typeof(TType), identifier, cache);

    /// <summary>
    /// Revoke an resource declaration.
    /// </summary>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="type">Type category of the resource.</param>
    /// <param name="identifier">Identifier to locate the resource.</param>
    /// <exception cref="UserError">
    /// Throw if the source has not been installed.
    /// </exception>
    protected void Revoke(Scope scope, Type type, IIdentifier identifier)
    {
        if (_container == null)
        {
            ErrorCenter.Report<UserError>(Importance.Warning,
                $"Source {GetType().Name} can not revoke resource before it is installed.");
            return;
        }
        _container.RevokeResource(scope, type, identifier, this);
    }
    
    /// <summary>
    /// Revoke an resource declaration.
    /// </summary>
    /// <typeparam name="TType">Type category of the resource.</typeparam>
    /// <param name="scope">Scope of the resource.</param>
    /// <param name="identifier">Identifier to locate the resource.</param>
    /// <exception cref="UserError">
    /// Throw if the source has not been installed.
    /// </exception>
    protected void Revoke<TType>(Scope scope, IIdentifier identifier) => Revoke(scope, typeof(TType), identifier);
    
    /// <summary>
    /// Overrides this method to do installing work.
    /// </summary>
    /// <param name="container">Container where this source is added to.</param>
    protected virtual void OnInstall(Container container)
    {}

    /// <summary>
    /// Overrides this method to do uninstalling work.
    /// Attention, container will automatically <b>revoke</b> resources declared by this source.
    /// </summary>
    protected virtual void OnUninstall()
    {}
}
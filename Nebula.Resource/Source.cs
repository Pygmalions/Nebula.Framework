using Nebula.Exceptions;

namespace Nebula.Resource;

public abstract class Source
{
    private Container? _container;

    internal void Install(Container container)
    {
        if (_container != null)
            throw new UserError("A source can not be added to more than one container.");
        _container = container;
    }

    internal void Uninstall()
    {
        _container = null;
    }

    public abstract object? Acquire(Type type, IIdentifier identifier);

    /// <summary>
    /// Declare a resource in the bound container.
    /// </summary>
    /// <param name="scope">Scope of this resource.</param>
    /// <param name="type">Type category of this resource.</param>
    /// <param name="identifier">Identifier to locate this resource.</param>
    /// <exception cref="UserError">
    /// Throw if this source has not been installed by <see cref="Container.AddSource"/>.
    /// </exception>
    protected void Declare(Scope scope, Type type, IIdentifier identifier)
    {
        if (_container == null)
            throw new UserError($"Source {GetType().Name} can not declare resource before it is installed.");
        _container.DeclareResource(scope, type, identifier, this);
    }

    protected void Declare<TType>(Scope scope, IIdentifier identifier) => Declare(scope, typeof(TType), identifier);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="type"></param>
    /// <param name="identifier"></param>
    /// <exception cref="UserError"></exception>
    protected void Revoke(Scope scope, Type type, IIdentifier identifier)
    {
        if (_container == null)
            throw new UserError($"Source {GetType().Name} can not revoke resource before it is installed.");
        _container.RevokeResource(scope, type, identifier, this);
    }
    
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
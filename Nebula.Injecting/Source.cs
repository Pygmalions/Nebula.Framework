using Nebula.Reporting;

namespace Nebula.Injecting;

public abstract class Source
{
    protected Container? Container { get; private set; }
    
    /// <summary>
    /// Install this source to a container.
    /// </summary>
    /// <param name="container">Container to install to.</param>
    internal void Install(Container container)
    {
        if (Container != null)
        {
            Report.Warning("Failed to Install", "The source has been installed.", this)
                .AttachDetails("TargetContainer", container)
                .AttachDetails("CurrentContainer", container)
                .Handle();
            return;
        }
            
        Container = container;
        OnInstall();
    }

    /// <summary>
    /// Uninstall this source from the container.
    /// </summary>
    internal void Uninstall()
    {
        if (Container == null)
        {
            Report.Warning("Failed to Uninstall", "The source has not been installed.", this)
                .Handle();
            return;
        }
        OnUninstall();
    }

    /// <summary>
    /// Invoked when this source is installed to a container.
    /// Protected property <see cref="Container"/> is already available.
    /// </summary>
    protected virtual void OnInstall() {}

    /// <summary>
    /// Invoked when this source is installed to a container.
    /// Protected property <see cref="Container"/> is still available until this method returns.
    /// </summary>
    protected virtual void OnUninstall() {}

    /// <summary>
    /// Overrides this method to implement the acquirement of the object.
    /// </summary>
    /// <param name="declaration">Declaration of the object.</param>
    /// <param name="type">Type of the acquired object.</param>
    /// <param name="name">Name of the acquired object.</param>
    /// <returns>Acquired object instance.</returns>
    protected internal abstract object? Get(Declaration declaration, Type type, string name = "");

    /// <summary>
    /// Declare an object in the bound container.
    /// </summary>
    /// <param name="type">Category type of the object to declare.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    /// <param name="accurate">
    /// If true, only declare the given type;
    /// if false, this method will also declare its base types as temporary (type of object is not included.).
    /// If the given type is a interface, then this option is ignored and considered as true.
    /// </param>
    /// <param name="singleton">If accurate is false, then this one must be specified.</param>
    /// <returns>Declaration instance, or null if this invocation failed.</returns>
    protected Declaration? Declare(Type type, string name = "",
        bool trying = false, bool accurate = true, bool singleton = true)
    {
        if (Container != null)
        {
            if (accurate || type.IsInterface)
                return Container.Declare(this, type, name, trying)?.SetSingleton(singleton);
            for (var baseType = type.BaseType;
                 baseType != null && baseType != typeof(object);
                 baseType = baseType.BaseType)
            {
                Container.Declare(this, baseType, name, true)
                    ?.SetSingleton(singleton).SetTemporary(true);
            }
            return Container.Declare(this, type, name, trying)?.SetSingleton(singleton);
        }
           
        Report.Warning("Failed to Declare", "The source has not been installed.", this)
            .AttachDetails("Category", type)
            .AttachDetails("Name", name)
            .Handle();
        return null;
    }

    /// <summary>
    /// Declare an object in the bound container.
    /// </summary>
    /// <typeparam name="TObject">Category type of the object to declare.</typeparam>
    /// <param name="name">Optional name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    /// <returns>Declaration instance, or null if this invocation failed.</returns>
    protected Declaration? Declare<TObject>(string name = "", bool trying = false)
        => Declare(typeof(TObject), name, trying);
    
    /// <summary>
    /// Revoke an object declaration in the bound container.
    /// </summary>
    /// <param name="type">Category type of the object to revoke.</param>
    /// <param name="name">Name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    protected void Revoke(Type type, string name = "", bool trying = false)
    {
        if (Container == null)
        {
            Report.Warning("Failed to Revoke", "The source has not been installed.", this)
                .AttachDetails("Category", type)
                .AttachDetails("Name", name)
                .Handle();
            return;
        }
        Container.Revoke(this, type, name, trying);
    }
}
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
            Report.Warning(new Exception(
                $"Failed to install {GetType()} to {container}: " +
                "The source has already been installed to a container."));
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
            Report.Warning(new Exception(
                $"Failed to uninstall {GetType()}: " +
                "The source has not been installed."));
            return;
        }
        OnUninstall();
    }

    /// <summary>
    /// Invoked when this source is installed to a container.
    /// Protected property <see cref="Container"/> is already available.
    /// </summary>
    protected abstract void OnInstall();

    /// <summary>
    /// Invoked when this source is installed to a container.
    /// Protected property <see cref="Container"/> is still available until this method returns.
    /// </summary>
    protected abstract void OnUninstall();

    /// <summary>
    /// Overrides this method to implement the acquirement of the object.
    /// </summary>
    /// <param name="declaration">Declaration of the object.</param>
    /// <param name="type">Type of the acquired object.</param>
    /// <param name="name">Name of the acquired object.</param>
    /// <returns>Acquired object instance.</returns>
    protected internal abstract object Get(Declaration declaration, Type type, string name = "");

    /// <summary>
    /// Declare an object in the bound container.
    /// </summary>
    /// <param name="type">Category type of the object to declare.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    /// <returns>Declaration instance, or null if this invocation failed.</returns>
    protected Declaration? Declare(Type type, string name = "", bool trying = false)
    {
        if (Container != null) 
            return Container.Declare(this, type, name, trying);
        Report.Warning(new Exception(
            $"Failed to declare an object: " +
            "The source has not been installed."));
        return null;
    }
    
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
            Report.Warning(new Exception(
                $"Failed to revoke a declaration: " +
                "The source has not been installed."));
            return;
        }
        Container.Revoke(this, type, name, trying);
    }
}
namespace Nebula.Injecting.Errors;

/// <summary>
/// This error is the base error of all errors that occurs in a container.
/// </summary>
public class ContainerError : Exception
{
    public Container Container { get; }

    public ContainerError(Container container, string message) : base(message)
    {
        Container = container;
    }
}
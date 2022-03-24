namespace Nebula.Injecting.Errors;

public class InvalidAccessError : ContainerError
{
    public InvalidAccessError(Container container, Source source, string message) :
        base(container, $"A source {source} failed to access container {container}: {message}")
    {}
}
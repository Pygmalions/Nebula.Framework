using Nebula.Injecting.Errors;

namespace Nebula.Injecting;

public class EntryError : ContainerError
{
    public Type Category { get; }
    public string? Name { get; }

    public EntryError(Container container, Type category, string name, string operation, string message) :
        base(container, $"Failed to {operation} an entry ({category}:{name}): {message}")
    {
        Category = category;
        Name = name;
    }
}
using Nebula.Core;
using Nebula.Resource;
using Nebula.Resource.Identifiers;

namespace SampleApplication;

[Source]
public class SampleSource : Source
{
    protected override void OnInstall(Container container)
    {
        Declare<object>(Scope.Instance, WildcardIdentifier.Anything);
    }

    public override object? Acquire(Type type, IIdentifier identifier)
    {
        Console.WriteLine($"Redirected: {type.Name}.");
        
        return this;
    }
}
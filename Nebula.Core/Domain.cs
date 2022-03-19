using Nebula.Resource;

namespace Nebula.Core;

public class Domain : Container
{
    private static readonly Lazy<Domain> SingletonInstance =
        new(() => new Domain(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static Domain Current => SingletonInstance.Value;
    
    private Domain()
    {}
}
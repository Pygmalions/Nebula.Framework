using System.Reflection;
using Nebula.Exceptions;
using Nebula.Extending;
using Nebula.Resource;

[assembly: PluginAssembly]

namespace Nebula.Core;

public class Domain : Container
{
    private static readonly Lazy<Domain> SingletonInstance =
        new(() => new Domain(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static Domain Current => SingletonInstance.Value;

    private Domain()
    {
        // In-assembly source auto discovery.
        foreach (var (_, assembly) in PluginRegistry.FoundPlugins)
        {
            foreach (var candidate in assembly.GetTypes())
            {
                if (candidate.IsSubclassOf(typeof(Source)))
                    continue;
                var sourceAttribute = candidate.GetCustomAttribute<SourceAttribute>();
                if (sourceAttribute == null)
                    continue;
                if (candidate.GetConstructor(Type.EmptyTypes) == null)
                    throw new UserError($"Source {candidate.Name} is marked with {nameof(SourceAttribute)} " +
                                        $"but does not have a no-argument constructor.");
                var source = Activator.CreateInstance(candidate) as Source;
                if (source == null)
                    throw new RuntimeError(
                        $"Failed to construct {candidate.Name} in in-assembly source auto discovery.");
                AddSource(source);
            }
        }
    }
}
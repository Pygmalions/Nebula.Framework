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

    /// <summary>
    /// Singleton instance of the current domain.
    /// </summary>
    public static Domain Current => SingletonInstance.Value;

    private readonly Task _initialization;
    
    private Domain()
    {
        _initialization = Task.Run(Initialize);
    }

    /// <summary>
    /// Initialize the domain. This will only work once in <see cref="_initialization"/>.
    /// </summary>
    /// <exception cref="RuntimeError"></exception>
    private void Initialize()
    {
        if (_initialization.IsCompleted) return;
        
        // In-assembly source auto discovery.
        foreach (var (_, assembly) in PluginRegistry.FoundPlugins)
        {
            foreach (var candidate in assembly.GetTypes())
            {
                if (!candidate.IsSubclassOf(typeof(Source)))
                    continue;
                var sourceAttribute = candidate.GetCustomAttribute<SourceAttribute>();
                if (sourceAttribute == null)
                    continue;
                if (candidate.GetConstructor(Type.EmptyTypes) == null)
                    ErrorCenter.Report<UserError>(Importance.Warning, 
                        $"Source {candidate.Name} is marked with {nameof(SourceAttribute)} " +
                        "but does not have a no-argument constructor.");
                else if (Activator.CreateInstance(candidate) is not Source source)
                    ErrorCenter.Report<RuntimeError>(Importance.Warning,
                        $"Failed to instantiate discovered source {candidate.Name}.");
                else AddSource(source);
            }
        }
    }
    
    public void Launch()
    {
        _initialization.Wait();
    }
}
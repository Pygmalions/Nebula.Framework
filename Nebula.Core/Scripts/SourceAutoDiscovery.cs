using System.Reflection;
using Nebula.Exceptions;
using Nebula.Extending;
using Nebula.Resource;

namespace Nebula.Core.Scripts;

[DomainScript("Initialize")]
public class SourceAutoDiscovery : IDomainScript
{
    public void Execute(string trigger)
    {
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
                if (Activator.CreateInstance(candidate) is not Source source)
                {
                    ErrorCenter.Report<RuntimeError>(Importance.Warning,
                        $"Failed to instantiate discovered source {candidate.Name}.");
                    continue;
                }
                Domain.Current.AddSource(source);
            }
        }
    }
}
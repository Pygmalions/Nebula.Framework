using System.Reflection;
using Nebula.Exceptions;
using Nebula.Extending;
using Nebula.Resource;

[assembly: PluginAssembly]

namespace Nebula.Core;

public partial class Domain : Container
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
    private void Initialize()
    {
        if (_initialization.IsCompleted) return;
        
        // In-assembly domain script auto discovery.
        foreach (var (_, assembly) in PluginRegistry.FoundPlugins)
        {
            foreach (var candidate in assembly.GetTypes())
            {
                if (!candidate.IsAssignableTo(typeof(IDomainScript)))
                    continue;
                var sourceAttribute = candidate.GetCustomAttribute<DomainScriptAttribute>();
                if (sourceAttribute == null)
                    continue;
                if (candidate.GetConstructor(Type.EmptyTypes) == null)
                    ErrorCenter.Report<UserError>(Importance.Warning, 
                        $"Domain script {candidate.Name} is marked " +
                        $"with {nameof(DomainScriptAttribute)} but does not have a no-argument constructor.");
                if (Activator.CreateInstance(candidate) is not IDomainScript script)
                {
                    ErrorCenter.Report<RuntimeError>(Importance.Warning,
                        $"Failed to instantiate discovered domain script {candidate.Name}.");
                    continue;
                }
                AddScript(script);
            }
        }
        
        TriggerScript("Initialize");
    }
    
    /// <summary>
    /// Launch the current program domain.
    /// </summary>
    public void Launch()
    {
        _initialization.Wait();
        
        TriggerScript("Launch");
        
        TriggerScript("Finish");
    }
}
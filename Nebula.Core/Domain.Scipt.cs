using System.Collections.Concurrent;

namespace Nebula.Core;

public partial class Domain
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, IDomainScript>> _scripts = new();

    public void AddScript(IDomainScript script, params string[] triggers)
    {
        foreach (var trigger in triggers)
        {
            var group = _scripts.GetOrAdd(trigger, 
                _ => new ConcurrentDictionary<Type, IDomainScript>());
            group.TryAdd(script.GetType(), script);
        }
    }

    public void RemoveScript(Type scriptType, params string[] triggers)
    {
        foreach (var trigger in triggers)
        {
            if (!_scripts.TryGetValue(trigger, out var group))
                continue;
            group.TryRemove(scriptType, out _);
        }
    }
    
    /// <summary>
    /// Trigger the scripts with the given trigger name.
    /// </summary>
    /// <param name="trigger">Trigger name.</param>
    public void TriggerScript(string trigger)
    {
        if (!_scripts.TryGetValue(trigger, out var scripts))
            return;
        foreach (var (_, script) in scripts)
        {
            script.Execute(trigger);
        }
    }
}
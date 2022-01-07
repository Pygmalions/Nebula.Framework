using System.Collections.Concurrent;

namespace Nebula.Injection.Rules;

public class SingletonPolicy : Policy
{
    private static readonly ConcurrentDictionary<Type, object> Instances = new();
    
    private readonly Type _boundType;

    public SingletonPolicy(Type boundType, IRule rule) : base(rule)
    {
        _boundType = boundType;
    }
    
    public override object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        if (Instances.TryGetValue(type, out var instance))
            return instance;
        instance = Rule.Get(source, type, attribute);
        Instances.TryAdd(type, instance);
        return instance;
    }

    public override bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        return Instances.ContainsKey(type) || Rule.Acceptable(source, type, attribute);
    }
}
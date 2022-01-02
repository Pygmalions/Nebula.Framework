namespace Nebula.Injection.Rules;

public class ScopePolicy : Policy
{
    private readonly object _instanceMutex = new();
    private object? _instance;
    
    public ScopePolicy(IRule rule) : base(rule)
    {}

    public override object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        lock (_instanceMutex)
        {
            if (_instance != null) 
                return _instance;
            _instance = Rule.Get(source, type, attribute);
            return _instance;
        }
    }

    public override bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        lock (_instanceMutex)
        {
            return _instance != null || Rule.Acceptable(source, type, attribute);
        }
    }
}
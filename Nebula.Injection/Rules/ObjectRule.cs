namespace Nebula.Injection.Rules;

/// <summary>
/// This rule will return the bound object instance.
/// </summary>
public class ObjectRule : IRule
{
    private readonly object _instance;
    
    public ObjectRule(object instance)
    {
        _instance = instance;
    }

    public object Get(ISource source, Type type, InjectionAttribute? attribute)
        => _instance;

    public bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
        => true;
}
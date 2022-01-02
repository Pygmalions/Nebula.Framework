namespace Nebula.Injection.Rules;

/// <summary>
/// Abstract for all policies.
/// This class is designed for check whether a class of IRule is a rule or a policy (rule wrapper).
/// </summary>
public abstract class Policy : IRule
{
    public readonly IRule Rule;
    
    protected Policy(IRule rule)
    {
        Rule = rule;
    }

    public abstract object Get(ISource source, Type type, InjectionAttribute? attribute);
    public abstract bool Acceptable(ISource source, Type type, InjectionAttribute? attribute);
}
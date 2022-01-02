namespace Nebula.Injection.Rules;

public abstract class Ruleset : IRule
{
    public abstract object Get(ISource source, Type type, InjectionAttribute? attribute);
    public abstract bool Acceptable(ISource source, Type type, InjectionAttribute? attribute);
}
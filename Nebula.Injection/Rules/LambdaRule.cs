using System.Reflection;

namespace Nebula.Injection.Rules;

public class LambdaRule : IRule
{
    private readonly Func<ISource, Type, InjectionAttribute?, object> _creator;
    private readonly Func<ISource, Type, InjectionAttribute?, bool> _checker;
    
    public LambdaRule(Func<ISource, Type, InjectionAttribute?, object> creator,
        Func<ISource, Type, InjectionAttribute?, bool>? checker = null)
    {
        _creator = creator;
        _checker = checker ?? ((_, _, _) => true);
    }
    
    /// <inheritdoc />
    public object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        return _creator(source, type, attribute);
    }

    /// <inheritdoc />
    public bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        return _checker(source, type, attribute);
    }
}
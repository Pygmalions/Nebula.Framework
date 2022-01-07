namespace Nebula.Injection.Rules;

/// <summary>
/// This is the simplest rule,
/// which can only bind classes with one default constructor
/// (the constructor without any parameters),
/// and not support any post-operations that <see cref="BindingRule"/> can do.
/// </summary>
/// <typeparam name="TType">Type of class to bind.</typeparam>
public class SimpleRule<TType> : IRule where TType : class, new()
{
    private static readonly Type TypeInformation = typeof(TType);
    
    public object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        return new TType();
    }

    public bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        return TypeInformation.IsAssignableTo(type);
    }
}
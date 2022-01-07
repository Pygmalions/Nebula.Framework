namespace Nebula.Injection;

public static class SourceHelper
{
    public static TType Acquire<TType>(this ISource source, InjectionAttribute? attribute = null) 
        where TType : class
    {
        if (source.Acquire(typeof(TType), attribute) is not TType instance)
            throw new InvalidCastException(
                "Can not cast the acquired instance form the source into the required to type.");
        return instance;
    }
    
    public static bool Acquirable<TType>(this ISource source, InjectionAttribute? attribute = null) 
        where TType : class
    {
        return source.Acquirable(typeof(TType), attribute);
    }
}
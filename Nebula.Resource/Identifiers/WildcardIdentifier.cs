namespace Nebula.Resource.Identifiers;

public class WildcardIdentifier : IIdentifier
{
    public static readonly WildcardIdentifier Anything = new();
    
    protected WildcardIdentifier()
    {}
}
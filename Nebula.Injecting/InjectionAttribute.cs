namespace Nebula.Injecting;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | 
                AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
public class InjectionAttribute : Attribute
{
    public readonly string? Name;

    public InjectionAttribute(string? name = null)
    {
        Name = name;
    }
}
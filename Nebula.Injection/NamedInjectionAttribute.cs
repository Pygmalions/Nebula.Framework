namespace Nebula.Injection;

[AttributeUsage(validOn: AttributeTargets.Parameter |
                         AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class NamedInjectionAttribute : InjectionAttribute
{
    public readonly string Name;

    public NamedInjectionAttribute(string name)
    {
        Name = name;
    }
}
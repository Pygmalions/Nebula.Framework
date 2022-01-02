namespace Nebula.Injection;

[AttributeUsage(validOn: AttributeTargets.Constructor |
                         AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class InjectionAttribute : Attribute
{
    
}
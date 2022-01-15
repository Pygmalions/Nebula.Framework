namespace Nebula.Prism;

/// <summary>
/// Generator will automatically create refractions for method or property marked with this attribute.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Property)]
public class RefractionAttribute : Attribute
{
    
}
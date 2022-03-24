namespace Nebula.Injecting;

/// <summary>
/// Member marked with this attribute will enable passive injection on them.
/// <para>
///     Parameters of a method or constructor can be marked with this attribute to indicates the name of the
///     injection object. But it is not necessary if it only wants to match the type of the injection object.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |  AttributeTargets.Parameter |
                AttributeTargets.Method  | AttributeTargets.Constructor)]
public class InjectionAttribute : Attribute
{
    public string Name { get; }

    public InjectionAttribute(string? name = null)
    {
        Name = name ?? "";
    }
}
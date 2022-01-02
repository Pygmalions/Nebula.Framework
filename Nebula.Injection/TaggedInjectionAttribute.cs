namespace Nebula.Injection;

[AttributeUsage(validOn: AttributeTargets.Parameter |
                         AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
public class TaggedInjectionAttribute : InjectionAttribute
{
    public readonly IReadOnlySet<string> Tags;

    public TaggedInjectionAttribute(params string[] tags)
    {
        Tags = new HashSet<string>(tags);
    }
}
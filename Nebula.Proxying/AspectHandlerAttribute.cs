using Nebula.Reporting;

namespace Nebula.Proxying;

/// <summary>
/// Classes marked with this attribute will be automatically loaded and applied to proxied objects
/// by the <see cref="ProxyGenerator" />.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AspectHandlerAttribute : Attribute
{
    /// <summary>
    /// Mark the types of trigger attributes that should trigger the aspect handler.
    /// </summary>
    /// <param name="triggerAttributes">
    /// Types of attributes derived from <see cref="AspectTrigger" /> which will trigger this aspect handler.
    /// </param>
    public AspectHandlerAttribute(params Type[] triggerAttributes)
    {
        var baseAttributeType = typeof(AspectTrigger);
        var availableTriggerAttribute = new HashSet<Type>();
        foreach (var attributeType in triggerAttributes)
        {
            if (attributeType.IsSubclassOf(baseAttributeType))
            {
                availableTriggerAttribute.Add(attributeType);
                continue;
            }
            Report.Warning("Invalid Aspect",
                "Attribute for an aspect handler to handle is not derived " +
                $"from the {nameof(AspectTrigger)}, " +
                "thus this handler will never be triggered by this attribute.")
                .AttachDetails("Attribute", attributeType)
                .Handle();
        }

        TriggerAttributes = availableTriggerAttribute;
    }

    public IReadOnlySet<Type> TriggerAttributes { get; }
}
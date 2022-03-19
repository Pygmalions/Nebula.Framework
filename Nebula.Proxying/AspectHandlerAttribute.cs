﻿using Nebula.Exceptions;

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
    /// <exception cref="UserError"></exception>
    public AspectHandlerAttribute(params Type[] triggerAttributes)
    {
        var baseAttributeType = typeof(Attribute);

        foreach (var attributeType in triggerAttributes)
            if (!attributeType.IsSubclassOf(baseAttributeType))
                throw new UserError($"Attribute {attributeType.Name} " +
                                    $"marked in the {nameof(AspectHandlerAttribute)} " +
                                    "is not an attribute type.");

        TriggerAttributes = new HashSet<Type>(triggerAttributes);
    }

    public IReadOnlySet<Type> TriggerAttributes { get; }
}
using System.Reflection;

namespace Nebula.Attributing;

/// <summary>
/// The abstract class which has a script to apply the .
/// </summary>
public abstract class ScriptAttribute : Attribute
{
    /// <summary>
    /// Apply this script to an object on member of which this attribute is marked.
    /// </summary>
    /// <param name="holderObject">Object which holds this attribute.</param>
    /// <param name="holderMember">Reflection information of the member which this attribute is marked.</param>
    public abstract void Apply(object holderObject, MemberInfo holderMember);

    /// <summary>
    /// Apply this script to an object on type of which this attribute is marked.
    /// </summary>
    /// <param name="holderObject">Object which holds this attribute.</param>
    /// <param name="holderType">Reflection information of the type which this attribute is marked.</param>
    public abstract void Apply(object holderObject, Type holderType);
}
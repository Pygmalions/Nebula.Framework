using System.Reflection;
using Nebula.Exceptions;
using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

/// <summary>
/// This preset indicates to locate a member in the current domain.
/// </summary>
[Preset("Location")]
public class MemberLocationPreset : IItem<MemberInfo>
{
    /// <summary>
    /// Name of the member.
    /// </summary>
    [Property("Name")]
    public IItem<string> Name { get; }
    
    /// <summary>
    /// Type which holds this member.
    /// </summary>
    [Property("Class")]
    public IItem<Type> Class { get; }
    
    /// <summary>
    /// Construct a member preset.
    /// </summary>
    /// <param name="classType">Type which holds the member.</param>
    /// <param name="memberName">Name of the member to find.</param>
    public MemberLocationPreset(IItem<Type> classType, IItem<string> memberName)
    {
        Class = classType;
        Name = memberName;
    }

    public MemberInfo Translate()
    {
        // C# does not allow multiple members to have the same name.
        var member = Class.Translate().GetMember(Name.Translate());
        if (member == null || member.Length == 0)
            throw new UserError($"Failed to find a member of {Class.Translate().Name} named {Name.Translate()}");
        return member[0];
    }
}
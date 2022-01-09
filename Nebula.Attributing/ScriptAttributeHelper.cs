using System.Reflection;

namespace Nebula.Attributing;

public static class ScriptAttributeHelper
{
    /// <summary>
    /// Apply all script attributes of the given object.
    /// This method will scan all <see cref="ScriptAttribute"/> marked on the class and members of the given object,
    /// and then apply them on the given object.
    /// </summary>
    /// <param name="holder">Holder to scan and apply.</param>
    /// <param name="includePrivate">Whether scan and apply attributes marked on private members or not.</param>
    public static void ApplyScriptAttribute(this object holder, bool includePrivate = false)
    {
        ApplyClassScriptAttribute(holder);
        ApplyMemberScriptAttribute(holder, includePrivate);
    }

    /// <summary>
    /// Apply all script attributes marked on the class of the given object.
    /// This method will scan all <see cref="ScriptAttribute"/> marked on the class of the given object,
    /// and then apply them on the given object.
    /// </summary>
    /// <param name="holder">Holder to scan and apply.</param>
    public static void ApplyClassScriptAttribute(this object holder)
    {
        var holderType = holder.GetType();
        foreach (var typeAttribute in holderType.GetCustomAttributes<ScriptAttribute>())
        {
            typeAttribute.Apply(holder, holderType);
        }
    }

    /// <summary>
    /// Apply all script attributes marked on the type of the given object.
    /// This method will scan all <see cref="ScriptAttribute"/> marked on the members of the given object,
    /// and then apply them on the given object.
    /// </summary>
    /// <param name="holder">Holder to scan and apply.</param>
    /// /// <param name="includePrivate">Whether scan and apply attributes marked on private members or not.</param>
    public static void ApplyMemberScriptAttribute(this object holder, bool includePrivate = false)
    {
        var holderType = holder.GetType();
        MemberInfo[]? members;
        if (includePrivate)
        {
            members = holderType.GetMembers(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
        }
        else
        {
            members = holderType.GetMembers(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
        }
        foreach (var member in members)
        {
            foreach (var memberAttribute in member.GetCustomAttributes<ScriptAttribute>())
            {
                memberAttribute.Apply(holder, member);
            }
        }
    }
}
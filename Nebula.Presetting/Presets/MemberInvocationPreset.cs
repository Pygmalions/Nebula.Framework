using System.Reflection;
using Nebula.Exceptions;
using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

/// <summary>
/// This preset will invoke a member and indicates to use the returning value of the invocation.
/// It will do different behaviors based the type of the member.
/// <list type="table">
///     <listheader>
///         <term>Member Type</term>
///         <description>Behavior</description>
///     </listheader>
///     <item>
///         <term>Property</term>
///         <description>Try to get value.</description>
///     </item>
///     <item>
///         <term>Field</term>
///         <description>Try to get value.</description>
///     </item>
///     <item>
///         <term>Method</term>
///         <description>Invoke without arguments and returns the returning value.</description>
///     </item>
///     <item>
///         <term>Event</term>
///         <description>Raise the event, and returns null.</description>
///     </item>
/// </list>
/// </summary>
[Preset("Invocation")]
public class MemberInvocationPreset : IItem<object?>
{
    [Property("Member")]
    public IItem<MemberInfo> Member;

    [Property("Object")]
    public IItem<object?>? Holder;

    [Content]
    public IArray<object?>? Arguments;

    /// <summary>
    /// Define a invocation on the given member.
    /// </summary>
    /// <param name="member">Member to invoke.</param>
    /// <param name="holder">Holder to invoke this member on. Ignore this if the member is static.</param>
    /// <param name="arguments">Arguments to pass to the member.</param>
    public MemberInvocationPreset(
        IItem<MemberInfo> member, IItem<object?>? holder = null, IArray<object?>? arguments = null)
    {
        Member = member;
        Holder = holder ?? new DirectItem<object?>(null);
        Arguments = arguments ?? new DirectArray<object?>();
    }
    
    public object? Translate()
    {
        switch (Member.Translate())
        {
            case EventInfo eventMember:
                return eventMember.RaiseMethod?.Invoke(Holder?.Translate(), Arguments?.Translate());
            case FieldInfo fieldMember:
                return fieldMember.GetValue(Holder?.Translate());
            case MethodInfo methodMember:
                return methodMember.Invoke(Holder?.Translate(), Arguments?.Translate().ToArray());
            case PropertyInfo propertyMember:
                return propertyMember.GetGetMethod()?.Invoke(Holder?.Translate(), Arguments?.Translate());
            default:
                return ErrorCenter.Report<UserError>(Importance.Error,
                    $"Invalid member preset. Can not find member " +
                    $"{Member.Translate().Name} in {Member.Translate().Name}.");
        }
    }
}
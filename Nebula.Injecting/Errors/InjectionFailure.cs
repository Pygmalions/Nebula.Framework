using System.Reflection;

namespace Nebula.Injecting.Errors;

/// <summary>
/// This exception is thrown when an injection fails.
/// </summary>
public class InjectionFailure : Exception
{
    /// <summary>
    /// Class of which member should be injected.
    /// </summary>
    public readonly Type Class;

    /// <summary>
    /// Name of the member to inject.
    /// </summary>
    public readonly string Member;

    /// <summary>
    /// Additional message to describe why this injection failed.
    /// </summary>
    public readonly string Reason;

    /// <summary>
    /// Construct this exception with the name of the member to inject.
    /// </summary>
    /// <param name="type">Type of the class.</param>
    /// <param name="member">Name of the member.</param>
    /// <param name="message">Additional message.</param>
    public InjectionFailure(Type type, string member, string message) :
        base($"Failed to inject {member}@{type}: {message}.")
    {
        Class = type;
        Member = member;
        Reason = message;
    }
    
    /// <summary>
    /// Construct this exception with the member to inject.
    /// </summary>
    /// <param name="type">Type of the class.</param>
    /// <param name="member">Member to inject. Its name will be recorded.</param>
    /// <param name="message">Additional message.</param>
    public InjectionFailure(Type type, MemberInfo member, string message) :
        this(type, member.Name, message)
    {}
}
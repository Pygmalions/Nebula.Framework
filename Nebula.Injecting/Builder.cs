using System.Reflection;

namespace Nebula.Injecting;

/// <summary>
/// This static class only defines the commonly used delegate types.
/// </summary>
public static class Builder
{ 
    /// <summary>
    /// Delegate to build an object with the optional injection information.
    /// </summary>
    public delegate object Item(MemberInfo? member, object? holder);
    /// <summary>
    /// Delegate to build an array with the optional injection information.
    /// </summary>
    public delegate object?[] Array(MemberInfo? member, object? holder);
}
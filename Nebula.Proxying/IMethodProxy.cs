using System.Reflection;

namespace Nebula.Proxying;

public interface IMethodProxy : IProxy
{
    /// <summary>
    /// Reflection information of the proxied method.
    /// </summary>
    MethodInfo ProxiedMethod { get; }

    /// <summary>
    /// Invoke this proxied method.
    /// </summary>
    /// <param name="arguments">Arguments to pass to the method.</param>
    /// <returns>Return value of this invocation.</returns>
    object? Invoke(params object?[] arguments);
}
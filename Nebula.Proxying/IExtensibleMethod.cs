using System.Reflection;

namespace Nebula.Proxying;

public interface IExtensibleMethod : IMethodProxy
{
    /// <summary>
    /// Event triggered before the proxied method is invoked.
    /// </summary>
    event Action<InvocationContext> Invoking;

    /// <summary>
    /// Event triggered after the proxied method is invoked.
    /// </summary>
    event Action<InvocationContext> Invoked;

    /// <summary>
    /// Whether the proxied method accept null value as a return or not.
    /// </summary>
    bool NullReturnAccepted { get; }
}
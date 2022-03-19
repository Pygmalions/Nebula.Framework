using System.Reflection;

namespace Nebula.Proxying;

public class Proxy
{
    /// <summary>
    /// Registered handlers for event <see cref="Invoked" />.
    /// </summary>
    private readonly List<Action<Context>> _invokedHandlers = new();

    /// <summary>
    /// Registered handlers for event <see cref="Invoking" />.
    /// </summary>
    private readonly List<Action<Context>> _invokingHandlers = new();

    /// <summary>
    /// Construct a proxy for the given method of the given object.
    /// </summary>
    /// <param name="proxiedObject">
    /// Object to which this proxy belongs. Be null if the proxied method is static.
    /// </param>
    /// <param name="proxiedMethod">Reflection information of the proxied method.</param>
    public Proxy(object proxiedObject, MethodInfo proxiedMethod, PropertyInfo? associatedProperty = null)
    {
        ProxiedObject = proxiedObject;
        ProxiedMethod = proxiedMethod;
        AssociatedProperty = associatedProperty;

        Invoker = context => { ProxiedMethod.Invoke(ProxiedObject, context.Arguments); };
    }

    /// <summary>
    /// Object to which this proxy belongs.
    /// </summary>
    public object ProxiedObject { get; }

    /// <summary>
    /// Reflection information of the proxied method.
    /// </summary>
    public MethodInfo ProxiedMethod { get; }

    public PropertyInfo? AssociatedProperty { get; }

    /// <summary>
    /// Delegate action used to invoke the proxied method.
    /// </summary>
    public Action<Context> Invoker { get; set; }

    /// <summary>
    /// Action triggered before the <see cref="Invoker" /> is invoked.
    /// </summary>
    public event Action<Context> Invoking
    {
        add => _invokingHandlers.Add(value);
        remove => _invokingHandlers.Remove(value);
    }

    /// <summary>
    /// Action triggered after the <see cref="Invoker" /> is invoked.
    /// </summary>
    public event Action<Context> Invoked
    {
        add => _invokedHandlers.Add(value);
        remove => _invokedHandlers.Remove(value);
    }

    /// <summary>
    /// Invoke this proxy.
    /// </summary>
    /// <param name="parameters">Parameters for this invocation.</param>
    public object? Invoke(object?[] parameters)
    {
        var context = new Context(ProxiedObject, ProxiedMethod, parameters);

        foreach (var invokingHandler in _invokingHandlers)
        {
            invokingHandler(context);
            if (context.ThrowingException != null)
                throw context.ThrowingException;
            if (context.InvokerStopping)
                return context.Result;
        }

        if (!context.InvokerSkipping)
            Invoker.Invoke(context);
        if (context.ThrowingException != null)
            throw context.ThrowingException;

        foreach (var invokedHandler in _invokedHandlers)
        {
            invokedHandler(context);
            if (context.ThrowingException != null)
                throw context.ThrowingException;
        }

        return context.Result;
    }
}
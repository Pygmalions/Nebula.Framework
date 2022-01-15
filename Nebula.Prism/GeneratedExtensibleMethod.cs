using System.Reflection;
using Nebula.Proxying;

namespace Nebula.Prism;

internal class GeneratedExtensibleMethod : IExtensibleMethod
{
    /// <inheritdoc />
    public event Action<InvocationContext> Invoking
    {
        add => _invokingHandlers.Add(value);
        remove => _invokingHandlers.Remove(value);
    }
    /// <inheritdoc />
    public event Action<InvocationContext> Invoked
    {
        add => _invokedHandlers.Add(value);
        remove => _invokedHandlers.Remove(value);
    }
    
    /// <inheritdoc />
    public object ProxiedHolder { get; }
    /// <inheritdoc />
    public MethodInfo ProxiedMethod { get; }
    
    /// <inheritdoc />
    public bool NullReturnAccepted { get; }

    /// <summary>
    /// Set of registered handlers for pre-process event.
    /// </summary>
    private readonly HashSet<Action<InvocationContext>> _invokingHandlers = new();
    /// <summary>
    /// Set of registered handlers for post-process event.
    /// </summary>
    private readonly HashSet<Action<InvocationContext>> _invokedHandlers = new();

    public GeneratedExtensibleMethod(object holder, MethodInfo method)
    {
        ProxiedHolder = holder;
        ProxiedMethod = method;

        NullReturnAccepted = ProxiedMethod.ReturnType == typeof(void) ||
                             ProxiedMethod.ReturnType.IsGenericType &&
                             ProxiedMethod.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public InvocationContext AcquireContext(object?[] arguments)
    {
        return new InvocationContext(this, arguments);
    }

    public object? ReleaseContext(InvocationContext context)
    {
        return context.ReturningValue;
    }

    public void TriggerInvokingEvent(InvocationContext context)
    {
        Parallel.ForEach(_invokingHandlers, action => action.Invoke(context));
        if (context.ThrowingException != null)
            throw context.ThrowingException;
        if (context.Interrupted && context.ReturningValue == null && !NullReturnAccepted)
            throw new Exception("Proxied method invocation interrupted without acceptable return value.");
        if (context.Skipped && context.ReturningValue == null && !NullReturnAccepted)
            throw new Exception("Proxied method invocation skipped without acceptable return value.");
    }

    public void TriggerInvokedEvent(InvocationContext context)
    {
        Parallel.ForEach(_invokedHandlers, action => action.Invoke(context));

        if (context.ThrowingException != null)
            throw context.ThrowingException;
    }
    
    public object? Invoke(params object?[] arguments)
    {
        var context = AcquireContext(arguments);
        
        Parallel.ForEach(_invokingHandlers, action => action.Invoke(context));
        if (context.ThrowingException != null)
            throw context.ThrowingException;
        
        if (context.Interrupted)
        {
            if (context.ReturningValue == null && !NullReturnAccepted)
                throw new Exception("Proxied method invocation interrupted without acceptable return value.");
            return context.ReturningValue;
        }
        if (!context.Skipped)
            context.ReturningValue = ProxiedMethod.Invoke(ProxiedHolder, arguments);
        else if (context.ReturningValue == null && !NullReturnAccepted)
            throw new Exception("Proxied method invocation skipped without acceptable return value.");
        
        Parallel.ForEach(_invokedHandlers, action => action.Invoke(context));

        if (context.ThrowingException != null)
            throw context.ThrowingException;
        
        return ReleaseContext(context);
    }
}
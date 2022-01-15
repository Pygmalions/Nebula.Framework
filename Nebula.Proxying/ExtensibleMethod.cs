using System.Reflection;

namespace Nebula.Proxying;

public class ExtensibleMethod : IMethodProxy
{
    /// <summary>
    /// Event triggered before the proxied method is invoked.
    /// </summary>
    public event Action<InvocationContext> Invoking
    {
        add => _invokingHandlers.Add(value);
        remove => _invokingHandlers.Remove(value);
    }
    /// <summary>
    /// Event triggered after the proxied method is invoked.
    /// </summary>
    public event Action<InvocationContext> Invoked
    {
        add => _invokedHandlers.Add(value);
        remove => _invokedHandlers.Remove(value);
    }
    
    /// <summary>
    /// Object which holds the proxied method.
    /// </summary>
    public object ProxiedHolder { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public MethodInfo ProxiedMethod { get; }
    
    /// <summary>
    /// Whether the proxied method accept null value as a return or not.
    /// </summary>
    public bool NullReturnAccepted { get; }

    /// <summary>
    /// Set of registered handlers for pre-process event.
    /// </summary>
    private readonly HashSet<Action<InvocationContext>> _invokingHandlers = new();
    /// <summary>
    /// Set of registered handlers for post-process event.
    /// </summary>
    private readonly HashSet<Action<InvocationContext>> _invokedHandlers = new();

    public ExtensibleMethod(object holder, MethodInfo method)
    {
        ProxiedHolder = holder;
        ProxiedMethod = method;

        NullReturnAccepted = ProxiedMethod.ReturnType == typeof(void) ||
                             ProxiedMethod.ReturnType.IsGenericType &&
                             ProxiedMethod.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public object? Invoke(params object?[] arguments)
    {
        InvocationContext context = new(this, arguments);

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
        
        return context.ReturningValue;
    }
}
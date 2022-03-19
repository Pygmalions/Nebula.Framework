using System.Reflection;

namespace Nebula.Proxying;

public class Context
{
    /// <summary>
    /// Construct this invocation context with the given information.
    /// </summary>
    /// <param name="proxiedObject">Object to which the method belongs.</param>
    /// <param name="proxiedMethod">Reflection information of the proxied method.</param>
    /// <param name="parameters">
    /// Parameters for this invocation. Be null if the proxied method does not need any parameter.
    /// </param>
    public Context(object? proxiedObject, MethodInfo proxiedMethod, object?[]? parameters)
    {
        ProxiedObject = proxiedObject;
        ProxiedMethod = proxiedMethod;

        Arguments = parameters ?? Array.Empty<object?>();

        ThrowingException = null;

        ResultNullable = ProxiedMethod.ReturnType == typeof(void) ||
                         ProxiedMethod.ReturnType.IsGenericType &&
                         ProxiedMethod.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// The object to which the proxied method belongs to.
    /// Can be null if the proxied method is static.
    /// </summary>
    public object? ProxiedObject { get; }

    /// <summary>
    /// Reflection information of the proxied method.
    /// </summary>
    public MethodInfo ProxiedMethod { get; }

    /// <summary>
    /// The arguments for this invocation.
    /// </summary>
    public object?[] Arguments { get; }

    /// <summary>
    /// The returning value for this invocation.
    /// </summary>
    public object? Result { get; private set; }

    /// <summary>
    /// Indicate whether the returning value can be null or not.
    /// </summary>
    public bool ResultNullable { get; }

    /// <summary>
    /// Exception which is requiring to throw.
    /// </summary>
    public Exception? ThrowingException { get; private set; }

    /// <summary>
    /// Whether the invoker is required to be skipped.
    /// In that case, only the invoker will not be invoked,
    /// but the invoked event will still be triggered.
    /// Only effective in the invoking stage.
    /// </summary>
    public bool InvokerSkipping { get; private set; }

    /// <summary>
    /// Whether the invocation process is required for an early stop.
    /// In that case, both of the invoker and invoked event will not be executed.
    /// Only effective in the invoking stage.
    /// </summary>
    public bool InvokerStopping { get; private set; }

    /// <summary>
    /// Skip the invoker with a returning value.
    /// The <see cref="ProxyGenerator.Meta.ProxyMeta.Properties.Invoker" /> will not be invoked,
    /// but <see cref="Proxy.Invoked" /> event will still be triggered.
    /// Only effective in the invoking stage.
    /// </summary>
    /// <param name="result">Returning value for this invocation.</param>
    public void Skip(object? result)
    {
        Return(result);
        InvokerSkipping = true;
    }

    /// <summary>
    /// Skip the invoker with a returning value.
    /// Both of the <see cref="Proxy.Invoker" /> and <see cref="Proxy.Invoked" /> event
    /// will not be invoked.
    /// Only effective in the invoking stage.
    /// </summary>
    /// <param name="result">Returning value for this invocation.</param>
    public void Stop(object? result)
    {
        Return(result);
        InvokerStopping = true;
    }

    /// <summary>
    /// Set the returning value of this invocation.
    /// If no acceptable returning value set before the invocation ends, an exception will be thrown.
    /// </summary>
    /// <param name="result">Returning value for this invocation.</param>
    /// <exception cref="ArgumentNullException">Returning value is null and not acceptable.</exception>
    public void Return(object? result)
    {
        if (result == null && !ResultNullable)
            throw new ArgumentNullException(nameof(result),
                "Returning value of the proxied method is not nullable.");
        Result = result;
    }

    /// <summary>
    /// Stop the invocation process by throwing an exception.
    /// It does not matter whether the returning value is set or not.
    /// </summary>
    /// <param name="exception">Exception to throw.</param>
    public void Throw(Exception exception)
    {
        ThrowingException = exception;
    }
}
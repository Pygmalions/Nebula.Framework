using System.Reflection;

namespace Nebula.Proxying;

public class InvocationContext
{
    public ExtensibleMethod Proxy { get; }

    public object?[] Arguments { get; }

    private object? _returningValue;
    /// <summary>
    /// The value marked to return.
    /// </summary>
    public object? ReturningValue
    {
        get => _returningValue;
        set
        {
            if (value == null && !Proxy.NullReturnAccepted)
                throw new Exception("A handler try to return a null value while it is not acceptable.");
            _returningValue = value;
        }
    }
    
    /// <summary>
    /// The exception marked to throw.
    /// </summary>
    public Exception? ThrowingException { get; private set; }

    /// <summary>
    /// Whether this invocation is marked to stop.
    /// </summary>
    public bool Interrupted { get; private set; }
    
    /// <summary>
    /// Whether the invocation stage is marked to stop,
    /// while post-process stage will not be effected.
    /// </summary>
    public bool Skipped { get; private set; }
    
    public InvocationContext(ExtensibleMethod proxy, params object?[] arguments)
    {
        Proxy = proxy;
        Arguments = arguments;
    }

    /// <summary>
    /// Skip the invocation stage while post-process stage will not be effected.
    /// If the proxied method has a not nullable return type and the return value of this invocation is null,
    /// then an exception will be thrown.
    /// </summary>
    public void Skip()
    {
        Skipped = true;
    }

    /// <summary>
    /// Stop the invocation and post-process stages.
    /// This only works in the pre-process stage.
    /// </summary>
    public void Stop()
    {
        Interrupted = true;
    }

    /// <summary>
    /// Throw an exception after current stage.
    /// </summary>
    /// <param name="exception">Exception to throw.</param>
    public void Throw(Exception exception)
    {
        ThrowingException = exception;
    }
}
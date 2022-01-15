namespace Nebula.Proxying;

public class AccessContext
{
    public IExtensibleProperty Proxy { get; }

    private object? _accessingValue;

    /// <summary>
    /// Value of current property to 'get' or 'set'.
    /// </summary>
    /// <exception cref="Exception">
    /// Throw if this value is set to null when null is not acceptable.
    /// </exception>
    public object? AccessingValue
    {
        get => _accessingValue;
        set
        {
            if (value == null && !Proxy.NullValueAccepted)
                throw new Exception(
                    "A handler try to set the value of an access to null while it is not acceptable.");
            _accessingValue = value;
        }
    }

    /// <summary>
    /// The exception marked to throw.
    /// </summary>
    public Exception? ThrowingException { get; private set; }
    
    /// <summary>
    /// Whether this access is marked to stop.
    /// </summary>
    public bool Interrupted { get; private set; }
    
    /// <summary>
    /// Whether the access stage is marked to stop,
    /// while post-process stage will not be effected.
    /// </summary>
    public bool Skipped { get; private set; }
    
    public AccessContext(IExtensibleProperty proxy, object? value)
    {
        Proxy = proxy;
        _accessingValue = value;
    }
    
    /// <summary>
    /// Skip the access stage while post-process stage will not be effected.
    /// If the proxied method has a not nullable return type and the return value of this invocation is null,
    /// then an exception will be thrown.
    /// </summary>
    public void Skip()
    {
        Skipped = true;
    }

    /// <summary>
    /// Stop the access and post-process stages.
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
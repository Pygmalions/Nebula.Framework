namespace Nebula.Exceptions;

/// <summary>
/// Context of the error to handle. <br />
/// <para>
/// If a handler does not invoke either <see cref="Throw"/> or <see cref="Continue"/>, then it will be considered
/// as give up the vote right.
/// </para>
/// <para>
/// If any one handler invokes <see cref="Throw"/>, then the exception will be thrown.
/// </para>
/// <para>
/// If no handler invokes <see cref="Throw"/>, and any handler invokes <see cref="Continue"/>, the exception will
/// be ignored.
/// </para>
/// <para>
/// If all handlers invokes nothing, then the exception will be thrown by default.
/// </para>
/// </summary>
public class ErrorContext
{
    public bool ThrowRequired { get; private set; }
    
    public bool ContinueRequired { get; private set; }
    
    /// <summary>
    /// Stored original error to handle.
    /// Usually, it is recommended to use <see cref="Error"/> instead.
    /// Rather than this exception, <see cref="Error"/> will be thrown if needed.
    /// </summary>
    public Exception OriginalError { get; }
    
    /// <summary>
    /// Stored original level of the error.
    /// Usually, it is recommended to use <see cref="Level"/> instead.
    /// </summary>
    public Importance OriginalLevel { get; }
    
    /// <summary>
    /// Exception to handle.
    /// Replaceable.
    /// </summary>
    public Exception Error { get; set; }
    
    /// <summary>
    /// Importance level of the exception to handle.
    /// Editable.
    /// </summary>
    public Importance Level { get; set; }

    /// <summary>
    /// Construct a context with the given exception and importance.
    /// Both the exception and importance level can be modified.
    /// </summary>
    /// <param name="exception">Exception to handle.</param>
    /// <param name="importance">Importance level of the exception.</param>
    public ErrorContext(Exception exception, Importance importance)
    {
        OriginalError = exception;
        OriginalLevel = importance;
        Error = exception;
        Level = importance;
    }

    /// <summary>
    /// Require to continue to execute.
    /// Invoke this method if the handler consider this error as acceptable,
    /// and the execution can continue.
    /// <para>
    ///     By not invoking <see cref="Continue"/> and <see cref="Throw"/>, the handler can express that it does not
    ///     care whether the exception will be thrown or not.
    /// </para>
    /// </summary>
    public void Continue()
    {
        ContinueRequired = true;
    }

    /// <summary>
    /// Require to throw this exception.
    /// Invoke this method if the handler consider this error as unacceptable,
    /// and the execution must be interrupted.
    /// <para>
    ///     By not invoking <see cref="Continue"/> and <see cref="Throw"/>, the handler can express that it does not
    ///     care whether the exception will be thrown or not.
    /// </para>
    /// </summary>
    public void Throw()
    {
        ThrowRequired = true;
    }
}
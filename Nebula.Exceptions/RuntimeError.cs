namespace Nebula.Exceptions;

/// <summary>
/// Exceptions inherited from this exception are caused by the runtime errors and failures.
/// Usually they does not indicate that the users have errors in using the framework or modules,
/// but something unexpected happened outsides the framework.
/// </summary>
public class RuntimeError : Exception
{
    public RuntimeError()
    {
    }

    public RuntimeError(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
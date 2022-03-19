namespace Nebula.Exceptions;

/// <summary>
/// Exceptions inherited from this exception are caused by the wrong use or
/// violations to the standard made by users themselves,
/// which means that all of them can and should be fixed in the programming stage.
/// </summary>
public class UserError : Exception
{
    public UserError()
    {
    }

    public UserError(string message, Exception? innerException = null) : base(message, innerException)
    {
    }
}
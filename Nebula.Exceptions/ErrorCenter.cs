using System.Diagnostics.CodeAnalysis;

namespace Nebula.Exceptions;

/// <summary>
/// Singleton access interface for <see cref="ErrorHandlingCenter"/>.
/// </summary>
public static class ErrorCenter
{
    private static readonly Lazy<ErrorHandlingCenter> SingletonInstance = new(()=>new ErrorHandlingCenter());

    /// <summary>
    /// Event triggered when a new exception is reporting.
    /// </summary>
    public static event Action<Exception, Importance>? Reporting
    {
        add => SingletonInstance.Value.Reporting += value;
        remove => SingletonInstance.Value.Reporting -= value;
    }
    
    /// <summary>
    /// Use the singleton instance to report an exception.
    /// If no handler can handle this error, then it will be thrown.
    /// </summary>
    /// <param name="exception">Exception to report.</param>
    /// <param name="importance">Importance level of this exception.</param>
    public static object? Report(Exception exception, Importance importance = Importance.Error, bool fatal = false)
    {
        return SingletonInstance.Value.ReportError(exception, importance, false);
    }

    /// <summary>
    /// Use the singleton instance to report an exception.
    /// If no handler can handle this error, then it will be thrown.
    /// Importance level is fixed at <see cref="Importance.Error"/>.
    /// </summary>
    /// <param name="exception">Exception to report.</param>
    [DoesNotReturn]
    public static void ReportFatal(Exception exception)
    {
        SingletonInstance.Value.ReportError(exception, Importance.Error, true);
    }
    
    /// <summary>
    /// Use the singleton instance to instantiate and report an exception.
    /// If no handler can handle this error, then it will be thrown.
    /// </summary>
    /// <typeparam name="TException">Type of exception to instantiate and report.</typeparam>
    /// <param name="importance">Importance level of this exception.</param>
    /// <param name="arguments">Arguments to construct the exception instance.</param>
    public static object? Report<TException>(Importance importance = Importance.Error, params object?[] arguments)
        where TException : Exception
    {
        if (Activator.CreateInstance(typeof(TException), arguments) is not Exception exception)
            throw new RuntimeError($"Failed to instantiate {typeof(TException).Name} or " +
                                   $"convert it into {nameof(Exception)}.");
        return SingletonInstance.Value.ReportError(exception, importance);
    }

    /// <summary>
    /// Use the singleton instance to instantiate and report an exception.
    /// If no handler can handle this error, then it will be thrown.
    /// Importance level is fixed at <see cref="Importance.Error"/>.
    /// </summary>
    /// <typeparam name="TException">Type of exception to instantiate and report.</typeparam>
    /// <param name="arguments">Arguments to construct the exception instance.</param>
    [DoesNotReturn]
    public static void ReportFatal<TException>(params object?[] arguments)
        where TException : Exception
    {
        if (Activator.CreateInstance(typeof(TException), arguments) is not Exception exception)
            throw new RuntimeError($"Failed to instantiate {typeof(TException).Name} or " +
                                   $"convert it into {nameof(Exception)}.");
        SingletonInstance.Value.ReportError(exception, Importance.Error, true);
    }

    public static void RegisterHandler(Type exceptionType, IErrorHandler handler)
        => SingletonInstance.Value.AddHandler(exceptionType, handler);

    public static void UnregisterHandler(Type exceptionType, Type handlerType)
        => SingletonInstance.Value.RemoveHandler(exceptionType, handlerType);
}
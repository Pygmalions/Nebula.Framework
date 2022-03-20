using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Nebula.Exceptions;

public class ErrorHandlingCenter
{
    /// <summary>
    /// Hash map of (Exception type -> Handler type -> Handler instance).
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IErrorHandler>> _handlers = new();

    /// <summary>
    /// Report an exception.
    /// If no handler can handle this error, then it will be thrown.
    /// </summary>
    /// <param name="exception">Exception to report.</param>
    /// <param name="importance">Importance level of this exception.</param>
    /// <param name="fatal">Whether this error is fatal or not. If true, the exception will always be thrown.</param>
    public void ReportError(Exception exception, Importance importance = Importance.Error, 
        [DoesNotReturnIf(true)] bool fatal = false)
    {
        var context = new ErrorContext(exception, importance);
        for (var searchingType = exception.GetType(); searchingType != null; searchingType = searchingType.BaseType)
        {
            if (!_handlers.TryGetValue(searchingType, out var category))
                continue;
            
            foreach (var (_, handler) in category)
            {
                handler.Handle(context);
            }
        }

        if (!fatal && !context.ThrowRequired && context.ContinueRequired)
            return;
        throw context.Error;
    }

    /// <summary>
    /// Report an fatal exception.
    /// This exception will certainly be thrown.
    /// </summary>
    /// <param name="exception">Exception to report.</param>
    /// <param name="importance">Importance level of this exception.</param>
    [DoesNotReturn]
    public void ReportFatalError(Exception exception, Importance importance = Importance.Error)
        => ReportError(exception, importance, true);

    /// <summary>
    /// Instantiate an exception and report it.
    /// If no handler can handle this error, then it will be thrown.
    /// </summary>
    /// <typeparam name="TException">Type of exception to instantiate and report.</typeparam>
    /// <param name="importance">Importance level of this exception.</param>
    /// <param name="arguments">Arguments to construct the exception instance.</param>
    public void ReportError<TException>(Importance importance = Importance.Error, params object?[] arguments)
        where TException : Exception
    {
        if (Activator.CreateInstance(typeof(TException), arguments) is not Exception exception)
            throw new RuntimeError($"Failed to instantiate {typeof(TException).Name} or " +
                                   $"convert it into {nameof(Exception)}.");
        ReportError(exception, importance);
    }
    
    /// <summary>
    /// Instantiate an exception and report it as an fatal error.
    /// The exception will certainly be thrown.
    /// </summary>
    /// <typeparam name="TException">Type of exception to instantiate and report.</typeparam>
    /// <param name="importance">Importance level of this exception.</param>
    /// <param name="arguments">Arguments to construct the exception instance.</param>
    [DoesNotReturn]
    public void ReportFatalError<TException>(Importance importance = Importance.Error, params object?[] arguments)
        where TException : Exception
    {
        if (Activator.CreateInstance(typeof(TException), arguments) is not Exception exception)
            throw new RuntimeError($"Failed to instantiate {typeof(TException).Name} or " +
                                   $"convert it into {nameof(Exception)}.");
        ReportError(exception, importance, true);
    }

    /// <summary>
    /// Add an error handler.
    /// </summary>
    /// <param name="exception">Type exception which can be handled by this handler.</param>
    /// <param name="handler">Handler instance.</param>
    public void AddHandler(Type exception, IErrorHandler handler)
    {
        if (!exception.IsSubclassOf(typeof(Exception)))
            ErrorCenter.Report<UserError>(Importance.Error,
                $"Target exception type {exception.Name} is not derived from {nameof(Exception)}.");
        _handlers.GetOrAdd(exception, _ => new ConcurrentDictionary<Type, IErrorHandler>())
            .TryAdd(handler.GetType(), handler);
    }

    /// <summary>
    /// Remove an error handler.
    /// </summary>
    /// <param name="exception">Type exception which can be handled by this handler.</param>
    /// <param name="handlerType">Type of the handler to remove.</param>
    public void RemoveHandler(Type exception, Type handlerType)
    {
        if (!_handlers.TryGetValue(exception, out var handlers))
            return;
        handlers.TryRemove(handlerType, out _);
    }
}
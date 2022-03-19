using System.Collections.Concurrent;

namespace Nebula.Exceptions;

using ContinuableErrorHandler = Func<Exception, bool>;

public static class ContinuableError
{
    private static readonly ConcurrentDictionary<Type,
        ConcurrentDictionary<ContinuableErrorHandler, ContinuableErrorHandler>> HandlerGroups = new();

    /// <summary>
    /// Register a continuable exception handler.
    /// </summary>
    /// <param name="exception">Type exception to handle.</param>
    /// <param name="handler">
    /// If this handler returns false,
    /// then the exception will be considered as not continuable and thrown immediately.
    /// </param>
    public static void RegisterHandler(Type exception, ContinuableErrorHandler handler)
    {
        var group = HandlerGroups.GetOrAdd(exception,
            type => new ConcurrentDictionary<ContinuableErrorHandler, Func<Exception, bool>>());
        group.TryAdd(handler, handler);
    }

    public static void UnregisterHandler(Type exception, ContinuableErrorHandler handler)
    {
        if (HandlerGroups.TryGetValue(exception, out var group))
            group.TryRemove(handler, out _);
    }

    /// <summary>
    /// Try throw an exception.
    /// If any handler returns false, then the exception will be thrown.
    /// </summary>
    /// <param name="exception">Exception to handle.</param>
    public static void Throw(Exception exception)
    {
        var exceptionType = exception.GetType();
        foreach (var (type, handlers) in HandlerGroups)
        {
            if (!exceptionType.IsSubclassOf(type)) continue;
            if (handlers.Values.Any(handler => !handler.Invoke(exception))) throw exception;
        }
    }
}
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Nebula.Reporting;

/// <summary>
/// Static reporter helper class,
/// provides global singleton instances of reporters.
/// This static class is the default reporting center of the whole program.
/// </summary>
public static class Report
{
    /// <summary>
    /// Lazy singleton instance of error reporter.
    /// </summary>
    private static readonly Lazy<Reporter<Exception>> SingletonErrorReporter =
        new(() => new Reporter<Exception>());

    /// <summary>
    /// Lazy singleton instance of the message reporter.
    /// </summary>
    private static readonly Lazy<Reporter<(string, Importance)>> SingletonMessageReporter =
        new(() => new Reporter<(string, Importance)>());

    /// <summary>
    /// Run-time debug mode checker.
    /// This code will check whether the assembly is in debug mode or not in runtime.
    /// Because this library will certainly be published in Release mode,
    /// and we want to know whether the program which is using it is in Debug mode or not.
    /// As a result, "DEBUG" macro will not help, and we have to check it in run-time.
    /// </summary>
    private static readonly Lazy<bool> DebugModeEnable = new(() =>
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return false;
        var debugAttributes = assembly.GetCustomAttribute<DebuggableAttribute>();
        return debugAttributes != null && 
               debugAttributes.DebuggingFlags.HasFlag(DebuggableAttribute.DebuggingModes.EnableEditAndContinue);
    });
    
    /// <summary>
    /// Reporter to handle error.
    /// </summary>
    public static Reporter<Exception> ErrorReporter => SingletonErrorReporter.Value;

    /// <summary>
    /// Reporter to handle message.
    /// </summary>
    public static Reporter<(string Text, Importance Level)> MessageReporter => SingletonMessageReporter.Value;

    /// <summary>
    /// Whether enable the debug mode or not.
    /// </summary>
    public static bool DebugMode => DebugModeEnable.Value;

    /// <summary>
    /// Report an error.
    /// An error is an exception which will be reported and thrown in any mode.
    /// </summary>
    /// <param name="exception">Exception to report and throw.</param>
    /// <param name="id">ID associated with this error.</param>
    /// <returns>The given exception.</returns>
    [DoesNotReturn]
    public static void Error(Exception exception, Guid? id = null)
    {
        // Do not use AsyncReport here, otherwise the reporter may be interrupted by exception.
        ErrorReporter.Report(exception, id);
        throw exception;
    }

    /// <summary>
    /// Report an warning.
    /// An warning is the exception which will be thrown in Debug mode.
    /// </summary>
    /// <param name="exception">Warning exception.</param>
    /// <param name="id">ID associated with this warning.</param>
    public static void Warning(Exception exception, Guid? id = null)
    {
        if (DebugMode)
        {
            MessageReporter.Report((exception.ToString(), Importance.Warning));
            throw exception;
        }
        MessageReporter.AsyncReport((exception.ToString(), Importance.Warning));
    }

    /// <summary>
    /// Report an message.
    /// </summary>
    /// <param name="text">Message text.</param>
    /// <param name="id">ID associated with this message.</param>
    public static void Message(string text, Guid? id = null)
    {
        MessageReporter.AsyncReport((text, Importance.Message), id);
    }

    /// <summary>
    /// Report an debug message.
    /// </summary>
    /// <param name="text">Debug text to report.</param>
    /// <param name="id">ID associated with this debug message.</param>
    public static void Debug(string text, Guid? id = null)
    {
        if (!DebugMode) return;
        MessageReporter.AsyncReport((text, Importance.Debug), id);
    }
}
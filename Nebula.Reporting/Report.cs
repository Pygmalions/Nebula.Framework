using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Nebula.Reporting;

/// <summary>
/// Report represents a report document.
/// It inherits the exception thus it can be directly thrown.
/// </summary>
public partial class Report : Exception
{
    /// <summary>
    /// Title of this report document.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Description of this report document.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Importance level of this report document.
    /// </summary>
    public Importance Level { get; private set; }
    
    /// <summary>
    /// Object instance which reports this document.
    /// </summary>
    public object? Owner { get; private set; }

    /// <summary>
    /// Date time when this document is reported.
    /// </summary>
    public DateTime Time { get; private set; }

    /// <summary>
    /// Attachment of this report document.
    /// </summary>
    public readonly Dictionary<string, object> Attachments = new();

    /// <summary>
    /// Construct a document with initial parameters.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Description of the document.</param>
    /// <param name="owner">Owner of the document.</param>
    /// <param name="level">Importance level.</param>
    public Report(string title, string? description = null,
        object? owner = null,
        Importance level = Importance.Information) : 
        base("Document exception: {title}")
    {
        Time = DateTime.Now;
        Title = title;
        Description = description ?? "";
        Level = level;
    }
    
    /// <summary>
    /// Set the reporting date time.
    /// </summary>
    /// <param name="time">Time to set, if null, then <see cref="DateTime.Now"/> will be used.</param>
    /// <returns>This document.</returns>
    public Report SetTime(DateTime? time = null)
    {
        Time = time ?? DateTime.Now;
        return this;
    }

    /// <summary>
    /// Set the description of this document.
    /// </summary>
    /// <param name="description">Description.</param>
    /// <returns>This document.</returns>
    public Report SetDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    /// Set the importance level of this document.
    /// </summary>
    /// <param name="importance">Importance level.</param>
    /// <returns>This document.</returns>
    public Report SetLevel(Importance importance)
    {
        Level = importance;
        return this;
    }

    /// <summary>
    /// Returns null if the entrance assembly is in Release mode.
    /// Thus, following code will only works in Debug mode.
    /// </summary>
    /// <returns>Null if it is not in Debug mode.</returns>
    public Report? InDebug => DebugModeChecker.Value ? this : null;
    
    /// <summary>
    /// Returns null if the entrance assembly is in Debug mode.
    /// Thus, following code will only works in Release mode.
    /// </summary>
    /// <returns>Null if it is not in Release mode.</returns>
    public Report? InRelease => !DebugModeChecker.Value ? this : null;

    /// <summary>
    /// Set the owner of this document.
    /// </summary>
    /// <param name="owner">The object which reports this document.</param>
    /// <returns>This document.</returns>
    public Report SetOwner(object owner)
    {
        Owner = owner;
        return this;
    }

    /// <summary>
    /// Report this document to the global reporters in <see cref="GlobalReporters"/>.
    /// <para>
    /// <b>DO NOT</b> report this document asynchronously if it will be thrown, otherwise the reporter will
    /// be interrupted by the exception, and some of the event handlers may not be able to finish their work.
    /// </para>
    /// </summary>
    /// <param name="async">
    /// Whether to report this document asynchronously or not.
    /// If null, then the method will be auto decided, and it will choose the synchronous reporting method
    /// when its Level is Importance.Error, otherwise the asynchronous way.
    /// </param>
    /// <returns>This document.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Invalid <see cref="Level"/> value.
    /// </exception>
    public Report GloballyNotify(bool? async = null)
    {
        async ??= Level != Importance.Error;
        var reporter = Level switch
        {
            Importance.Error => GlobalReporters.Error,
            Importance.Warning => GlobalReporters.Warning,
            Importance.Information => GlobalReporters.Information,
            _ => throw new ArgumentOutOfRangeException(nameof(Level))
        };
        if (async.Value)
            reporter.ReportAsync(this);
        else
            reporter.Report(this);
        return this;
    }

    /// <summary>
    /// Wrap this report into an exception.
    /// </summary>
    /// <param name="innerException">Inner exception.</param>
    /// <returns>Exception wrapper.</returns>
    public ReportExceptionWrapper AsException(Exception? innerException = null)
        => new ReportExceptionWrapper(this, innerException);

    /// <summary>
    /// Wrap this as an exception and throw it. <br />
    /// It is not recommended to use this method in scenarios where it is certainly that
    /// the exception will be thrown. <br />
    /// This method is designed to be used after <see cref="InDebug"/> or <see cref="InRelease"/>.
    /// </summary>
    /// <param name="innerException">Inner exception.</param>
    [DoesNotReturn]
    public void Throw(Exception? innerException = null)
        => throw new ReportExceptionWrapper(this, innerException);

    /// <summary>
    /// Describe this report in plain text.
    /// Attachments will be <b>ignored</b>.
    /// </summary>
    /// <returns>Plain text description of this document.</returns>
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Level.ToString().ToUpper());
        builder.Append(" - ");
        builder.AppendLine(Title);
        builder.Append(Time.ToString("yyyy-MM-dd HH:mm:ss:ms"));
        if (Owner != null)
        {
            builder.Append(" @{");
            builder.Append(Owner);
            builder.Append('}');
        }

        builder.AppendLine();
        builder.AppendLine(Description);

        return builder.ToString();
    }
}
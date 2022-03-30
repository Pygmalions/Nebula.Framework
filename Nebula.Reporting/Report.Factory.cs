using System.Diagnostics;
using System.Reflection;
using Nebula.Pooling;

namespace Nebula.Reporting;

public partial class Report
{
    internal static readonly Lazy<bool> DebugModeChecker = new(() =>
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            return false;
        var debuggableAttribute = assembly.GetCustomAttribute<DebuggableAttribute>();
        return debuggableAttribute is { IsJITTrackingEnabled: true };
    });
    
    private static readonly Lazy<Pool<Report>> SingletonReportPool = new(()=> new Pool<Report>(
        () => new Report("Undefined"), 
        report =>
        {
            report.Title = "Action Failed";
            report.Description = "";
            report.Owner = null;
            report.Level = Importance.Information;
            report.Attachments.Clear();
            return true;
        }){MaxCount = 32});

    public static Pool<Report> ReportPool => SingletonReportPool.Value;

    /// <summary>
    /// Store this report into the reports pool.
    /// </summary>
    public void Recycle()
    {
        ReportPool.Store(this);
    }

    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Error"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Error(string title, string? description = null, object? owner = null)
    {
        var report = ReportPool.Acquire();
        report.SetTitle(title);
        report.SetLevel(Importance.Error);
        if (description != null) 
            report.SetDescription(description);
        if (owner != null) 
            report.SetOwner(owner);
        report.SetTime();
        return report;
    }

    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Warning"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Warning(string title, string? description = null, object? owner = null)
    {
        var report = ReportPool.Acquire();
        report.SetTitle(title);
        report.SetLevel(Importance.Warning);
        if (description != null) 
            report.SetDescription(description);
        if (owner != null) 
            report.SetOwner(owner);
        report.SetTime();
        return report;
    }
    
    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Information"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Information(string title, string? description = null, object? owner = null)
    { 
        var report = ReportPool.Acquire();
        report.SetTitle(title);
        report.SetLevel(Importance.Information);
        if (description != null) 
            report.SetDescription(description);
        if (owner != null) 
            report.SetOwner(owner);
        report.SetTime();
        return report;
    }
}

public static class ReportHelper
{
    /// <summary>
    /// Handle this report according to its level. <br />
    /// If its level is Warning in Debug mode or Error,
    /// then it will be globally notified synchronously and thrown;
    /// otherwise, it will just be globally notified asynchronously.
    /// </summary>
    /// <param name="report">Report to handle.</param>
    /// <exception cref="ReportException">
    /// Report exception wrapper to throw if the level or the report is Warning in Debug mode or Error.
    /// </exception>
    public static void Handle(this Report report)
    {
        if (report.Level == Importance.Error || report.Level == Importance.Warning && Report.DebugModeChecker.Value)
        {
            report.Notify(false);
            throw report.AsException();
        }
        report.Notify(true);
        Report.ReportPool.Store(report);
    }
    
    /// <summary>
    /// Globally notify this report and then wrap it as an exception.
    /// </summary>
    /// <param name="report">Report to notify and wrap.</param>
    /// <returns>Exception wrapper.</returns>
    public static ReportException NotifyAsException(this Report report)
    {
        report.Notify(false);
        return report.AsException();
    }
}
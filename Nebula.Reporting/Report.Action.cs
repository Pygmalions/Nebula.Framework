using System.Runtime.CompilerServices;

namespace Nebula.Reporting;

/// <summary>
/// Action report is a kind of report which carries the result of a series of actions.
/// It will captured the exception thrown by the actions done by itself,
/// and can rethrow it and the end.
/// </summary>
public static class ActionReport
{
    /// <summary>
    /// Get the action exception attachment from an report.
    /// </summary>
    /// <param name="report">Report to acquire action exception.</param>
    /// <returns>Action exception.</returns>
    private static Exception? GetActionException(Report report)
    {
        return report.Attachments.TryGetValue("ActionException", out var exception) ? 
            exception as Exception : null;
    }

    /// <summary>
    /// Set the action exception to an report.
    /// </summary>
    /// <param name="report">Report to carry this exception.</param>
    /// <param name="exception">Exception to attach.</param>
    private static void SetActionException(Report report, Exception exception)
    {
        report.Attachments["ActionException"] = exception;
    }
    
    /// <summary>
    /// Acquire a report to carry the action exception.
    /// </summary>
    /// <param name="title">Title of the action report.</param>
    /// <param name="owner">Optional owner of this report.</param>
    /// <returns>Action report.</returns>
    public static Report BeginAction(string? title = null, object? owner = null)
    {
        var report = Report.ReportPool.Acquire();
        report.SetLevel(Importance.Information);
        if (title != null) 
            report.SetTitle(title);
        if (owner != null) 
            report.SetOwner(owner);
        report.SetTime();
        return report;
    }

    /// <summary>
    /// Invoke the handler if any <b>previous</b> actions failed.
    /// </summary>
    /// <param name="report">Report which may carry an action exception.</param>
    /// <param name="handler">Handler action.</param>
    /// <returns>This report.</returns>
    public static Report OnFailed(this Report report, Action<Report> handler)
    {
        if (GetActionException(report) == null)
            return report;
        handler(report);
        return report;
    }

    /// <summary>
    /// Invoke the handler if all <b>previous</b> actions succeeded.
    /// </summary>
    /// <param name="report">Report which may carry an action exception.</param>
    /// <param name="handler">Handler action.</param>
    /// <returns>This report.</returns>
    public static Report OnSucceeded(this Report report, Action<Report> handler)
    {
        if (GetActionException(report) != null)
            return report;
        handler(report);
        return report;
    }

    /// <summary>
    /// Do an action.
    /// If this action throws an exception,
    /// then the exception thrown by this action will be captured and rethrow in <see cref="FinishAction"/> method.
    /// following DoAction(...) be skipped. < br/>
    /// That means, this method will do nothing if an exception has already been captured.
    /// </summary>
    /// <param name="report">Report to carry the exception.</param>
    /// <param name="action">Action to perform.</param>
    /// <param name="description">Description set to the report if this action failed.</param>
    /// <returns>This report.</returns>
    public static Report DoAction(this Report report, Action action, 
        [CallerArgumentExpression("action")] string description = "")
    {
        if (GetActionException(report) != null)
            return report;
        try
        {
            action();
        }
        catch (Exception exception)
        {
            report.SetLevel(Importance.Error);
            report.SetDescription(description);
            SetActionException(report, exception);
        }
        return report;
    }

    /// <summary>
    /// Finish the process of these actions.
    /// If a exception is thrown, the this method will notify the global reporters if notifiable is true,
    /// and then it will rethrow the captured exception. <br />
    /// <br />
    /// This method will store this report into the global reports pool if all actions succeeded.
    /// </summary>
    /// <param name="report">Report which may carry an action exception.</param>
    /// <param name="notifyOnSuccess">
    /// If true, this report will be reported to GlobalReporters.Information if it succeeded.</param>
    /// <param name="notifyOnFailure">
    /// If true, this report will be reported to GlobalReporters.Error if it failed.</param>
    /// <exception cref="ReportException">
    /// The wrapper of this report, and it will be thrown if any action throws a exception;
    /// the exception will be stored as inner exception.
    /// </exception>
    public static void FinishAction(this Report report, bool notifyOnSuccess = false, bool notifyOnFailure = true)
    {
        var exception = GetActionException(report);
        if (exception == null)
        {
            if (notifyOnSuccess)
            {
                report.SetLevel(Importance.Information);
                report.SetTitle(report.Title + " - SUCCEEDED");
                GlobalReporters.Information.Report(report);
            }
            report.Recycle();
            return;
        }

        report.SetLevel(Importance.Error);
        report.SetTitle(report.Title + " - FAILED");
        report.SetTime();
        if (notifyOnFailure)
            GlobalReporters.Error.Report(report);
        throw report.AsException(exception);
    }
}
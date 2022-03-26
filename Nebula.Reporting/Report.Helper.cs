namespace Nebula.Reporting;

public static class ReportHelper
{
    /// <summary>
    /// Handle this report according to its level. <br />
    /// If its level is Warning in Debug mode or Error,
    /// then it will be globally notified synchronously and thrown;
    /// otherwise, it will just be globally notified asynchronously.
    /// </summary>
    /// <param name="report">Report to handle.</param>
    /// <exception cref="ReportExceptionWrapper">
    /// Report exception wrapper to throw if the level or the report is Warning in Debug mode or Error.
    /// </exception>
    public static void Handle(this Report report)
    {
        if (report.Level == Importance.Error || report.Level == Importance.Warning && Report.DebugModeChecker.Value)
        {
            report.GloballyNotify(false);
            throw report.AsException();
        }
        report.GloballyNotify(true);
    }
}
namespace Nebula.Reporting;

/// <summary>
/// Event entrance for reporting reports.
/// </summary>
public class Reporter
{
    /// <summary>
    /// Event triggered when a report document is reporting by this reporter.
    /// </summary>
    public event Action<Report>? Received;

    /// <summary>
    /// Report a document.
    /// </summary>
    /// <param name="document">Document to report.</param>
    public void Report(Report document)
    {
        Received?.Invoke(document);
    }

    /// <summary>
    /// Use the thread pool to report a document asynchronously.
    /// </summary>
    /// <param name="document">Document to report.</param>
    public void ReportAsync(Report document)
    {
        if (Received == null)
            return;
        ThreadPool.QueueUserWorkItem(_ => Received?.Invoke(document));
    }
}
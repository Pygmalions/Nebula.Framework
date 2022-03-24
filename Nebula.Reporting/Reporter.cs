namespace Nebula.Reporting;

/// <summary>
/// Reporter provides an central message handling entrance and event interface.
/// If the user want to throw an exception, record a log, use the global static reporters
/// by calling methods of the static helper class <see cref="Reporting.Report"/>:
/// <see cref="Reporting.Report.Error"/>, <see cref="Reporting.Report.Warning"/>,
/// <see cref="Reporting.Report.Message"/>, and <see cref="Reporting.Report.Debug"/>
/// </summary>
/// <typeparam name="TContent">Type of the content that this reporter can report.</typeparam>
/// <seealso cref="Reporting.Report"/>
public class Reporter<TContent>
{
    /// <summary>
    /// Event triggered when a <typeparamref name="TContent"/> is reported by <see cref="Report"/>.
    /// </summary>
    public event Action<TContent, Guid?>? Reported;

    /// <summary>
    /// Trigger the event <see cref="Reported"/> with the specific content.
    /// </summary>
    /// <param name="content">Content to report.</param>
    /// <param name="id">ID associated with this report.</param>
    public void Report(TContent content, Guid? id = null)
    {
        Reported?.Invoke(content, id);
    }

    /// <summary>
    /// Queue the reporting action in the thread pool and return immediately.
    /// </summary>
    /// <param name="content">Content to report.</param>
    /// <param name="id">ID associated with this report.</param>
    public void AsyncReport(TContent content, Guid? id = null)
    {
        if (Reported == null)
            return;
        ThreadPool.QueueUserWorkItem(_ => Reported?.Invoke(content, id));
    }
}
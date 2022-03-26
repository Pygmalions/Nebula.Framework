namespace Nebula.Reporting;

/// <summary>
/// This exception can wrap a document into an exception.
/// </summary>
public class ReportExceptionWrapper : Exception
{
    /// <summary>
    /// The attached document.
    /// </summary>
    public Report Document { get; }

    /// <summary>
    /// Wrap a report document into an exception.
    /// </summary>
    /// <param name="document">Document attached to this exception.</param>
    /// <param name="innerException">Inner exception within this exception.</param>
    public ReportExceptionWrapper(Report document, Exception? innerException = null) : base(
        $"(Report) {document.Title}: {document.Description}.", innerException)
    {
        Document = document;
    }
}
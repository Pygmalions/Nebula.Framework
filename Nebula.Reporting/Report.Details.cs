namespace Nebula.Reporting;

public static class ReportDetailsSupport
{
    /// <summary>
    /// Attach details to the report document.
    /// </summary>
    /// <param name="document">Document to operate on.</param>
    /// <param name="name">Name of details to attach.</param>
    /// <param name="instance">Details information object to attach.</param>
    /// <returns>This document.</returns>
    public static Report AttachDetails(this Report document, string name, object instance)
    {
        var details = GetDetails(document);

        if (details == null)
        {
            details = new Dictionary<string, object>();
            document.Attachments["Details"] = details;
        }
        
        details.Add(name, instance);
        return document;
    }

    /// <summary>
    /// Get all details attached to the document.
    /// </summary>
    /// <param name="document">Document to operate on.</param>
    /// <returns>Details dictionary.</returns>
    public static Dictionary<string, object>? GetDetails(this Report document)
    {
        if (document.Attachments.TryGetValue("Details", out var attachments) && 
            attachments is Dictionary<string, object> details)
        {
            return details;
        }

        return null;
    }
}
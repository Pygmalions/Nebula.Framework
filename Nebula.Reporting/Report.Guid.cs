namespace Nebula.Reporting;

public static class ReportGuidSupport
{
    /// <summary>
    /// Attach an GUID to the report document.
    /// </summary>
    /// <param name="document">Document to operate on.</param>
    /// <param name="guid">GUID to attach.</param>
    /// <param name="description">Optional description of the GUID.</param>
    /// <returns>This document.</returns>
    public static Report AttachGuid(this Report document, Guid guid, string description = "")
    {
        var guids = GetGuidAttachments(document);

        if (guids == null)
        {
            guids = new List<(Guid, string)>();
            document.Attachments["GUID"] = guids;
        }
        
        guids.Add((guid, description));
        return document;
    }

    /// <summary>
    /// Get all GUID attached to the document.
    /// </summary>
    /// <param name="document">Document to operate on.</param>
    /// <returns>List of GUID and their description.</returns>
    public static List<(Guid, string)>? GetGuidAttachments(this Report document)
    {
        if (document.Attachments.TryGetValue("GUID", out var attachments) && 
            attachments is List<(Guid, string)> guids)
        {
            return guids;
        }

        return null;
    }
}
using System.Diagnostics;
using System.Reflection;

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

    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Error"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Error(string title, string? description = null, object? owner = null)
        => new (title, description, owner, level: Importance.Error);

    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Warning"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Warning(string title, string? description = null, object? owner = null)
        => new(title, description, owner, level: Importance.Warning);
    
    /// <summary>
    /// Factory method to construct a document at the level of <see cref="Importance.Information"/>.
    /// </summary>
    /// <param name="title">Title of the document.</param>
    /// <param name="description">Optional description of the document.</param>
    /// <param name="owner">Optional owner of the document.</param>
    /// <returns>Document instance.</returns>
    public static Report Information(string title, string? description = null, object? owner = null)
        => new(title, description, owner, level: Importance.Information);
}
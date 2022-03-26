using System;

namespace Nebula.Extending;

/// <summary>
/// Nebula in-assembly resource discovery function will only work on assemblies marked with this attribute.
/// This attribute is used to indicate that this assembly should be used as a plugin.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class PluginAssemblyAttribute : Attribute
{
    /// <summary>
    /// Enable there plugins in release mode.
    /// </summary>
    public bool ApplyOnReleaseMode { get; init; } = true;
    
    /// <summary>
    /// Enable these plugins in debug mode.
    /// </summary>
    public bool ApplyOnDebugMode { get; init; } = true;
}
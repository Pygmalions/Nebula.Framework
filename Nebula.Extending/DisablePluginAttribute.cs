using System;
using System.Collections.Generic;

namespace Nebula.Extending;

/// <summary>
/// Inform the Nebula that this assembly will disable extern plugin assemblies,
/// this will remove the specific assemblies from the plugins list,
/// even if they have <see cref="PluginAssemblyAttribute" /> marked on them.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class DisablePluginAttribute : Attribute
{
    /// <summary>
    /// Declares the assemblies that should be used as plugins.
    /// </summary>
    /// <param name="assemblies">Names of assemblies to use as plugins.</param>
    public DisablePluginAttribute(params string[] assemblies)
    {
        Plugins = new HashSet<string>(assemblies);
    }

    /// <summary>
    /// Name of assemblies which should be used as plugins.
    /// </summary>
    public IReadOnlySet<string> Plugins { get; }

    /// <summary>
    /// Disable there plugins in release mode.
    /// </summary>
    public bool ApplyOnReleaseMode { get; init; } = true;
    
    /// <summary>
    /// Disable these plugins in debug mode.
    /// </summary>
    public bool ApplyOnDebugMode { get; init; } = true;
}
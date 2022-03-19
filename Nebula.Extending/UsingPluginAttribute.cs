using System;
using System.Collections.Generic;

namespace Nebula.Extending;

/// <summary>
/// Inform the Nebula that this assembly will use extern assemblies as its plugins,
/// this can enable in-assembly resource discovery function on assemblies
/// which have no <see cref="PluginAssemblyAttribute" /> marked on them.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class UsingPluginAttribute : Attribute
{
    /// <summary>
    /// Declares the assemblies that should be used as plugins.
    /// </summary>
    /// <param name="assemblies">Names of assemblies to use as plugins.</param>
    public UsingPluginAttribute(params string[] assemblies)
    {
        Plugins = new HashSet<string>(assemblies);
    }

    /// <summary>
    /// Name of assemblies which should be used as plugins.
    /// </summary>
    public IReadOnlySet<string> Plugins { get; }

    public bool Transitive { get; init; } = false;
}
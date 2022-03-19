using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nebula.Extending;

public partial class PluginRegistry
{
    private static Lazy<PluginRegistry> SingletonInstance = 
        new Lazy<PluginRegistry>(() => new PluginRegistry(true));

    /// <summary>
    /// Cached list of found plugins, and it is generated when it or <see cref="UpdateFoundPlugins"/>
    /// is firstly called.
    /// This list may be out of date and need to be update by <see cref="UpdateFoundPlugins"/>.
    /// <para>
    ///     This method uses the member <see cref="PluginRegistry.Plugins"/> of the
    ///     singleton lazy instance of a <see cref="PluginRegistry"/>.
    /// </para>
    /// </summary>
    public static IReadOnlyDictionary<AssemblyName, Assembly> FoundPlugins  => SingletonInstance.Value.Plugins;

    /// <summary>
    /// Update <see cref="FoundPlugins"/>.
    /// <para>
    ///     This method uses the <see cref="PluginRegistry.Update"/> method of the
    ///     singleton lazy instance of a <see cref="PluginRegistry"/>.
    /// </para>
    /// </summary>
    public static void UpdateFoundPlugins()
    {
        if (SingletonInstance.IsValueCreated)
            SingletonInstance.Value.Update();
        else _ = SingletonInstance.Value;
    }
        
}
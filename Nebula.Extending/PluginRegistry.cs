using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Nebula.Extending;

public partial class PluginRegistry
{
    /// <summary>
    /// Assembly name to assembly mapping dictionary of plugin assemblies.
    /// </summary>
    private readonly ConcurrentDictionary<AssemblyName, Assembly> _plugins = new();
    
    /// <summary>
    /// Registered plugins.
    /// Allow to do in-assembly resource discovery on these assemblies.
    /// </summary>
    public IReadOnlyDictionary<AssemblyName, Assembly> Plugins => _plugins;

    /// <summary>
    /// When a plugin manager is constructed, it can scan all plugin assemblies.
    /// </summary>
    /// <param name="scanningRequired">If true, this plugin manager will </param>
    public PluginRegistry(bool scanningRequired = true)
    {
        if (!scanningRequired)
            return;

        Update();
    }

    /// <summary>
    /// Rescan all loaded assemblies and update the plugins list.
    /// </summary>
    public void Update()
    {
        // Load and scan all plugin assemblies.
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Search assemblies for import targets.
        var allowedNames = new ConcurrentDictionary<string, string>();
        var selfAssembly = Assembly.GetEntryAssembly();
        if (selfAssembly != null)
            RegisterPlugin(selfAssembly);

        // Summarize plugins information and register assemblies with plugin attribute.
        foreach (var assembly in loadedAssemblies)
        {
            if (assembly.GetCustomAttribute<PluginAssemblyAttribute>() != null) RegisterPlugin(assembly);
            var isSelf = assembly == selfAssembly;
            var usingAttributes = assembly.GetCustomAttributes<UsingPluginAttribute>();
            foreach (var usingAttribute in usingAttributes)
            {
                if (!isSelf && !usingAttribute.Transitive)
                    return;
                foreach (var pluginName in usingAttribute.Plugins) allowedNames.TryAdd(pluginName, pluginName);
            }
        }

        // Register assemblies which is used by the executing assembly or other plugin assemblies.
        foreach (var assembly in loadedAssemblies)
        {
            var name = assembly.GetName().Name;
            if (name != null && allowedNames.ContainsKey(name))
                RegisterPlugin(assembly);
        }
    }
    
    /// <summary>
    /// Register an assembly as a plugin.
    /// This will enable the in-assembly resource discovery function on this assembly.
    /// </summary>
    /// <param name="assembly">Assembly to register.</param>
    public void RegisterPlugin(Assembly assembly)
    {
        _plugins.TryAdd(assembly.GetName(), assembly);
    }

    /// <summary>
    /// Unregister an assembly from plugin assemblies.
    /// This will disable the in-assembly resource discovery function on this assembly.
    /// </summary>
    /// <param name="assemblyName">Name of the assembly to unregister.</param>
    public void UnregisterPlugin(AssemblyName assemblyName)
    {
        _plugins.TryRemove(assemblyName, out _);
    }
}
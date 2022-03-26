using System.Collections.Concurrent;
using System.Reflection;

namespace Nebula.Extending;

public partial class PluginRegistry
{
    /// <summary>
    /// Assembly name to assembly mapping dictionary of plugin assemblies.
    /// </summary>
    private readonly ConcurrentDictionary<AssemblyName, (Assembly, bool Scanned)> _plugins = new();
    
    /// <summary>
    /// Registered plugins.
    /// Allow to do in-assembly resource discovery on these assemblies.
    /// </summary>
    public IReadOnlyDictionary<AssemblyName, (Assembly, bool Scanned)> Plugins => _plugins;

    /// <summary>
    /// Rescan all loaded assemblies and update the plugins list.
    /// </summary>
    public void Update()
    {
        // Remove all scanned plugins.
        foreach (var (name, (assembly, scanned)) in _plugins)
        {
            if (scanned)
                _plugins.TryRemove(name, out _);
        }
        
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
            var pluginAttribute = assembly.GetCustomAttribute<PluginAssemblyAttribute>();
            if (pluginAttribute == null)
                continue;
            if (DebugMode && !pluginAttribute.ApplyOnDebugMode || 
                !DebugMode && !pluginAttribute.ApplyOnReleaseMode)
                continue;
            RegisterPlugin(assembly);
            var enablingAttributes = assembly.GetCustomAttributes<EnablePluginAttribute>();
            foreach (var enablingAttribute in enablingAttributes)
            {
                // The priority of "&&" is higher than "||".
                if (DebugMode && !enablingAttribute.ApplyOnDebugMode || 
                    !DebugMode && !enablingAttribute.ApplyOnReleaseMode)
                    return;
                foreach (var pluginName in enablingAttribute.Plugins) allowedNames.TryAdd(pluginName, pluginName);
            }
            var disablingAttributes = assembly.GetCustomAttributes<DisablePluginAttribute>();
            foreach (var disablingAttribute in disablingAttributes)
            {
                if (DebugMode && !disablingAttribute.ApplyOnDebugMode || 
                    !DebugMode && !disablingAttribute.ApplyOnReleaseMode)
                    return;
                foreach (var pluginName in disablingAttribute.Plugins) 
                    allowedNames.TryRemove(pluginName, out _);
            }
        }

        // Register assemblies which is used by the executing assembly or other plugin assemblies.
        foreach (var assembly in loadedAssemblies)
        {
            var name = assembly.GetName().Name;
            if (name != null && allowedNames.ContainsKey(name))
                _plugins.TryAdd(assembly.GetName(), (assembly, true));
        }
    }
    
    /// <summary>
    /// Register an assembly as a plugin.
    /// This will enable the in-assembly resource discovery function on this assembly.
    /// </summary>
    /// <param name="assembly">Assembly to register.</param>
    public void RegisterPlugin(Assembly assembly)
    {
        _plugins.TryAdd(assembly.GetName(), (assembly, false));
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
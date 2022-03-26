using System.Diagnostics;
using System.Reflection;

namespace Nebula.Extending;

/// <summary>
/// Global singleton mode wrapper for <see cref="PluginRegistry"/>.
/// </summary>
public static class Plugins
{
    private static readonly Lazy<PluginRegistry> SingletonInstance = new(() =>
    {
        var registry = new PluginRegistry();
        registry.Update();
        return registry;
    });

    /// <inheritdoc cref="PluginRegistry.Plugins"/>
    public static IReadOnlyDictionary<AssemblyName, (Assembly, bool Scanned)> Assemblies => 
        SingletonInstance.Value.Plugins;

    /// <inheritdoc cref="PluginRegistry.Update"/>
    public static void Update()
        => SingletonInstance.Value.Update();

    /// <inheritdoc cref="PluginRegistry.RegisterPlugin"/>
    public static void RegisterPlugin(Assembly assembly)
        => SingletonInstance.Value.RegisterPlugin(assembly);
    
    /// <inheritdoc cref="PluginRegistry.UnregisterPlugin"/>
    public static void UnregisterPlugin(AssemblyName assemblyName)
        => SingletonInstance.Value.UnregisterPlugin(assemblyName);
    
}
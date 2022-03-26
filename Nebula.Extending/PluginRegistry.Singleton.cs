using System.Diagnostics;
using System.Reflection;

namespace Nebula.Extending;

public partial class PluginRegistry
{
    private static readonly Lazy<bool> DebugModeChecker = new(() =>
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            return false;
        var debuggableAttribute = assembly.GetCustomAttribute<DebuggableAttribute>();
        return debuggableAttribute is { IsJITTrackingEnabled: true };
    });

    private static readonly Lazy<PluginRegistry> SingletonInstance = new(() =>
    {
        var registry = new PluginRegistry();
        registry.Update();
        return registry;
    });

    /// <summary>
    /// Whether the entrance assembly is built in debug mode or not.
    /// This check is performing in run-time, thus it is available for libraries to use.
    /// </summary>
    public static bool DebugMode => DebugModeChecker.Value;
    
    /// <summary>
    /// Global singleton instance of the plugin registry, which will scan all plugin assemblies
    /// once this scanner is accessed.
    /// </summary>
    public static PluginRegistry Scanner => SingletonInstance.Value;
}
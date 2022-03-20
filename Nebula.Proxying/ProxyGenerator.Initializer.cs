using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Nebula.Exceptions;
using Nebula.Extending;

namespace Nebula.Proxying;

public partial class ProxyGenerator
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, AspectHandler>>
        AspectHandlers = new();

    private static bool _aspectHandlerInitialized;

    /// <summary>
    /// By default, it will scan aspect handlers in assemblies found in <see cref="PluginRegistry.FoundPlugins"/>.
    /// </summary>
    private static void InitializeAspectHandlers()
    {
        foreach (var (_, assembly) in PluginRegistry.FoundPlugins)
            Parallel.ForEach(assembly.GetTypes(), handlerType =>
            {
                if (!handlerType.IsSubclassOf(Meta.AspectHandlerMeta.Class))
                    return;
                var attribute = handlerType.GetCustomAttribute<AspectHandlerAttribute>();
                if (attribute == null)
                    return;
                RegisterAspectHandler(handlerType);
            });
    }

    /// <summary>
    /// Search all classes derived from <see cref="AspectHandler" /> and
    /// marked with <see cref="AspectHandlerAttribute" />.
    /// </summary>
    /// <param name="assembly">Assembly to search from.</param>
    /// <returns>Set of aspect handler types.</returns>
    public static IReadOnlySet<Type> SearchAspectHandler(Assembly assembly)
    {
        var handlerTypes = new HashSet<Type>();

        foreach (var candidateType in assembly.GetTypes())
        {
            if (!candidateType.IsSubclassOf(Meta.AspectHandlerMeta.Class))
                continue;
            var attribute = candidateType.GetCustomAttribute<AspectHandlerAttribute>();
            if (attribute == null)
                continue;
            handlerTypes.Add(candidateType);
        }

        return handlerTypes;
    }

    /// <summary>
    /// Try to register a plugin to the generator.
    /// </summary>
    /// <param name="handlerType">Type of a proxy generator.</param>
    /// <exception cref="UserError">
    /// Throw if the given type is not a class type which derived from <see cref="AspectHandler" />,
    /// or it is not marked with <see cref="AspectHandlerAttribute" />.
    /// </exception>
    public static void RegisterAspectHandler(Type handlerType)
    {
        if (!handlerType.IsSubclassOf(Meta.AspectHandlerMeta.Class))
            ErrorCenter.ReportFatal<UserError>(
                $"Type {handlerType.Name} is not a class type inherited from {nameof(AspectHandler)}.");
        var attribute = handlerType.GetCustomAttribute<AspectHandlerAttribute>();
        if (attribute == null)
            ErrorCenter.ReportFatal<UserError>(
                $"Type {handlerType.Name} doest not have attribute {nameof(AspectHandlerAttribute)}.");
        foreach (var triggerAttributeType in attribute.TriggerAttributes)
        {
            if (!triggerAttributeType.IsSubclassOf(Meta.AspectTriggerMeta.Class))
            {
                ErrorCenter.Report<UserError>(Importance.Error,
                    $"Plugin {handlerType.Name} will not be triggered by the attribute " +
                    $"{triggerAttributeType.Name} for it is not derived from {nameof(AspectTrigger)}");
                continue;
            }
            var group = AspectHandlers.GetOrAdd(
                triggerAttributeType, _ =>
                    new ConcurrentDictionary<Type, AspectHandler>());
            AspectHandler? handler;
            try
            {
                handler = Activator.CreateInstance(handlerType) as AspectHandler;
            }
            catch (Exception exception)
            {
                switch (exception)
                {
                    case MissingMethodException:
                        ErrorCenter.Report<UserError>(Importance.Error,
                            $"Aspect handler {handlerType.Name} is marked with the " +
                            $"{nameof(AspectHandlerAttribute)} but has no constructor without arguments.",
                            exception);
                        continue;
                    default:
                        ErrorCenter.Report<RuntimeError>(Importance.Error,
                            "Failed to instantiate instance for aspect handler {handlerType.Name}",
                            exception);
                        continue;
                }
            }

            if (handler == null)
                ErrorCenter.ReportFatal<RuntimeError>(Importance.Error,
                    "Failed to instantiate instance for aspect handler {handlerType.Name}");
            
            group.TryAdd(handlerType, handler!);
        }
    }

    /// <summary>
    /// Try to unregister an aspect handler from the generator.
    /// </summary>
    /// <param name="handlerType">Type of the handler to register.</param>
    public static void UnregisterAspectHandler(Type handlerType)
    {
        var attribute = handlerType.GetCustomAttribute<AspectHandlerAttribute>();
        if (attribute == null)
            return;
        foreach (var triggerAttributeType in attribute.TriggerAttributes)
        {
            if (!triggerAttributeType.IsSubclassOf(Meta.AspectTriggerMeta.Class))
                continue;
            if (!AspectHandlers.TryGetValue(triggerAttributeType, out var group))
                continue;

            group.TryRemove(handlerType, out _);
        }
    }

    /// <summary>
    /// Apply plugins on the given proxy.
    /// </summary>
    /// <param name="proxy">Proxy entry to apply plugins on.</param>
    private static void ApplyAspectHandlers(Proxy proxy)
    {
        if (!_aspectHandlerInitialized)
        {
            InitializeAspectHandlers();
            _aspectHandlerInitialized = true;
        }

        var matchedAttributes = new HashSet<AspectTrigger>();

        matchedAttributes.UnionWith(proxy.ProxiedMethod.GetCustomAttributes<AspectTrigger>());

        if (proxy.AssociatedProperty != null)
            matchedAttributes.UnionWith(proxy.AssociatedProperty.GetCustomAttributes<AspectTrigger>());

        foreach (var triggerAttribute in matchedAttributes)
        {
            if (!AspectHandlers.TryGetValue(triggerAttribute.GetType(),
                    out var plugins))
                continue;
            foreach (var plugin in plugins) plugin.Value.Handle(proxy);
        }
    }
}
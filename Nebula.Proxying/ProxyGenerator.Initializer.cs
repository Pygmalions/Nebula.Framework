using System.Collections.Concurrent;
using System.Reflection;
using Nebula.Extending;
using Nebula.Reporting;

namespace Nebula.Proxying;

public partial class ProxyGenerator
{
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, AspectHandler>>
        AspectHandlers = new();

    private static bool _aspectHandlerInitialized;

    /// <summary>
    /// By default, it will scan aspect handlers in assemblies found in <see cref="Plugins.Assemblies"/>.
    /// </summary>
    private static void InitializeAspectHandlers()
    {
        foreach (var (_, (assembly, _)) in Plugins.Assemblies)
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
    /// <exception cref="ReportExceptionWrapper">
    /// Throw if the given type is not a class type which derived from <see cref="AspectHandler" />,
    /// or it is not marked with <see cref="AspectHandlerAttribute" />.
    /// </exception>
    public static void RegisterAspectHandler(Type handlerType)
    {
        if (!handlerType.IsSubclassOf(Meta.AspectHandlerMeta.Class))
            throw Report.Error("Invalid Handler Type", 
                $"This handler type is not a class derived from {nameof(AspectHandler)}.")
                .AttachDetails("Handler", handlerType)
                .GloballyNotify()
                .AsException();
        var attribute = handlerType.GetCustomAttribute<AspectHandlerAttribute>();
        if (attribute == null)
            throw Report.Error("Invalid Handler Type", 
                    $"This handler type is marked with the attribute {nameof(AspectHandlerAttribute)}.")
                .AttachDetails("Handler", handlerType)
                .GloballyNotify()
                .AsException();
        foreach (var triggerAttributeType in attribute.TriggerAttributes)
        {
            if (!triggerAttributeType.IsSubclassOf(Meta.AspectTriggerMeta.Class))
            {
                Report.Warning("Invalid Aspect",
                        "Handler will not be triggered by this aspect, for it is not derived " +
                        $"from {nameof(AspectHandlerAttribute)}.")
                    .AttachDetails("Handler", handlerType)
                    .AttachDetails("Aspect", triggerAttributeType)
                    .Handle();
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
                        Report.Warning("Invalid Aspect Handler",
                                "No constructor without parameters for this aspect handler type which " +
                                $"is marked with {nameof(AspectHandlerAttribute)}")
                            .AttachDetails("Handler", handlerType)
                            .Handle();
                        continue;
                    default:
                        Report.Warning("Invalid Aspect Handler",
                                "Failed to instantiate this handler which" +
                                $"is marked with {nameof(AspectHandlerAttribute)}")
                            .AttachDetails("Handler", handlerType)
                            .Handle();
                        continue;
                }
            }

            if (handler == null)
            {
                Report.Warning("Invalid Aspect Handler",
                        "Failed to instantiate this handler which" +
                        $"is marked with {nameof(AspectHandlerAttribute)}")
                    .AttachDetails("Handler", handlerType)
                    .Handle();
                continue;
            }

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
    /// <param name="proxyEntry">Proxy entry to apply plugins on.</param>
    private static void ApplyAspectHandlers(ProxyEntry proxyEntry)
    {
        if (!_aspectHandlerInitialized)
        {
            InitializeAspectHandlers();
            _aspectHandlerInitialized = true;
        }

        var matchedAttributes = new HashSet<AspectTrigger>();

        matchedAttributes.UnionWith(proxyEntry.ProxiedMethod.GetCustomAttributes<AspectTrigger>());

        if (proxyEntry.AssociatedProperty != null)
            matchedAttributes.UnionWith(proxyEntry.AssociatedProperty.GetCustomAttributes<AspectTrigger>());

        foreach (var triggerAttribute in matchedAttributes)
        {
            if (!AspectHandlers.TryGetValue(triggerAttribute.GetType(),
                    out var plugins))
                continue;
            foreach (var plugin in plugins) plugin.Value.Handle(proxyEntry);
        }
    }
}
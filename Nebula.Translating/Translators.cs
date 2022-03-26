using System.Reflection;
using Nebula.Extending;
using Nebula.Reporting;

namespace Nebula.Translating;

/// <summary>
/// Singleton facade static class for <see cref="TranslatorRegistry"/>.
/// </summary>
public static class Translators
{
    private static readonly Lazy<TranslatorRegistry> SingletonInstance = new(() =>
    {
        var registry = new TranslatorRegistry();

        // Apply auto discovery.
        foreach (var (_, (assembly, _)) in Plugins.Assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attributes = type.GetCustomAttributes<TranslatorAttribute>().ToArray();
                if (attributes.Length == 0)
                    continue;
                Type? declaredFromType = null;
                Type? declaredToType = null;
                
                // Filter all class implements ITranslator<,>
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType ||
                        implementedInterface.GetGenericTypeDefinition() != typeof(ITranslator<,>))
                        continue;

                    var arguments = implementedInterface.GetGenericArguments();
                    if (arguments.Length != 2)
                    {
                        Report.Warning("Object is Not Translator.",
                                "Arguments length incorrect.")
                            .AttachDetails("Length", arguments.Length)
                            .AttachDetails("Arguments", arguments)
                            .Handle();
                        continue;
                    }
                    declaredFromType = arguments[0];
                    declaredToType = arguments[1];
                }

                if (declaredFromType == null || declaredToType == null)
                    continue;
                
                // Check and apply attributes.
                foreach (var attribute in attributes)
                {
                    var attributeFromType = attribute.FromType ?? declaredFromType;
                    var attributeToType = attribute.ToType ?? declaredToType;

                    // Filter useless translators.
                    if (attributeFromType.IsAssignableTo(attributeToType) && attribute.Protocol == "")
                    {
                        Report.Warning("Failed to Register Translator", 
                                "An nameless translator can not have the original type " +
                                "which can be directly assigned to destination type.")
                            .AttachDetails("Translator", type)
                            .AttachDetails("OriginalType", attributeFromType)
                            .AttachDetails("DestinationType", attributeToType)
                            .Handle();
                        continue;
                    }
                    
                    if (!attributeFromType.IsAssignableTo(declaredFromType))
                    {
                        Report.Warning("Failed to Discover a Translator",
                                "Expected type is not assignable to original type.")
                            .AttachDetails("Type", declaredFromType)
                            .AttachDetails("ExpectedType", attributeFromType)
                            .Handle();
                        continue;
                    }

                    if (attributeToType.IsAssignableFrom(declaredToType))
                    {
                        var translator = Activator.CreateInstance(type);
                        if (translator == null)
                        {
                            Report.Warning("Failed to Discover a Translator",
                                    "Failed to initiate a translator instance.")
                                .AttachDetails("Type", type);
                            continue;
                        }
                        registry.RegisterTranslator(attributeFromType, attributeToType, 
                            translator, attribute.Protocol);
                    }

                    Report.Warning("Failed to Verify Translator.",
                            "Expected type is not assignable from destination type.")
                        .AttachDetails("Type", declaredToType)
                        .AttachDetails("ExpectedType", attributeToType)
                        .Handle();
                }
                
            }
        }

        return registry;
    });

    /// <inheritdoc cref="TranslatorRegistry.RegisterTranslator"/>
    public static void RegisterTranslator(Type from, Type to, object translator, string protocol = "")
        => SingletonInstance.Value.RegisterTranslator(from, to, translator, protocol);
    
    /// <inheritdoc cref="TranslatorRegistry.UnregisterTranslator"/>
    public static void UnregisterTranslator(Type from, Type to, string protocol = "")
        => SingletonInstance.Value.UnregisterTranslator(from, to, protocol);

    /// <inheritdoc cref="TranslatorRegistry.GetProtocols"/>
    public static ICollection<string> GetProtocols(Type from, Type to)
        => SingletonInstance.Value.GetProtocols(from, to);
    
    
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <param name="from">Object to translate.</param>
    /// <param name="toType">Destination type.</param>
    /// <param name="protocol">
    /// Protocol to use. Empty means that this any protocol is accepted.
    /// </param>
    /// <returns>Translated object, or null if no suitable translator found or translation failed.</returns>
    public static object? Translate(object from, Type toType, string protocol = "")
        => SingletonInstance.Value.Translate(from, toType, protocol);
    
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <param name="from">Object to translate.</param>
    /// <param name="fromType">Original type.</param>
    /// <param name="toType">Destination type.</param>
    /// <param name="protocol">
    /// Protocol to use. Empty means that this any protocol is accepted.
    /// </param>
    /// <returns>Translated object, or null if no suitable translator found or translation failed.</returns>
    public static object? Translate(object from, Type fromType, Type toType, string protocol = "")
        => SingletonInstance.Value.Translate(from, fromType, toType, protocol);
    
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <typeparam name="TTo">Destination type.</typeparam>
    /// <param name="from">Object to translate.</param>
    /// <param name="protocol">Protocol to use. Empty means that this any protocol is accepted.</param>
    /// <returns>
    /// Translated <typeparamref name="TTo"/>,
    /// or null if no suitable translator found or translation failed.
    /// </returns>
    public static TTo? Translate<TTo>(object from, string protocol = "")
        => (TTo?)SingletonInstance.Value.Translate(from, typeof(TTo), protocol);
    
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    /// <param name="from">Object to translate.</param>
    /// <param name="protocol">Protocol to use. Empty means that this any protocol is accepted.</param>
    /// <returns>
    /// Translated <typeparamref name="TTo"/>,
    /// or null if no suitable translator found or translation failed.
    /// </returns>
    public static TTo? Translate<TFrom, TTo>(object from, string protocol = "")
        => (TTo?)SingletonInstance.Value.Translate(from, typeof(TFrom), typeof(TTo), protocol);
    
    /// <summary>
    /// Register a translator into the global singleton registry.
    /// </summary>
    /// <param name="translator">Translator to register.</param>
    /// <param name="protocol">Protocol of the translator.</param>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    public static void RegisterTranslator<TFrom, TTo>(object translator, string protocol = "")
        => SingletonInstance.Value.RegisterTranslator(typeof(TFrom), typeof(TTo), translator, protocol);
    
    /// <summary>
    /// Unregister a translator from the global singleton registry.
    /// </summary>
    /// <param name="protocol">Protocol of the translator.</param>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    public static void UnregisterTranslator<TFrom, TTo>(string protocol = "")
        => SingletonInstance.Value.UnregisterTranslator(typeof(TFrom), typeof(TTo), protocol);
    
}
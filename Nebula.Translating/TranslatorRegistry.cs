using System.Collections.Concurrent;
using Nebula.Reporting;

namespace Nebula.Translating;

/// <summary>
/// A TranslatorRegistry is a center of translators. <br />
/// This class provides translator searching functions. <br />
/// </summary>
public class TranslatorRegistry
{
    /// <summary>
    /// All registered translators.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>>
        _translators = new();

    /// <summary>
    /// Verify whether the object is a translator satisfied the requirements.
    /// </summary>
    /// <param name="type">Type of the translator.</param>
    /// <param name="from">Expected original type.</param>
    /// <param name="to">Expected destination type.</param>
    /// <returns>True if the type satisfied the requirement; otherwise false.</returns>
    private static bool VerifyTranslator(Type type, Type from, Type to)
    {
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

            if (!from.IsAssignableTo(arguments[0]) )
            {
                Report.Warning("Failed to Verify Translator.",
                        "Expected type is not assignable to original type.")
                    .AttachDetails("Type", arguments[0])
                    .AttachDetails("ExpectedType", from)
                    .Handle();
                continue;
            }

            if (arguments[1].IsAssignableFrom(to)) return true;
            
            Report.Warning("Failed to Verify Translator.",
                    "Destination type is not assignable to expected type.")
                .AttachDetails("Type", arguments[0])
                .AttachDetails("ExpectedType", to)
                .Handle();
        }

        return false;
    }
    
    /// <summary>
    /// Register a translator in to this registry.
    /// <para>
    /// If the original type can be directly assigned to the destination type,
    /// then the protocol name can not be empty.
    /// </para>
    /// </summary>
    /// <param name="from">Original type,
    /// must be assignable <b>to</b> the declared original type of the translator.</param>
    /// <param name="to">Destination type,
    /// must be assignable <b>from</b> the declared original type of the translator.</param>
    /// <param name="translator">Translator to register.</param>
    /// <param name="protocol">Optional protocol name.</param>
    public void RegisterTranslator(Type from, Type to, object translator, string protocol = "")
    {
        if (from.IsAssignableTo(to) && protocol == "")
        {
            Report.Warning("Failed to Register Translator", 
                    "An nameless translator can not have the original type " +
                    "which can be directly assigned to destination type.", this)
                .AttachDetails("Translator", translator.GetType())
                .AttachDetails("OriginalType", from)
                .AttachDetails("DestinationType", to)
                .Handle();
            return;
        }

        if (!VerifyTranslator(translator.GetType(), from, to))
        {
            Report.Warning("Failed to Register Translator", 
                "Translator has not implemented ITranslator interface.", this)
                .AttachDetails("Translator", translator.GetType())
                .Handle();
            return;
        }
        var origins = _translators.GetOrAdd(from,
            _ => new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>());
        var destinations = origins.GetOrAdd(to, 
            _ => new ConcurrentDictionary<string, object>());
        destinations.TryAdd(protocol, translator);
    }

    /// <summary>
    /// Unregister a translator from this registry.
    /// </summary>
    /// <param name="from">Original type.</param>
    /// <param name="to">Destination type.</param>
    /// <param name="protocol">Protocol name.</param>
    public void UnregisterTranslator(Type from, Type to, string protocol = "")
    {
        if (!_translators.TryGetValue(from, out var origins))
            return;
        if (origins.TryGetValue(to, out var destinations))
            destinations.TryRemove(protocol, out _);
    }

    /// <summary>
    /// List all supported protocols of converting from to to.
    /// </summary>
    /// <param name="from">Original type.</param>
    /// <param name="to">Destination type.</param>
    /// <returns>Supported protocols, may be empty.</returns>
    public ICollection<string> GetProtocols(Type from, Type to)
    {
        if (!_translators.TryGetValue(from, out var origins))
            return Array.Empty<string>();
        return origins.TryGetValue(to, out var destinations) ? destinations.Keys :
            Array.Empty<string>();
    }

    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <param name="from">Object to translate.</param>
    /// <param name="toType">Destination type.</param>
    /// <param name="protocol">
    /// Protocol to use. Empty means that this any protocol is accepted.
    /// </param>
    /// <returns>Translated object, or null if no suitable translator found or translation failed.</returns>
    public object? Translate(object from, Type toType, string protocol = "")
        => Translate(from, from.GetType(), toType, protocol);

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
    public object? Translate(object from, Type fromType, Type toType, string protocol = "")
    {
        if (!fromType.IsInstanceOfType(from))
        {
            Report.Warning("Translation Failed",
                    "Object to translate does not match the given original type.", this)
                .AttachDetails("Object", from)
                .AttachDetails("OriginalType", fromType);
            return null;
        }
        
        if (protocol == "" && fromType.IsAssignableTo(toType))
            return from;
        
        if (!_translators.TryGetValue(fromType, out var origins))
            return null;
        if(!origins.TryGetValue(toType, out var destinations))
            return null;
        if (!destinations.TryGetValue(protocol, out var translator))
        {
            if (protocol == "" && destinations.Values.Count > 0)
                translator = destinations.Values.First();
        }

        return translator?.GetType().GetMethod("Translate")?.Invoke(translator, new []{from});
    }
}
using System.Collections.Concurrent;

namespace Nebula.Translating;

public class TranslatorRegistry
{
    /// <summary>
    /// All registered translators.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>>
        _translators = new();

    /// <summary>
    /// Register a translator in to this registry.
    /// </summary>
    /// <param name="from">Original type.</param>
    /// <param name="to">Destination type.</param>
    /// <param name="translator">Translator to register.</param>
    /// <param name="protocol">Optional protocol name.</param>
    public void RegisterTranslator(Type from, Type to, object translator, string protocol = "")
    {
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
    /// <param name="protocol">Protocol to use.</param>
    /// <returns>Translated object, or null if no suitable translator found or translation failed.</returns>
    public object? Translate(object from, Type toType, string protocol = "")
    {
        var fromType = from.GetType();
        if (!_translators.TryGetValue(fromType, out var origins))
            return null;
        if(!origins.TryGetValue(toType, out var destinations))
            return null;
        if (destinations.TryGetValue(protocol, out var translator))
        {
            if (protocol == "" && destinations.Values.Count > 0)
                translator = destinations.Values.First();
        }

        return translator?.GetType().GetMethod("Translate")?.Invoke(translator, new []{from});
    }
}
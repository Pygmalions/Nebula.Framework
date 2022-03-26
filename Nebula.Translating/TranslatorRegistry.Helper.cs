namespace Nebula.Translating;

public static class TranslatorRegistryHelper
{
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <typeparam name="TTo">Destination type.</typeparam>
    /// <param name="registry">Registry to use.</param>
    /// <param name="from">Object to translate.</param>
    /// <param name="protocol">Protocol to use. Empty means that this any protocol is accepted.</param>
    /// <returns>
    /// Translated <typeparamref name="TTo"/>,
    /// or null if no suitable translator found or translation failed.
    /// </returns>
    public static TTo? Translate<TTo>(this TranslatorRegistry registry, object from, string protocol = "")
        => (TTo?)registry.Translate(from, typeof(TTo), protocol);
    
    /// <summary>
    /// Try to translate an object to the specific type with a specific protocol if it is specified.
    /// </summary>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    /// <param name="registry">Registry to use.</param>
    /// <param name="from">Object to translate.</param>
    /// <param name="protocol">Protocol to use. Empty means that this any protocol is accepted.</param>
    /// <returns>
    /// Translated <typeparamref name="TTo"/>,
    /// or null if no suitable translator found or translation failed.
    /// </returns>
    public static TTo? Translate<TFrom, TTo>(this TranslatorRegistry registry, object from, string protocol = "")
        => (TTo?)registry.Translate(from, typeof(TFrom), typeof(TTo), protocol);

    /// <summary>
    /// Register a translator into this registry.
    /// </summary>
    /// <param name="registry">Registry to use.</param>
    /// <param name="translator">Translator to register.</param>
    /// <param name="protocol">Protocol of the translator.</param>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    public static void RegisterTranslator<TFrom, TTo>(this TranslatorRegistry registry,
        object translator, string protocol = "")
        => registry.RegisterTranslator(typeof(TFrom), typeof(TTo), translator, protocol);
    
    /// <summary>
    /// Unregister a translator from this registry.
    /// </summary>
    /// <param name="registry">Registry to use.</param>
    /// <param name="protocol">Protocol of the translator.</param>
    /// <typeparam name="TFrom">Original type.</typeparam>
    /// <typeparam name="TTo">Destination type.</typeparam>
    public static void UnregisterTranslator<TFrom, TTo>(this TranslatorRegistry registry, string protocol = "")
        => registry.UnregisterTranslator(typeof(TFrom), typeof(TTo), protocol);
}
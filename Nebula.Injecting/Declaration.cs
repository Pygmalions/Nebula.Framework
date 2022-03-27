namespace Nebula.Injecting;

public sealed class Declaration : Entry
{
    /// <summary>
    /// Cached singleton instance.
    /// </summary>
    public object? SingletonInstance { get; internal set; }
    
    /// <summary>
    /// Whether the object gotten from this source should be injected.
    /// </summary>
    public bool Singleton { get; private set; }

    /// <summary>
    /// Source that provides this instance.
    /// </summary>
    public Source Source { get; internal set; }

    /// <summary>
    /// Whether the entry is temporary and can be declared by other source or not.
    /// </summary>
    public bool Temporary { get; private set; }

    /// <summary>
    /// Customized additional data.
    /// </summary>
    public object? AdditionalData { get; set; }

    /// <summary>
    /// Set whether this declaration is temporary and can be reclaimed by other sources or not.
    /// </summary>
    /// <param name="temporary">Temporary or not.</param>
    /// <returns>This declaration.</returns>
    public Declaration SetTemporary(bool temporary)
    {
        Temporary = temporary;
        return this;
    }

    /// <summary>
    /// Set whether this object works in the singleton mode or not.
    /// If true, then the container will use the cached result for following requirements.
    /// </summary>
    /// <param name="singleton">Whether this object works in the singleton mode or not.</param>
    /// <returns>This declaration.</returns>
    public Declaration SetSingleton(bool singleton)
    {
        Singleton = singleton;
        return this;
    }

    /// <summary>
    /// Construct a declaration and bind it to a source and existing preset.
    /// This method will be invoked by the container.
    /// </summary>
    /// <param name="source">Source to bind.</param>
    /// <param name="preset">Preset given by the container.</param>
    internal Declaration(Source source, Preset preset) : base(preset)
    {
        Source = source;
    }
}
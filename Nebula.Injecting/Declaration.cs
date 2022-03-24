namespace Nebula.Injecting;

public sealed class Declaration : Entry
{
    /// <summary>
    /// Whether the object gotten from this source should be injected.
    /// </summary>
    public bool Injectable { get; private set; }

    /// <summary>
    /// Source that provides this instance.
    /// </summary>
    public Source Source { get; internal set; }

    /// <summary>
    /// Whether the entry is temporary and can be declared by other source or not.
    /// </summary>
    public bool Temporary { get; private set; }

    /// <summary>
    /// Instance cached for the next acquirement.
    /// Container will <b>NOT</b> query and use this instance,
    /// this instance is prepared for the source.
    /// </summary>
    public object? CachedInstance { get; set; }

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
    /// Set whether this declaration is injectable or not.
    /// If true, the container will inject the instance acquired from the source.
    /// </summary>
    /// <param name="injectable">Whether the container should be injected or not.</param>
    /// <returns>This declaration.</returns>
    public Declaration SetInjectable(bool injectable)
    {
        Injectable = injectable;
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
namespace Nebula.Injecting;

public abstract class Entry
{
    /// <summary>
    /// Preset for injection.
    /// </summary>
    public Preset Injection { get; }

    protected Entry(Preset injection)
    {
        Injection = injection;
    }
}
namespace Nebula.Proxying;

public interface IExtensibleProperty : IPropertyProxy
{
    /// <summary>
    /// Event triggered before the value is got.
    /// </summary>
    public event Action<AccessContext> Getting;
    /// <summary>
    /// Event triggered after the value is got.
    /// </summary>
    public event Action<AccessContext> AfterGetting;

    /// <summary>
    /// Event triggered before the value is set.
    /// </summary>
    public event Action<AccessContext> Setting;

    /// <summary>
    /// Event triggered after the value is set.
    /// </summary>
    public event Action<AccessContext> AfterSetting;

    /// <summary>
    /// Whether the proxied method accept null value as a return or not.
    /// </summary>
    public bool NullValueAccepted { get; }
}
namespace Nebula.Presetting.Features;

/// <summary>
/// This interface defines translatable array which can do the translation on it own.
/// </summary>
/// <typeparam name="TItem">Type of item that this preset indicates to use.</typeparam>
public interface IItem<out TItem> : IPreset
{
    TItem Translate();
}
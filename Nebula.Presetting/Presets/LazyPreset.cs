using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("Lazy")]
public class LazyPreset<TItem> : IItem<TItem>
{
    public Lazy<TItem> Item { get; set; }

    public TItem Translate() => Item.Value;

    public LazyPreset(Lazy<TItem> item)
    {
        Item = item;
    }
}
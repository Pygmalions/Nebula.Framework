using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("Array")]
public class PresetArray<TItem> : IArray<TItem>
{
    public List<IItem<TItem>> Items { get; set; }

    public TItem[] Translate()
    {
        return Items.Select(preset => preset.Translate()).ToArray();
    }
}
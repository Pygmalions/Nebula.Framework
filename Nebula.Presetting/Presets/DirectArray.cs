using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("Array")]
public class DirectArray<TItem> : IArray<TItem>
{
    public TItem[] Items;

    public TItem[] Translate() => Items;

    public DirectArray(params TItem[] items)
    {
        Items = items;
    }

    private static readonly Lazy<DirectArray<TItem>> SingletonEmptyInstance =
        new(() => new DirectArray<TItem>());

    public static DirectArray<TItem> EmptyArray => SingletonEmptyInstance.Value;
}
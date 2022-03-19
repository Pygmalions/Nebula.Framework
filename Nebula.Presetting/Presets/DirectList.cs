using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("List")]
public class DirectList<TItem> : IArray<TItem>
{
    public List<TItem> Items { get; }

    public TItem[] Translate() => Items.ToArray();

    public DirectList(params TItem[] items)
    {
        Items = items.ToList();
    }

    private static readonly Lazy<DirectArray<TItem>> SingletonEmptyInstance =
        new Lazy<DirectArray<TItem>>(() => new DirectArray<TItem>());

    public static DirectArray<TItem> EmptyArray => SingletonEmptyInstance.Value;
}
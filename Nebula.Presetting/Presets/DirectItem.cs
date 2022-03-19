using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("Item")]
public class DirectItem<TItem> : IItem<TItem>
{
    public TItem Item;

    public TItem Translate() => Item;

    public DirectItem(TItem item)
    {
        Item = item;
    }
}

[Preset("Null")]
public class NullItem : DirectItem<object?>
{
    private NullItem() : base(null)
    {}
    
    private static readonly Lazy<NullItem> SingletonInstance = new (() => new NullItem());

    public static NullItem Instance => SingletonInstance.Value;
}
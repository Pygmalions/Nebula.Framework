using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

[Preset("Lambda")]
public class LambdaPreset<TItem> : IItem<TItem>
{
    public Func<TItem> Functor { get; set; }

    public TItem Translate() => Functor();

    public LambdaPreset(Func<TItem> functor)
    {
        Functor = functor;
    }
}
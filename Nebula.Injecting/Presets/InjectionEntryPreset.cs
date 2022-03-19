using Nebula.Presetting;
using Nebula.Presetting.Features;

namespace Nebula.Injecting.Presets;

[Preset("Entry")]
public class InjectionEntryPreset : IPreset
{
    [Property("Name")]
    public IItem<string> Name;

    [Content]
    public IItem<object?> Content;

    public InjectionEntryPreset(IItem<string> name, IItem<object?> content)
    {
        Name = name;
        Content = content;
    }
}
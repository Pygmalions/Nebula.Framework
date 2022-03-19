using Nebula.Injecting.Presets;
using Nebula.Presetting.Features;
using Nebula.Presetting.Presets;

namespace Nebula.Injecting;

public class InjectionBuilder
{
    private readonly Dictionary<string, IItem<object?>> _entries = new();

    public InjectionPreset BuildPreset()
    {
        var injections = new DirectList<InjectionEntryPreset>();
        foreach (var (name, item) in _entries)
        {
            injections.Items.Add(new InjectionEntryPreset(new DirectItem<string>(name), item));
        }

        return new InjectionPreset(injections);
    }

    public InjectionBuilder InjectObject(string name, object? value)
    {
        _entries[name] = new DirectItem<object?>(value);
        return this;
    }
    
    public InjectionBuilder InvokeMethod(string name, params object?[]? arguments)
    {
        _entries[name] = new DirectItem<object?[]?>(arguments);
        return this;
    }

    public InjectionBuilder InjectPreset(string name, Presetting.Features.IItem<object?> preset)
    {
        _entries[name] = preset;
        return this;
    }

    public void Clear()
    {
        _entries.Clear();
    }
}
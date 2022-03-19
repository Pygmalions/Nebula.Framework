using Nebula.Presetting;
using Nebula.Presetting.Features;

namespace Nebula.Injecting.Presets;

[Preset("Injection")]
public class InjectionPreset : IPreset
{
    /// <summary>
    /// Support injections for fields, properties, and methods.
    /// <para>
    ///     The searching order is firstly fields, secondly properties and lastly methods.
    ///     If there is a member of the searching member type matching the given name, and it is writable
    ///     and assignable from the given value, then it will be matched.
    ///     An injection entry will finish until one or none of members matched.
    /// </para>
    /// <para>
    ///     Attention, if the injection target is a method, then the value must an object?[].
    ///     Otherwise the matching will failed.
    /// </para>
    /// </summary>
    [Content]
    public IArray<InjectionEntryPreset> Entries;

    public InjectionPreset(IArray<InjectionEntryPreset> entries)
    {
        Entries = entries;
    }
}
using Nebula.Presetting.Features;

namespace Nebula.Presetting.Presets;

/// <summary>
/// This preset indicates the details to generate an object with
/// <see cref="Activator.CreateInstance(Type, object?[])"/>,
/// and also provides injection functions.
/// </summary>
[Preset("Construction")]
public class ObjectConstructionPreset : IItem<object?>
{
    /// <summary>
    /// Type of the class to instantiate.
    /// </summary>
    [Property("Class")]
    public IItem<Type> Class;

    /// <summary>
    /// Arguments to pass to the constructor.
    /// </summary>
    [Content]
    public IArray<object?> Arguments;

    public ObjectConstructionPreset(IItem<Type> type, IArray<object?> arguments)
    {
        Class = type;
        Arguments = arguments;
    }

    public object? Translate() => Activator.CreateInstance(Class.Translate(), Arguments.Translate());
}
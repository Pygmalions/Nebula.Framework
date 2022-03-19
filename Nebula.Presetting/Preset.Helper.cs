using System.Reflection;
using Nebula.Exceptions;
using Nebula.Presetting.Features;

namespace Nebula.Presetting;

public static class PresetHelper
{
    /// <summary>
    /// Try to translate this preset into an object, no matter whether it is <see cref="IItem{TItem}"/>
    /// or <see cref="IArray{TItem}"/>.
    /// </summary>
    /// <param name="preset">Preset to translate.</param>
    /// <param name="translation">Translation result.</param>
    /// <returns>
    /// If true, this preset is successfully translated,
    /// otherwise this preset is not translatable, which means it can not done the translation on 
    /// </returns>
    public static bool TryTranslate(this IPreset preset, out object? translation)
    {
        translation = null;
        switch (preset)
        {
            case IItem<object?> item:
                translation = item.Translate();
                return true;
            case IArray<object?> array:
                translation = array.Translate();
                return true;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Get the content preset of this preset.
    /// </summary>
    /// <param name="preset">Preset instance to scan with.</param>
    /// <returns>Instance of content preset.</returns>
    /// <exception cref="UserError">
    /// Throw if type of the given preset is not marked with <see cref="PresetAttribute"/>.
    /// </exception>
    public static IPreset? GetContentPresets(this IPreset preset)
    {
        var presetType = preset.GetType();
        var presetAttribute = presetType.GetCustomAttribute<PresetAttribute>();
        if (presetAttribute == null)
            throw new UserError($"Preset {presetType.Name} must be marked with " +
                                $"{nameof(PresetAttribute)} to enable member presets scanning.");
        presetAttribute.ScanMemberPresets(presetType);
        return presetAttribute.ContentPreset switch
        {
            FieldInfo field => field.GetValue(preset) as IPreset,
            PropertyInfo property => property.GetValue(preset) as IPreset,
            _ => null
        };
    }

    /// <summary>
    /// Get the property presets of this preset.
    /// </summary>
    /// <param name="preset">Preset instance to scan with.</param>
    /// <returns>Name-Instance dictionary of property presets.</returns>
    /// <exception cref="UserError">
    /// Throw if type of the given preset is not marked with <see cref="PresetAttribute"/>.
    /// </exception>
    /// <exception cref="RuntimeError">
    /// Throw if failed to get the instance of any property preset.
    /// </exception>
    public static IReadOnlyDictionary<string, IPreset> GetPropertyPresets(this IPreset preset)
    {
        var presets = new Dictionary<string, IPreset>();
        
        var presetType = preset.GetType();
        var presetAttribute = presetType.GetCustomAttribute<PresetAttribute>();
        if (presetAttribute == null)
            throw new UserError($"Preset {presetType.Name} must be marked with " +
                                $"{nameof(PresetAttribute)} to enable member presets scanning.");
        presetAttribute.ScanMemberPresets(presetType);

        if (presetAttribute.PropertyPresets == null)
            return new Dictionary<string, IPreset>();
        
        foreach (var (name, member) in presetAttribute.PropertyPresets)
        {
            switch (member)
            {
                case FieldInfo field:
                {
                    var memberPreset = field.GetValue(preset) as IPreset;
                    if (memberPreset == null)
                        throw new RuntimeError($"Failed to get the instance of property preset " +
                                               $"{field.Name} with name {name}");
                    presets.Add(name, memberPreset);
                    break;
                }
                case PropertyInfo property:
                {
                    var memberPreset = property.GetValue(preset) as IPreset;
                    if (memberPreset == null)
                        throw new RuntimeError($"Failed to get the instance of property preset " +
                                               $"{property.Name} with name {name}");
                    presets.Add(name, memberPreset);
                    break;
                }
            }
        }

        return presets;
    }
}
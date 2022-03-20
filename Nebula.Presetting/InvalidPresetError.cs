using Nebula.Exceptions;

namespace Nebula.Presetting;

public class InvalidPresetError : UserError
{
    public IPreset InvalidPreset { get; }

    public InvalidPresetError(IPreset preset, string message) :
        base($"Preset {preset.GetType().Name} is invalid: {message}.")
    {
        InvalidPreset = preset;
    }
}
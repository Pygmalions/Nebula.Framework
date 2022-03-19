namespace Nebula.Presetting;

[AttributeUsage(AttributeTargets.Class)]
public partial class PresetAttribute : Attribute
{
    /// <summary>
    /// Name of this kind of preset.
    /// </summary>
    public string Name { get; }

    public PresetAttribute(string name)
    {
        Name = name;
    }
}
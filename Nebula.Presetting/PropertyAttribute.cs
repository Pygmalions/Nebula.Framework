namespace Nebula.Presetting;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PropertyAttribute : Attribute
{
    public string Name { get; }

    public PropertyAttribute(string name)
    {
        Name = name;
    }
}
using System.Reflection;
using Nebula.Injecting.Presets;

namespace Nebula.Injecting;

public class PresetInjector
{
    public static void Inject(object instance, InjectionPreset preset)
    {
        foreach (var entry in preset.Entries.Translate())
        {
            var members = instance.GetType().GetMember(entry.Name.Translate(), 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (members.Length == 0)
                continue;
            var content = entry.Content.Translate();
            foreach (var member in members)
            {
                switch (member)
                {
                    case FieldInfo field:
                        if (!field.IsInitOnly && field.FieldType.IsInstanceOfType(content))
                            field.SetValue(instance, content);
                        break;
                    case PropertyInfo property:
                        if (property.CanWrite && property.PropertyType.IsInstanceOfType(content))
                            property.SetValue(instance, content);
                        break;
                    case MethodInfo method:
                        if (method.IsAbstract)
                            break;
                        switch (content)
                        {
                            case null:
                                method.Invoke(instance, null);
                                break;
                            case object?[] parameters:
                                method.Invoke(instance, parameters);
                                break;
                        }
                        break;
                }
            }
        }
    }
}
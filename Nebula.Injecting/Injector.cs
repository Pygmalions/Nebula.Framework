using System.Reflection;
using Nebula.Injecting.Presets;
using Nebula.Resource;
using Nebula.Resource.Identifiers;

namespace Nebula.Injecting;

public static partial class Injector
{
    /// <summary>
    /// Inject the specific preset into the given instance.
    /// </summary>
    /// <param name="instance">Object to inject the preset.</param>
    /// <param name="preset">Preset which describes members of the instance to inject.</param>
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

    /// <summary>
    /// Scan the members and inject according to the <see cref="InjectionAttribute"/> marked on them.
    /// </summary>
    /// <param name="instance">Instance to scan and inject.</param>
    /// <param name="container">Container which provides resource to inject.</param>
    public static void Inject(object instance, Container container)
    {
        
        var members = instance.GetType().GetMembers(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        foreach (var member in members)
        {
            var injectionAttribute = member.GetCustomAttribute<InjectionAttribute>();
            if (injectionAttribute == null)
                continue;
            object? content = null;
            IIdentifier identifier = injectionAttribute.Name != null
                ? new NameIdentifier(injectionAttribute.Name)
                : WildcardIdentifier.Anything;
            switch (member)
            {
                case FieldInfo field:
                    if (field.IsInitOnly)
                        continue;
                    content = container.Acquire(field.FieldType,
                        identifier, injectionAttribute.Scopes);
                    if (content == null)
                        continue;
                    field.SetValue(instance, content);
                    break;
                case PropertyInfo property:
                    if (!property.CanWrite)
                        continue;
                    content = container.Acquire(property.PropertyType, identifier,
                        injectionAttribute.Scopes);
                    if (content == null)
                        continue;
                    property.SetValue(instance, content);
                    break;
                case MethodInfo method:
                    if (method.IsAbstract)
                        break;
                    var parameters = PrepareParameters(method, container);
                    if (parameters != null)
                        method.Invoke(instance, parameters);
                    break;
            }
        }
    }

    public static TType? Inject<TType>(Container container) where TType : class
    {
        var type = typeof(TType);

        foreach (var constructor in type.GetConstructors())
        {
            var attribute = constructor.GetCustomAttribute<InjectionAttribute>();
            if (attribute == null)
                continue;
            var parameters = PrepareParameters(constructor, container);
            if (parameters == null)
                continue;
            return Activator.CreateInstance(type, parameters) as TType;
        }

        return Activator.CreateInstance(type, null) as TType;
    }
}
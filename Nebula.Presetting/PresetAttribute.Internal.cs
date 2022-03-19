using System.Reflection;
using Nebula.Exceptions;

namespace Nebula.Presetting;

public partial class PresetAttribute
{
    internal bool Scanned { get; private set; }
    internal MemberInfo? ContentPreset { get; private set; }
    internal IReadOnlyDictionary<string, MemberInfo>? PropertyPresets { get; private set; }

    internal void ScanMemberPresets(Type type)
    {
        if (Scanned) return;
        var candidateMembers = new List<MemberInfo>();
        candidateMembers.AddRange(type.GetFields());
        candidateMembers.AddRange(type.GetProperties());

        var propertyPresets = new Dictionary<string, MemberInfo>();
        
        foreach (var member in candidateMembers)
        {
            var contentAttribute = member.GetCustomAttribute<ContentAttribute>();
            if (contentAttribute != null)
            {
                if (ContentPreset != null)
                    throw new UserError(
                        $"Only one member preset in {type.Name} " +
                        $"can be marked with {nameof(ContentAttribute)}.");
                ContentPreset = member switch
                {
                    PropertyInfo property when !property.CanRead => throw new UserError(
                        $"Content preset {property.Name} is not allowed to read."),
                    PropertyInfo property when !property.PropertyType.IsAssignableTo(typeof(IPreset)) => throw
                        new UserError($"{nameof(ContentAttribute)} can only " +
                                      $"be marked on member implements {nameof(IPreset)}."),
                    FieldInfo field when !field.FieldType.IsAssignableTo(typeof(IPreset)) => throw new UserError(
                        $"{nameof(ContentAttribute)} can only " + $"be marked on member implements {nameof(IPreset)}."),
                    _ => member
                };
            }

            var propertyAttribute = member.GetCustomAttribute<PropertyAttribute>();
            if (propertyAttribute == null)
                continue;
            if (contentAttribute != null)
                throw new UserError($"Member preset {member.Name} can not be marked with both of" +
                                    $" {nameof(ContentAttribute)} and {nameof(PropertyAttribute)}.");
            
            ContentPreset = member switch
            {
                PropertyInfo { CanRead: false } property => throw new UserError(
                    $"Property preset {property.Name} is not allowed to read."),
                PropertyInfo property when !property.PropertyType.IsAssignableTo(typeof(IPreset)) => throw
                    new UserError($"{nameof(PropertyAttribute)} can only " +
                                  $"be marked on member implements {nameof(IPreset)}."),
                FieldInfo field when !field.FieldType.IsAssignableTo(typeof(IPreset)) => throw new UserError(
                    $"{nameof(PropertyAttribute)} can only " + $"be marked on member implements {nameof(IPreset)}."),
                _ => member
            };
            
            propertyPresets.Add(propertyAttribute.Name, member);
        }

        PropertyPresets = propertyPresets;
        Scanned = true;
    }
}
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
                {
                    ErrorCenter.Report<UserError>(Importance.Warning,
                        $"Only one member preset in {type.Name} " +
                        $"can be marked with {nameof(ContentAttribute)}.");
                    continue;
                }

                switch (member)
                {
                    case PropertyInfo property when !property.CanRead:
                        ErrorCenter.Report<UserError>(Importance.Error,
                            $"Content preset {property.Name} is not allowed to read.");
                        continue;
                    case PropertyInfo property when !property.PropertyType.IsAssignableTo(typeof(IPreset)):
                    case FieldInfo field when !field.FieldType.IsAssignableTo(typeof(IPreset)):
                        ErrorCenter.Report<UserError>(Importance.Error,
                            $"{nameof(ContentAttribute)} can only " +
                            $"be marked on member implements {nameof(IPreset)}.");
                        continue;
                    default:
                        ContentPreset = member;
                        break;
                }
            }

            var propertyAttribute = member.GetCustomAttribute<PropertyAttribute>();
            if (propertyAttribute == null)
                continue;
            if (contentAttribute != null)
            {
                ErrorCenter.Report<UserError>(Importance.Warning,
                    $"Member preset {member.Name} can not be marked with both of" +
                    $" {nameof(ContentAttribute)} and {nameof(PropertyAttribute)}.");
            }

            switch (member)
            {
                case PropertyInfo { CanRead: false } property:
                    ErrorCenter.Report<UserError>(Importance.Error, 
                        $"Property preset {property.Name} is not allowed to read.");
                    continue;
                case PropertyInfo property when !property.PropertyType.IsAssignableTo(typeof(IPreset)):
                case FieldInfo field when !field.FieldType.IsAssignableTo(typeof(IPreset)):
                    ErrorCenter.Report<UserError>(Importance.Error, 
                        $"{nameof(PropertyAttribute)} can only " +
                        $"be marked on member implements {nameof(IPreset)}.");
                    continue;
                default:
                    ContentPreset = member;
                    break;
            }

            propertyPresets.Add(propertyAttribute.Name, member);
        }

        PropertyPresets = propertyPresets;
        Scanned = true;
    }
}
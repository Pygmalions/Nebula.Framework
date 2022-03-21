using System.Reflection;
using Nebula.Exceptions;
using Nebula.Resource;
using Nebula.Resource.Identifiers;

namespace Nebula.Injecting;

public static partial class Injector
{
    public static object?[]? PrepareParameters(MethodBase method, Container container)
    {
        var methodParameters = method.GetParameters();
        var parameterContents = new List<object?>();
        var injectionSatisfied = true;
        foreach (var parameter in methodParameters)
        {
            var parameterAttribute = parameter.GetCustomAttribute<InjectionAttribute>();
            if (parameterAttribute == null)
            {
                ErrorCenter.Report<UserError>(Importance.Warning,
                    $"Injection on method {method.Name} can only enable when all " +
                    $"parameters have {nameof(InjectionAttribute)} on them.");
                injectionSatisfied = false;
                break;
            }

            IIdentifier parameterContentIdentifier = parameterAttribute.Name != null
                ? new NameIdentifier(parameterAttribute.Name)
                : WildcardIdentifier.Anything;
            var parameterContent = container.Acquire(parameter.ParameterType,
                parameterContentIdentifier, parameterAttribute.Scopes);
            if (parameterContent == null &&
                !(parameter.ParameterType.IsGenericType &&
                  parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                injectionSatisfied = false;
                break;
            }
                        
            parameterContents.Add(parameterContent);
        }

        return injectionSatisfied ? parameterContents.ToArray() : null;
    }
}
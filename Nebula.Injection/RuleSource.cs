using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using Nebula.Injection.Rules;

namespace Nebula.Injection;

public class RuleSource : ISource
{
    private readonly ConcurrentDictionary<Type, IRule> _ruleset = new();

    /// <inheritdoc />
    public void Configure(object instance)
    {
        var instanceType = instance.GetType();
        foreach (var field in instanceType.GetFields())
        {
            if (field.IsInitOnly) continue;
            var attribute = field.GetCustomAttribute<InjectionAttribute>();
            if (attribute == null) continue;
            field.SetValue(instance, Acquire(field.FieldType, attribute));
        }
        foreach (var property in instanceType.GetProperties())
        {
            if (!property.CanWrite) continue;
            var attribute = property.GetCustomAttribute<InjectionAttribute>();
            if (attribute == null) continue;
            property.SetValue(instance, Acquire(property.PropertyType, attribute));
        }
        foreach (var method in instanceType.GetMethods(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (method.IsAbstract || method.IsStatic) continue;
            var attribute = method.GetCustomAttribute<InjectionAttribute>();
            if (attribute == null) continue;
            var parameters = method.GetParameters();
            if (parameters.Length == 0) continue;
            var arguments = new List<object?>(parameters.Length);
            arguments.AddRange(
                parameters.Select(
                    parameter => Acquire(
                        parameter.ParameterType, parameter.GetCustomAttribute<InjectionAttribute>())));
            method.Invoke(instance, arguments.ToArray());
        }
    }

    /// <summary>
    /// Acquire the instance of the given type according to the given attribute.
    /// If the required type is not abstract or interface, and it is not in this source,
    /// then a <see cref="BindingRule"/> will be created and registered for it.
    /// </summary>
    /// <param name="type">Type of instance to acquire. If nullable, will be considered as its underlying type.</param>
    /// <param name="attribute">Attribute which provides external injection information.</param>
    /// <returns>Instance as required.</returns>
    /// <exception cref="Exception">
    /// Ruleset source does not have the rule for the requiring abstract or interface type.
    /// </exception>
    public object Acquire(Type type, InjectionAttribute? attribute)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = type.GetGenericArguments()[0];
        if (_ruleset.TryGetValue(type, out var rule)) 
            return rule.Get(this, type, attribute);
        if (type.IsAbstract || type.IsInterface)
            throw new Exception($"Ruleset source does not have the rule for the requiring " +
                                $"abstract or interface type {{type.Name}}.");
        rule = new BindingRule(type);
        _ruleset.TryAdd(type, new BindingRule(type));
        return rule.Get(this, type, attribute);
    }

    /// <inheritdoc />
    public bool Acquirable(Type type, InjectionAttribute? attribute)
    {
        if (!type.IsAbstract && !type.IsInterface) return true;
        return _ruleset.TryGetValue(type, out var rule) && rule.Acceptable(this, type, attribute);
    }

    /// <summary>
    /// Set and get a rule of this ruleset source.
    /// </summary>
    /// <param name="type">Type of the rule to get or set.</param>
    public IRule this[Type type]
    {
        get => _ruleset[type];
        set => _ruleset[type] = value;
    }
}
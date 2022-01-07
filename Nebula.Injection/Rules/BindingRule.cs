using System.Reflection;

namespace Nebula.Injection.Rules;

public class BindingRule : IRule
{
    private readonly Type _boundType;

    private readonly Dictionary<PropertyInfo, object?> _presetProperties = new();
    private readonly Dictionary<FieldInfo, object?> _presetFields = new();
    private readonly Dictionary<MethodInfo, object?[]?> _presetInvocations = new();

    private ISource? _sourceForSelectedConstructor;
    private ConstructorInfo? _selectedConstructor;

    private object?[]? _boundArguments;

    public BindingRule(Type type)
    {
        _boundType = type;
    }

    /// <inheritdoc />
    public object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        object? instance = null;
        if (_selectedConstructor == null || source != _sourceForSelectedConstructor)
        {
            _selectedConstructor = SearchConstructor(source);
            if (_selectedConstructor == null)
                throw new Exception("No matched constructor found.");
            _sourceForSelectedConstructor = source;
        }

        var constructorArguments = _boundArguments;
        
        var constructorParameters = _selectedConstructor.GetParameters();
        if (constructorArguments == null && constructorParameters.Length > 0)
        {
            constructorArguments = new object?[constructorParameters.Length];
            // Get all the needed injections from the source.
            for (var parameterIndex = 0; parameterIndex < constructorParameters.Length; ++parameterIndex)
            {
                var constructorParameter = constructorParameters[parameterIndex];
                constructorArguments[parameterIndex] = source.Acquire(
                    constructorParameter.ParameterType,
                    constructorParameter.GetCustomAttribute<InjectionAttribute>());
            }
        }
        // If arguments mismatch, then the CLR will throw an exception, so there no need to check it.
        instance = _selectedConstructor.Invoke(constructorArguments);
        source.Configure(instance);
        foreach (var (field, value) in _presetFields)
        {
            field.SetValue(instance, value);
        }
        foreach (var (property, value) in _presetProperties)
        {
            property.SetValue(instance, value);
        }
        foreach (var (method, arguments) in _presetInvocations)
        {
            method.Invoke(instance, arguments);
        }

        return instance;
    }
    
    /// <summary>
    /// Search a constructor of which all parameters can be provided by the given source.
    /// </summary>
    /// <param name="source">Source to search with.</param>
    /// <returns>Constructor found.</returns>
    private ConstructorInfo? SearchConstructor(ISource source)
    {
        ConstructorInfo? attributedConstructor = null;
        ConstructorInfo? defaultConstructor = null;
        ConstructorInfo? acceptableConstructor = null;
        foreach (var constructor in _boundType.GetConstructors())
        {
            // Check whether arguments match the parameters or not.
            var parameters = constructor.GetParameters();
            var parametersMatched = 
                parameters.All(parameter => source.Acquirable(
                    parameter.ParameterType, parameter.GetCustomAttribute<InjectionAttribute>()));
            if (!parametersMatched) continue;
            
            if (constructor.GetCustomAttribute<InjectionAttribute>() != null)
            {
                attributedConstructor = constructor;
                break;
            }

            if (parameters.Length == 0)
                defaultConstructor = constructor;
            else acceptableConstructor = constructor;
        }

        if (attributedConstructor != null)
            return attributedConstructor;
        return defaultConstructor != null ? defaultConstructor : acceptableConstructor;
    }
    
    /// <inheritdoc />
    public bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        if (_selectedConstructor != null) return true;
        _selectedConstructor = SearchConstructor(source);
        _sourceForSelectedConstructor = source;
        return _selectedConstructor != null;
    }

    /// <summary>
    /// Bind constructor arguments.
    /// </summary>
    /// <param name="arguments">Arguments for constructor to bind.</param>
    /// <exception cref="Exception">
    /// Throw if no constructor matches the given arguments can be found.
    /// </exception>
    public BindingRule BindArguments(params object?[] arguments)
    {
        var constructorMatched = false;
        var attributedMode = false;
        foreach (var constructor in _boundType.GetConstructors())
        {
            var parameters = constructor.GetParameters();
            if (arguments.Length != parameters.Length) continue;
            var attribute = constructor.GetCustomAttribute<InjectionAttribute>();
            if (attributedMode && attribute == null)
                continue;
            
            var parametersMatched = true;
            
            for (var index = 0; index < parameters.Length; ++index)
            {
                var parameterType = parameters[index].ParameterType;
                var argument = arguments[index];
                if (argument == null)
                {
                    if (parameterType.IsGenericType &&
                          parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        continue;
                    parametersMatched = false;
                    break;
                }
                if (argument.GetType().IsAssignableTo(parameterType))
                    continue;
                parametersMatched = false;
                break;
            }

            if (!parametersMatched) continue;
            
            
            if (attribute != null && !attributedMode)
            {
                attributedMode = true;
            }

            _selectedConstructor = constructor;
            constructorMatched = true;
        }

        if (!constructorMatched)
            throw new Exception(
                "Failed to bind constructor arguments: no suitable constructor matched for given arguments.");
        _boundArguments = arguments;
        return this;
    }

    /// <summary>
    /// Set a field value after a instance is created.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="value">Value to set.</param>
    /// <exception cref="Exception">
    /// Throw if field with the given name can not be found,
    /// or it can not accept the given value.
    /// </exception>
    public BindingRule SetField(string name, object? value)
    {
        var field = _boundType.GetField(name);
        if (field == null)
            throw new Exception("Can not preset field value: " +
                                $"{_boundType.Name} does not have a field named {name}.");
        if (field.IsInitOnly)
            throw new Exception("Can not preset field value: " +
                                $"{_boundType.Name}::{name} is for initialization only.");
        var fieldType = field.FieldType;
        if (value == null && !(fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            throw new Exception("Can not preset field value: " +
                            $"{_boundType.Name}::{name} is not nullable while attempting to set it to null.");
        if (fieldType.IsAssignableFrom(value?.GetType()))
            throw new Exception("Can not preset field value: " +
                                $"{_boundType.Name}::{name} does not accept a value of {value.GetType().Name}.");
        _presetFields.Add(field, value);
        return this;
    }

    /// <summary>
    /// Set a property value after a instance is created.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="value">Value to set.</param>
    /// <exception cref="Exception">
    /// Throw if property with the given name can not be found,
    /// or it can not accept the given value.
    /// </exception>
    public BindingRule SetProperty(string name, object? value)
    {
        var property = _boundType.GetProperty(name);
        if (property == null)
            throw new Exception("Can not preset property value: " +
                                $"{_boundType.Name} does not have a property named {name}.");
        if (!property.CanWrite)
            throw new Exception("Can not preset property value: " +
                                $"{_boundType.Name}::{name} is for initialization only.");
        var propertyType = property.PropertyType;
        if (value == null && !(propertyType.IsGenericType &&
                               propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            throw new Exception("Can not preset property value: " +
                                $"{_boundType.Name}::{name} is not nullable while attempting to set it to null.");
        if (propertyType.IsAssignableFrom(value?.GetType()))
            throw new Exception("Can not preset field value: " +
                                $"{_boundType.Name}::{name} does not accept a value of {value.GetType().Name}.");
        _presetProperties.Add(property, value);
        return this;
    }

    /// <summary>
    /// Invoke a method after a instance is created.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    /// <param name="arguments">Optional arguments to pass to the method.</param>
    /// <exception cref="Exception">
    /// Throw if no method matching the given name and arguments can be found.
    /// </exception>
    public BindingRule InvokeMethod(string name, params object?[] arguments)
    {
        MethodInfo? matchedMethod = null;
        foreach (var method in _boundType.GetMethods())
        {
            if (method.Name != name)
                continue;
            if (method.IsAbstract)
                continue;
            var parameters = method.GetParameters();
            if (arguments.Length != parameters.Length)
                continue;

            var parametersMatched = true;
            for (var index = 0; index < parameters.Length; ++index)
            {
                var parameterType = parameters[index].ParameterType;
                var argument = arguments[index];
                if (argument == null)
                {
                    if (parameterType.IsGenericType &&
                        parameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        continue;
                    parametersMatched = false;
                    break;
                }
                if (argument.GetType().IsAssignableTo(parameterType))
                    continue;
                parametersMatched = false;
                break;
            }

            if (!parametersMatched) continue;
            matchedMethod = method;
        }

        if (matchedMethod == null)
            throw new Exception("Can not preset method invocation: " +
                                $"{_boundType.Name} does not have a method named {name}, " +
                                "or none of them matches the given arguments.");
        
        _presetInvocations.Add(matchedMethod, arguments);
        return this; 
    }
}
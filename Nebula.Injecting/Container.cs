using System.Collections.Concurrent;
using System.Reflection;
using Nebula.Reporting;

namespace Nebula.Injecting;

public class Container
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Definition>> _definitions = new();

    /// <summary>
    /// Declare an object in this container.
    /// </summary>
    /// <param name="category">Category type.</param>
    /// <param name="name">Name of the object.</param>
    /// <returns>Object definition.</returns>
    public Definition Declare(Type category, string? name = null)
    {
        var group = 
            _definitions.GetOrAdd(category, new ConcurrentDictionary<string, Definition>());
        return group.GetOrAdd(name ?? "", _ =>
        {
            var definition = new Definition();
            if (!category.IsInterface && !category.IsAbstract)
                definition.BindClass(category);
            return definition;
        });
    }

    /// <summary>
    /// Revoke an object definition in this container.
    /// </summary>
    /// <param name="category">Category type.</param>
    /// <param name="name">Name of the object.</param>
    public void Revoke(Type category, string? name = null)
    {
        if (!_definitions.TryGetValue(category, out var group))
            return;
        group.TryRemove(name ?? "", out _);
    }

    /// <summary>
    /// Get an object from this container.
    /// </summary>
    /// <param name="category">Category type of the object.</param>
    /// <param name="name">Name of the object.</param>
    /// <returns>Object instance, or null if not found.</returns>
    public object? Get(Type category, string? name = null)
        => Get(category, name, null, null);
    
    /// <summary>
    /// Get an object from the container with optional injection information.
    /// </summary>
    /// <param name="category">Category type of the object.</param>
    /// <param name="name">Name of the object.</param>
    /// <param name="member">Member information of the member to inject.</param>
    /// <param name="holder">Object which holds the member to inject.</param>
    /// <returns>Object instance, or null if not found.</returns>
    private object? Get(Type category, string? name, MemberInfo? member, object? holder)
    {
        if (!_definitions.TryGetValue(category, out var group))
            return null;
        return group.TryGetValue(name ?? "", out var definition) ? 
            definition.Get(this, member, holder) : 
            group.TryGetValue("*", out var creator) ? 
                creator.Get(this, member, holder) : null;
    }
    
    /// <summary>
    /// Construct an instance using the objects in this container to inject into the constructor.
    /// </summary>
    /// <param name="type">Type to instantiate.</param>
    /// <returns>Constructed instance.</returns>
    /// <exception cref="ReportException">
    /// Thrown if instantiation failed.
    /// </exception>
    public object Construct(Type type)
    {
        ConstructorInfo? attributedConstructor = null;
        ConstructorInfo? defaultConstructor = null;
        ConstructorInfo? suitableConstructor = null;
        object?[]? suitableArguments = null;
        
        foreach (var constructor in type.GetConstructors().OrderBy(constructor => constructor.GetParameters().Length))
        {
            if (constructor.GetParameters().Length == 0)
            {
                defaultConstructor = constructor;
                continue;
            }

            var injectionAttribute = constructor.GetCustomAttribute<InjectionAttribute>();
            if (injectionAttribute != null)
            {
                attributedConstructor = constructor;
                break;
            }

            if (!FindArguments(constructor, out suitableArguments)) 
                continue;
            suitableConstructor = constructor;
            break;
        }

        var selectedConstructor = attributedConstructor != null ? attributedConstructor :
            suitableConstructor != null ? suitableConstructor : defaultConstructor;
        if (selectedConstructor == null)
            throw Report.Error("Failed to Construct", "No suitable constructor found.", this)
                .AttachDetails("Type", type)
                .NotifyAsException();
        
        if (suitableConstructor == null)
            if (!FindArguments(selectedConstructor, out suitableArguments))
                throw Report.Error("Failed to Construct", "Can not find all arguments.", this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Constructor", selectedConstructor)
                    .NotifyAsException();

        object? instance = null;
        
        ActionReport.BeginAction("Construct Object", owner: this)
            .DoAction(()=> instance = selectedConstructor.Invoke(suitableArguments))
            .OnFailed(report => report.AttachDetails("Type", type)
                .AttachDetails("Constructor", selectedConstructor)
                .AttachDetails("Arguments", suitableArguments ?? Array.Empty<object?>()))
            .FinishAction();

        if (instance == null)
            throw Report.Error("Failed to Construct", "Constructor returns null.", this)
                .AttachDetails("Type", type)
                .AttachDetails("Constructor", selectedConstructor)
                .NotifyAsException();
        
        return instance;
    }

    /// <summary>
    /// Use the objects in this container to do the attribute injection.
    /// </summary>
    /// <param name="instance">Instance to inject.</param>
    public void Inject(object instance)
    {
        var type = instance.GetType();

        foreach (var member in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
        {
            var injectionAttribute = member.GetCustomAttribute<InjectionAttribute>();
            
            if (injectionAttribute == null)
                continue;

            switch (member)
            {
                case FieldInfo field:
                    if (field.IsInitOnly)
                    {
                        Report.Warning("Failed to Inject", "Field is init-only.", this)
                            .AttachDetails("Type", type)
                            .AttachDetails("Member", member)
                            .Handle();
                        continue;
                    }

                    ActionReport.BeginAction("Inject Field", owner: this)
                        .DoAction(() => field.SetValue(instance, 
                                Get(field.FieldType, injectionAttribute.Name)), "Set field value.")
                        .OnFailed(report => report
                            .AttachDetails("Class", type)
                            .AttachDetails("Field", field))
                        .FinishAction();
                    ;
                    continue;
                case PropertyInfo property:
                    if (!property.CanWrite)
                    {
                        Report.Warning("Failed to Inject", "Property is read-only.", this)
                            .AttachDetails("Type", type)
                            .AttachDetails("Member", member)
                            .Handle();
                        continue;
                    }
                    
                    ActionReport.BeginAction("Inject Property", owner: this)
                        .DoAction(() => property.SetValue(instance, 
                                Get(property.PropertyType, injectionAttribute.Name)), 
                            "Set property value.")
                        .OnFailed(report => report
                            .AttachDetails("Class", type)
                            .AttachDetails("Property", property))
                        .FinishAction();
                    continue;
                case MethodInfo method:
                    if (!FindArguments(method, out var arguments))
                    {
                        Report.Warning("Failed to Inject", "Can not find all arguments for the method.",
                                this)
                            .AttachDetails("Type", type)
                            .AttachDetails("Member", member)
                            .Handle();
                        continue;
                    }
                    
                    ActionReport.BeginAction("Inject Property", owner: this)
                        .DoAction(() => method.Invoke(instance, arguments), "Invoke method.")
                        .OnFailed(report => report
                            .AttachDetails("Class", type)
                            .AttachDetails("Method", method)
                            .AttachDetails("Arguments", arguments ?? Array.Empty<object?>()))
                        .FinishAction();
                    continue;
            }
        }
    }

    /// <summary>
    /// Find all arguments of a method from this container.
    /// </summary>
    /// <param name="method">Method to prepare arguments for.</param>
    /// <param name="arguments">Found arguments.</param>
    /// <returns>
    /// Returns true if all parameters are satisfied, otherwise false.
    /// </returns>
    private bool FindArguments(MethodBase method, out object?[]? arguments)
    {
        var parameters = method.GetParameters();

        arguments = null;
        
        var foundArguments = new List<object?>();

        foreach (var parameter in parameters)
        {
            var injectionAttribute = parameter.GetCustomAttribute<InjectionAttribute>();
            var argument = Get(parameter.ParameterType, injectionAttribute?.Name);
            if (argument == null &&
                (!parameter.ParameterType.IsGenericType ||
                 parameter.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                return false;
            foundArguments.Add(argument);
        }

        arguments = foundArguments.ToArray();

        return true;
    }
}
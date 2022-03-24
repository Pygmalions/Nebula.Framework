using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Nebula.Injecting.Errors;
using Nebula.Reporting;

namespace Nebula.Injecting;

public class Container
{
    /// <summary>
    /// Map of (Category type -> Name -> Entry).
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, Preset>> _entries = new();

    /// <summary>
    /// Map of (Source -> (Category type, Name) -> Declaration).
    /// </summary>
    private readonly ConcurrentDictionary<Source, ConcurrentDictionary<(Type, string), Declaration>> _sources = new();

    /// <summary>
    /// Get an object from this container.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual object? Get(Type type, string name = "")
    {
        if (!_entries.TryGetValue(type, out var group) ||
            !group.TryGetValue(name, out var preset))
            return null;
        // Prefer to use the bound source to get the object.
        if (preset.BoundDeclaration == null) 
            return preset.BoundBuilder?.Build(this);
        var instance = preset.BoundDeclaration.Source.Get(preset.BoundDeclaration, type, name);
        if (preset.BoundDeclaration.Injectable)
            Inject(instance, preset);
        return instance;
    }

    /// <summary>
    /// Verify whether a source belongs to this container or not.
    /// If the source belongs to this container, returns its declarations set,
    /// otherwise report a warning.
    /// </summary>
    /// <param name="source">Source to verify.</param>
    /// <returns>Declaration set if this source belongs to this container, or null if not.</returns>
    private ConcurrentDictionary<(Type, string), Declaration>? VerifySource(Source source)
    {
        if (!_sources.TryGetValue(source, out var declarations))
            Report.Warning(new InvalidAccessError(this, source, 
                "Source does not belongs to this container."));
        return declarations;
    }

    /// <summary>
    /// Add a source to this container.
    /// </summary>
    /// <param name="source">Source to install and add.</param>
    public void AddSource(Source source)
    {
        if (_sources.ContainsKey(source))
        {
            Report.Warning(new ContainerError(this, "Source has already been added."));
            return;
        }

        _sources.TryAdd(source, new ConcurrentDictionary<(Type, string), Declaration>());
        source.Install(this);
    }

    /// <summary>
    /// Remove a source from this container.
    /// All remaining declarations of this source will be revoked.
    /// </summary>
    /// <param name="source">Source to uninstall and remove.</param>
    public void RemoveSource(Source source)
    {
        if (!_sources.TryRemove(source, out var declarations))
        {
            Report.Warning(new ContainerError(this, "Source does not belong to this container."));
            return;
        }
        
        source.Uninstall();

        foreach (var ((category, name), declaration) in declarations)
        {
            if (!_entries.TryGetValue(category, out var entries))
                continue;
            if (!entries.TryGetValue(name, out var entry))
                continue;
            if (entry.BoundDeclaration == declaration)
                entry.BoundDeclaration = null;
        }
    }

    /// <summary>
    /// Declare an object in the container.
    /// </summary>
    /// <param name="source">Source which wants to declare an object.</param>
    /// <param name="category">Category of the object.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    /// <returns>Declaration instance, or null if this method failed silently.</returns>
    protected internal virtual Declaration? Declare(Source source, Type category, string name = "",
       bool trying = false)
    {
        var declarations = VerifySource(source);
        if (declarations == null)
            return null;

        // Get or create an entry of the specific category and name.
        var group = _entries.GetOrAdd(category, 
            _ => new ConcurrentDictionary<string, Preset>());
        var entry = group.GetOrAdd(name, _ => new Preset(category));
        
        // Set the declaration of this entry.
        if (entry.BoundDeclaration == null)
        {
            var declaration = new Declaration(source, entry);
            entry.BoundDeclaration = declaration;
            declarations.TryAdd((category, name), declaration);
            return declaration;
        }
        
        // Otherwise, this entry has already got a declaration.
        var existingDeclaration = entry.BoundDeclaration;
        if (existingDeclaration.Source == source)
        {
            return existingDeclaration;
        }

        // Reclaim this declaration if it is temporary.
        if (existingDeclaration.Temporary)
        {
            var previousSource = VerifySource(existingDeclaration.Source);
            if (previousSource != null)
                previousSource.TryRemove((category, name), out _);
            existingDeclaration.Source = source;
            declarations.TryAdd((category, name), existingDeclaration);
            return existingDeclaration;
        }

        if (!trying)
            Report.Error(new EntryError(this, category, name, "declare",
                "Entry has already been declared by another source."));
        return null;
    }

    /// <summary>
    /// Revoke an object declaration in the container.
    /// </summary>
    /// <param name="source">Source which wants to revoke a declaration.</param>
    /// <param name="category">Category of the object.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <param name="trying">Whether allow this method to fail silently.</param>
    protected internal virtual void Revoke(Source source, Type category, string name = "", bool trying = false)
    {
        var declarations = VerifySource(source);

        if (declarations == null)
            return;

        if (!_entries.TryGetValue(category, out var group) ||
            !group.TryGetValue(name, out var entry))
        {
            if (trying) return;
            Report.Warning(new EntryError(this, category, name, "revoke" ,
                "Entry does not exit."));
            return;
        }
        
        if (entry.BoundDeclaration?.Source != source)
        {
            Report.Warning(new EntryError(this, category, name, "revoke" ,
                "Entry does not belongs to this source."));
            return;
        }
        
        declarations.TryRemove((category, name), out _);
        entry.BoundDeclaration = null;

        if (entry.IsEmpty())
            group.TryRemove(name, out _);
    }

    /// <summary>
    /// Preset an object in the container.
    /// </summary>
    /// <param name="category">Category of the object.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <returns>Preset instance.</returns>
    public virtual Preset Preset(Type category, string name = "")
    {
        var group = _entries.GetOrAdd(category, 
            _ => new ConcurrentDictionary<string, Preset>());
        return group.GetOrAdd(name, _ => new Preset(category));
    }

    /// <summary>
    /// Use the objects in this container to inject an instance with the given preset.
    /// This method is the inject method used by <see cref="Get"/>.
    /// </summary>
    /// <param name="instance">Instance to inject.</param>
    /// <param name="preset">Preset to use.</param>
    protected internal virtual void Inject(object instance, Preset preset)
    {
        var type = instance.GetType();
        
        foreach (var (name, item) in preset._injectedFields)
        {
            var field = 
                type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                Report.Warning(new InjectionFailure(type, name, 
                    "Can not find a field with the given name."));
                continue;
            }
            if (field.IsInitOnly)
            {
                Report.Warning(new InjectionFailure(type, field, "Field is initalize-only."));
                continue;
            }
            field.SetValue(instance, item.Translate());
        }
        
        foreach (var (name, item) in preset._injectedProperties)
        {
            var property = 
                type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                Report.Warning(new InjectionFailure(type, name, 
                    "Can not find a property with the given name."));
                continue;
            }
            if (!property.CanWrite)
            {
                Report.Warning(new InjectionFailure(type, property, "Property is read-only."));
                continue;
            }
            property.SetValue(instance, item.Translate());
        }
        
        foreach (var (name, items) in preset._injectedMethods)
        {
            var method = 
                type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Report.Warning(new InjectionFailure(type, name, 
                    "Can not find a property with the given name."));
                continue;
            }
            method.Invoke(instance, items?.Translate());
        }
        
        // Inject the passive injections.
        Inject(instance);
    }

    /// <summary>
    /// Use objects in this container to inject an instance without a preset.
    /// This method <b>only</b> supports passive injection on public members.
    /// </summary>
    /// <param name="instance">Instance to inject.</param>
    public void Inject(object instance)
    {
        var type = instance.GetType();
        foreach (var member in type.GetMembers())
        {
            var injectionAttribute = member.GetCustomAttribute<InjectionAttribute>();
            if (injectionAttribute == null)
                continue;
            switch (member)
            {
                case FieldInfo field:
                    if (field.IsInitOnly)
                    {
                        Report.Warning(new InjectionFailure(type, field, "Field is init-only."));
                        continue;
                    }
                    field.SetValue(instance, Get(field.FieldType, injectionAttribute.Name));
                    break;
                case PropertyInfo property:
                    if (!property.CanWrite)
                    {
                        Report.Warning(new InjectionFailure(type, property, "Property is read-only."));
                        continue;
                    }
                    property.SetValue(instance, Get(property.PropertyType, injectionAttribute.Name));
                    break;
                case MethodInfo method:
                    if (!PrepareArguments(method, out var arguments))
                    {
                        Report.Warning(new InjectionFailure(type, method, 
                            "Failed to find all parameters."));
                        continue;
                    }
                    
                    method.Invoke(instance, arguments.ToArray());
                    break;
            }
        }
    }

    /// <summary>
    /// Use objects in this container to inject an instance without a preset.
    /// This method <b>only</b> supports constructor injection and passive injection on public members.
    /// </summary>
    /// <param name="type">Type of the object to instantiate and inject.</param>
    public object? Inject(Type type)
    {
        ConstructorInfo? foundConstructor = null;
        object?[]? foundArguments = null;
        
        // Constructor inject.
        foreach (var constructor in type.GetConstructors())
        {
            if (!PrepareArguments(constructor, out var arguments))
                continue;
            foundConstructor = constructor;
            foundArguments = arguments;
        }

        if (foundConstructor == null)
            return null;

        var instance = foundConstructor.Invoke(foundArguments);

        // Passive inject.
        Inject(instance);

        return instance;
    }

    /// <summary>
    /// Try to find objects required by the specific method.
    /// </summary>
    /// <param name="method">Method to find arguments.</param>
    /// <param name="arguments">Found arguments that meet the requirement of the given method.</param>
    /// <returns>
    /// True if all arguments meet the needs of the method are found, otherwise false.
    /// </returns>
    private bool PrepareArguments(MethodBase method, [NotNullWhen(true)] out object?[]? arguments)
    {
        arguments = null;
        var parameters = method.GetParameters();
        if (parameters.Length == 0) 
            return false;
        var argumentsList = new List<object?>();
        foreach (var parameter in parameters)
        {
            var parameterAttribute = parameter.GetCustomAttribute<InjectionAttribute>();
            var parameterNullable = parameter.ParameterType.IsGenericType &&
                                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);
            var argument = Get(parameter.ParameterType, parameterAttribute?.Name ?? "");
            if (argument == null && !parameterNullable)
                return false;
            argumentsList.Add(argument);
        }

        arguments = argumentsList.ToArray();
        return true;
    }
}
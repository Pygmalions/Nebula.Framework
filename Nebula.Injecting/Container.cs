using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
    /// If the entry is injectable, this method will do preset injection and passive injection on the given object.
    /// </summary>
    /// <param name="type">Type of the object.</param>
    /// <param name="name">Name of the object. Empty means that object with any name can match.</param>
    /// <returns>Object matching the requirement, or null if not found.</returns>
    public virtual object? Get(Type type, string name = "")
    {
        if (!_entries.TryGetValue(type, out var group))
            return null;
        
        // Name matching.
        if (!group.TryGetValue(name, out var preset))
        {
            // Try to mach with "*"
            if (!group.TryGetValue("*", out preset))
                // If name is empty, match any entry.
                if (name.Length == 0)
                    return group.Values.Count > 0 ? group.Values.First() : null;
        }

        if (preset == null)
            return null;
        
        // Prefer to use the bound source to get the object.
        if (preset.BoundDeclaration == null) 
            // The build method will do the injection.
            return preset.BoundBuilder?.Build(this);

        // Check cached singleton instance.
        if (preset.BoundDeclaration.SingletonInstance != null)
            return preset.BoundDeclaration.SingletonInstance;
        var instance = preset.BoundDeclaration.Source.Get(preset.BoundDeclaration, type, name);
        if (instance == null) 
            return null;
        
        // Preset injection.
        preset.Inject(instance);
        // Passive injection.
        Inject(instance);

        // Cache the singleton instance.
        if (!preset.BoundDeclaration.Singleton)
            preset.BoundDeclaration.SingletonInstance = instance;
        
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
            Report.Warning("Container Access Denied",
                    "Source does not belongs to this container.", owner: this)
                .AttachDetails("Source", source)
                .Handle();
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
            Report.Warning("Failed to Add Source", "Source has already been added.",this)
                .AttachDetails("Source", source)
                .Handle();
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
            Report.Warning("Failed to Remove Source",
                    "Source does not belong to this container.", owner: this)
                .AttachDetails("Source", source)
                .Handle();
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
    /// <param name="name">
    /// Optional name of the object.
    /// Being empty means that this object is default for non-named requirements;
    /// being "*" means that the source will generate instance for requirements which can not match any entry.
    /// </param>
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
            Report.Warning("Failed to Revoke", "Entry has been declared by other source.", 
                    this)
                .AttachDetails("Source", source)
                .AttachDetails("Declarer", existingDeclaration.Source)
                .AttachDetails("Category", category)
                .AttachDetails("Name", name)
                .Handle();
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
            !group.TryGetValue(name, out var entry) || entry.BoundDeclaration == null)
        {
            if (trying) return;
            Report.Warning("Failed to Revoke", "Declaration does not exist.", 
                    this)
                .AttachDetails("Source", source)
                .AttachDetails("Category", category)
                .AttachDetails("Name", name)
                .Handle();
            return;
        }
        
        if (entry.BoundDeclaration.Source != source)
        {
            Report.Warning("Failed to Revoke", "Declaration is not declared by this source.", 
                    this)
                .AttachDetails("Source", source)
                .AttachDetails("Declarer", entry.BoundDeclaration.Source)
                .AttachDetails("Category", category)
                .AttachDetails("Name", name)
                .Handle();
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
        return group.GetOrAdd(name, _ => new Preset(category, this));
    }

    /// <summary>
    /// Use objects in this container to inject an instance without a preset.
    /// This method <b>only</b> supports <b>passive injection</b> on public members.
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
            var injectionName = injectionAttribute.Name ?? "";
            switch (member)
            {
                case FieldInfo field:
                    if (field.IsInitOnly)
                    {
                        Report.Warning("Field is init-only.").InDebug?.Throw();
                        continue;
                    }
                    field.SetValue(instance, Get(field.FieldType, injectionName));
                    break;
                case PropertyInfo property:
                    if (!property.CanWrite)
                    {
                        Report.Warning("Injection Failure",
                                "Property is read-only", this)
                            .AttachDetails("Class", type)
                            .AttachDetails("Member", property)
                            .Handle();
                        continue;
                    }
                    property.SetValue(instance, Get(property.PropertyType, injectionName));
                    break;
                case MethodInfo method:
                    if (!PrepareArguments(method, out var arguments))
                    {
                        Report.Warning("Injection Failure",
                                "Failed to find all parameters.", this)
                            .AttachDetails("Class", type)
                            .AttachDetails("Member", method)
                            .Handle();
                        continue;
                    }
                    
                    method.Invoke(instance, arguments.ToArray());
                    break;
            }
        }
    }

    /// <summary>
    /// Use objects in this container to inject an instance without a preset.
    /// This method <b>only</b> supports <b>constructor injection</b> and <b>passive injection</b> on public members.
    /// </summary>
    /// <param name="type">Type of the object to instantiate and inject.</param>
    public object? Inject(Type type)
    {
        ConstructorInfo? defaultConstructor = null;
        (ConstructorInfo Constructor, object?[]? Arguments)? attributedConstructor = null;

        // Constructor inject.
        foreach (var constructor in type.GetConstructors())
        {
            if (constructor.GetParameters().Length == 0)
            {
                defaultConstructor = constructor;
                continue;
            }

            if (constructor.GetCustomAttribute<InjectionAttribute>() == null ||
                !PrepareArguments(constructor, out var arguments)) 
                continue;
            attributedConstructor = (constructor, arguments);
        }
        var foundConstructor = attributedConstructor?.Constructor ?? defaultConstructor;
        var foundArguments = attributedConstructor?.Arguments;
        
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
            var injectionName = parameterAttribute?.Name ?? "";
            var argument = Get(parameter.ParameterType, injectionName);
            if (argument == null && !parameterNullable)
                return false;
            argumentsList.Add(argument);
        }

        arguments = argumentsList.ToArray();
        return true;
    }
}
using System.Reflection;
using Nebula.Reporting;

namespace Nebula.Injecting;

public class Preset
{
    internal Builder? BoundBuilder = null;

    internal Declaration? BoundDeclaration = null;

    /// <summary>
    /// Field names and the corresponding values to inject.
    /// </summary>
    private readonly Dictionary<string, Func<object>> _injectedFields = new();
    
    /// <summary>
    /// Property names and the corresponding values to inject.
    /// </summary>
    private readonly Dictionary<string, Func<object>> _injectedProperties = new();
    
    /// <summary>
    /// Method names and the corresponding arguments to invoke with.
    /// </summary>
    private readonly List<(string, Func<object?[]>?)> _injectedMethods = new();

    /// <summary>
    /// Preset the value of a field.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="value">Value to preset.</param>
    /// <returns>This entry.</returns>
    public Preset PresetField(string name, Func<object> value)
    {
        _injectedFields[name] = value;
        return this;
    }
    
    /// <summary>
    /// Preset the value of a property.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="value">Value to preset.</param>
    /// <returns>This entry.</returns>
    public Preset PresetProperty(string name, Func<object> value)
    {
        _injectedProperties[name] = value;
        return this;
    }
    
    /// <summary>
    /// Invoke a method after on the instance when it is injected.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    /// <param name="arguments">Arguments to pass the method.</param>
    /// <returns>This entry.</returns>
    public Preset InvokeMethod(string name, Func<object?[]>? arguments)
    {
        _injectedMethods.Add((name, arguments));
        return this;
    }

    /// <summary>
    /// Check whether this preset is empty.
    /// An empty preset can be removed safely.
    /// </summary>
    /// <returns>True if this preset is empty, otherwise false.</returns>
    internal bool IsEmpty()
    {
        return BoundBuilder == null && BoundDeclaration == null &&
               _injectedFields.Count == 0 && _injectedProperties.Count == 0 && _injectedMethods.Count == 0;
    }
    
    private readonly Type _category;

    private readonly Container? _container;

    public Preset(Type category, Container? container = null)
    {
        _category = category;
        _container = container;
    }
    
    /// <summary>
    /// Return the builder instance of this preset.
    /// </summary>
    /// <returns>Builder instance, or null if it does not have one.</returns>
    public Builder? AsBuilder()
    {
        return BoundBuilder;
    }

    /// <summary>
    /// Add a builder to this preset if it does not have one.
    /// </summary>
    /// <param name="boundType">
    /// Type for the builder to use,
    /// if null, the builder will use the category to build.
    /// </param>
    /// <returns>This preset.</returns>
    public Preset SetBuilder(Type? boundType = null)
    {
        BoundBuilder ??= new Builder(boundType ?? _category, this, _container);
        return this;
    }

    /// <summary>
    /// Remove the builder from this preset if it has one.
    /// </summary>
    /// <returns>This preset.</returns>
    public Preset UnsetBuilder()
    {
        BoundBuilder = null;
        return this;
    }

    /// <summary>
    /// Inject an instance with the given preset. This method does not need a container.
    /// This method is the inject method used by <see cref="Container.Get"/>.
    /// </summary>
    /// <param name="instance">Instance to inject.</param>
    public void Inject(object instance)
    {
        var type = instance.GetType();
        
        foreach (var (name, item) in _injectedFields)
        {
            var field = 
                type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                Report.Warning("Injection Failure", "Can not find a field with the given name.")
                    .AttachDetails("Class", type)
                    .AttachDetails("Member", name)
                    .Handle();
                continue;
            }
            if (field.IsInitOnly)
            {
                Report.Warning("Injection Failure", "Field is init-only.")
                    .AttachDetails("Class", type)
                    .AttachDetails("Member", name)
                    .Handle();
                continue;
            }
            field.SetValue(instance, item.Invoke());
        }
        
        foreach (var (name, item) in _injectedProperties)
        {
            var property = 
                type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
            {
                Report.Warning("Injection Failure",
                        "Can not find a property with the given name.")
                    .AttachDetails("Class", type)
                    .AttachDetails("Member", name)
                    .Handle();
                continue;
            }
            if (!property.CanWrite)
            {
                Report.Warning("Injection Failure",
                        "Property is read-only.")
                    .AttachDetails("Class", type)
                    .AttachDetails("Member", name)
                    .Handle();
                continue;
            }
            property.SetValue(instance, item.Invoke());
        }
        
        foreach (var (name, items) in _injectedMethods)
        {
            var method = 
                type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                Report.Warning("Injection Failure",
                        "Can not find a method with the given name.")
                    .AttachDetails("Class", type)
                    .AttachDetails("Member", name)
                    .Handle();
                continue;
            }
            method.Invoke(instance, items?.Invoke());
        }
    }
}
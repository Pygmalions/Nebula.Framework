using Nebula.Injecting.Presetting;

namespace Nebula.Injecting;

public class Preset
{
    internal Builder? BoundBuilder = null;

    internal Declaration? BoundDeclaration = null;

    /// <summary>
    /// Lock to protect the injection information.
    /// </summary>
    internal readonly object InjectionLock = new();
    
    /// <summary>
    /// Field names and the corresponding values to inject.
    /// </summary>
    internal readonly Dictionary<string, IItem<object>> _injectedFields = new();
    
    /// <summary>
    /// Property names and the corresponding values to inject.
    /// </summary>
    internal readonly Dictionary<string, IItem<object>> _injectedProperties = new();
    
    /// <summary>
    /// Method names and the corresponding arguments to invoke with.
    /// </summary>
    internal readonly List<(string, IArray<object?>?)> _injectedMethods = new();

    /// <summary>
    /// Preset the value of a field.
    /// </summary>
    /// <param name="name">Name of the field.</param>
    /// <param name="value">Value to preset.</param>
    /// <returns>This entry.</returns>
    public Preset PresetField(string name, IItem<object> value)
    {
        lock (InjectionLock)
        {
            _injectedFields[name] = value;
        }
        return this;
    }
    
    /// <summary>
    /// Preset the value of a property.
    /// </summary>
    /// <param name="name">Name of the property.</param>
    /// <param name="value">Value to preset.</param>
    /// <returns>This entry.</returns>
    public Preset PresetProperty(string name, IItem<object> value)
    {
        lock (InjectionLock)
        {
            _injectedProperties[name] = value;
        }
        return this;
    }
    
    /// <summary>
    /// Invoke a method after on the instance when it is injected.
    /// </summary>
    /// <param name="name">Name of the method.</param>
    /// <param name="arguments">Arguments to pass the method.</param>
    /// <returns>This entry.</returns>
    public Preset InvokeMethod(string name, IArray<object>? arguments)
    {
        lock (InjectionLock)
        {
            _injectedMethods.Add((name, arguments));
        }
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

    public Preset(Type category)
    {
        _category = category;
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
    /// <returns>This preset.</returns>
    public Preset SetBuilder(Type? boundType)
    {
        BoundBuilder ??= new Builder(boundType ?? _category, this);
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
}
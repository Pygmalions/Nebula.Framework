using Nebula.Injecting.Presetting;
using Nebula.Reporting;

namespace Nebula.Injecting;

/// <summary>
/// A preset is the injection information set manually.
/// </summary>
public sealed class Builder : Entry
{
    /// <summary>
    /// The instance of singleton mode.
    /// </summary>
    private object? _singletonInstance;

    /// <summary>
    /// Whether the singleton mode is enabled or not.
    /// </summary>
    private bool _singletonEnable;

    /// <summary>
    /// Set the instance of this preset.
    /// If the instance is set, the container will not generate new instance
    /// but use this instance directly.
    /// This method will enable the singleton mode.
    /// </summary>
    /// <param name="instance">Instance to set.</param>
    public Builder BindInstance(object instance)
    {
        _singletonEnable = true;
        _singletonInstance = instance;
        return this;
    }
    
    /// <summary>
    /// Type to generate instance.
    /// </summary>
    public Type Class { get; private set; }
    
    /// <summary>
    /// Arguments to pass to the constructor.
    /// </summary>
    public IArray<object?>? Arguments { get; private set; }

    /// <summary>
    /// Set the class to generate instance with.
    /// </summary>
    /// <param name="type">Type to use.</param>
    /// <returns>This preset.</returns>
    public Builder BindClass(Type type)
    {
        Class = type;
        return this;
    }

    /// <summary>
    /// Bind the arguments to pass to the constructor.
    /// </summary>
    /// <param name="arguments">Arguments to use to generate instance.</param>
    /// <returns>This preset.</returns>
    public Builder BindArguments(IArray<object?>? arguments)
    {
        Arguments = arguments;
        return this;
    }

    /// <summary>
    /// Set the enable of singleton mode.
    /// If the singleton mode is enabled, then the <see cref="Build"/> method will try to return
    /// the cached singleton instance if it is not null, and the <see cref="Build"/> method will store the instance
    /// as the cached singleton instance.
    /// If the singleton mode is required to disable, then the existing singleton instance will be removed.
    /// </summary>
    /// <param name="enable">Whether enable the singleton mode or not.</param>
    /// <returns>This preset.</returns>
    public Builder SetSingletonMode(bool enable)
    {
        _singletonEnable = enable;
        if (!enable)
            _singletonInstance = null;
        return this;
    }

    /// <summary>
    /// Constructor for a preset to create a builder insides it.
    /// </summary>
    /// <param name="type">Bound type.</param>
    /// <param name="preset">Preset to bind.</param>
    internal Builder(Type type, Preset preset) : base(preset)
    {
        Class = type;
    }
    
    /// <summary>
    /// Constructor for user to create a builder outsides the container.
    /// This constructor will construct a preset instance.
    /// </summary>
    /// <param name="type">Bound type.</param>
    public Builder(Type type) : base(new Preset(type))
    {
        Injection.BoundBuilder = this;
        Class = type;
    }

    /// <summary>
    /// Build a new instance if the singleton mode is not enabled or the singleton instance is null.
    /// If the singleton mode is enabled, then this method will build a new instance and store it as the singleton
    /// instance.
    /// <para>
    ///     This method will do the injection with the given container if it is not null.
    /// </para>
    /// </summary>
    /// <param name="container">Container to inject with, if null, the injection will be ignored.</param>
    /// <returns>Instance, or null if failed.</returns>
    public object? Build(Container? container)
    {
        if (_singletonInstance != null)
            return _singletonInstance;

        if (Class.IsAbstract || Class.IsInterface)
        {
            Report.Warning(
                new Exception($"Failed to build: Builder is bound to an abstract class or interface. {Class}"));
            return null;
        }

        var instance = Activator.CreateInstance(Class, Arguments?.Translate());

        if (instance == null)
            return null;
        
        container?.Inject(instance, Injection);

        if (_singletonEnable)
            _singletonInstance = instance;
        
        return instance;
    }
}
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
    /// Creator used to acquire a new instance.
    /// If the singleton mode is enabled, then its result will be stored and used in following accesses.
    /// </summary>
    private Func<object?>? _creator;

    /// <summary>
    /// Set the instance of this preset.
    /// If the instance is set, the container will not generate new instance
    /// but use this instance directly.
    /// This method will <b>enable</b> the singleton mode. <br />
    /// The bound instance will <b>not</b> be injected.
    /// </summary>
    /// <param name="instance">Instance to set.</param>
    public Builder BindInstance(object instance)
    {
        _singletonEnable = true;
        _singletonInstance = instance;

        return this;
    }
    
    /// <summary>
    /// Bind a creator to this builder.
    /// This method will NOT auto enable the singleton mode.
    /// The instance created by this creator will NOT be auto injected in <see cref="Build"/> method.
    /// </summary>
    /// <param name="creator">Creator to acquire new instance.</param>
    public Builder BindCreator(Func<object> creator)
    {
        _creator = creator;
        return this;
    }
    /// <summary>
    /// Remove the creator from this builder.
    /// </summary>
    public Builder UnbindCreator()
    {
        _creator = null;
        return this;
    }

    /// <summary>
    /// Type to generate instance.
    /// </summary>
    public Type Class { get; private set; }
    
    /// <summary>
    /// Arguments to pass to the constructor.
    /// </summary>
    public Func<object?[]>? Arguments { get; private set; }

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
    /// <para>
    /// Use <see cref="BindInstance"/> if the object to build is a language built-in value type which
    /// does not have a constructor, such as int, long, double, etc.
    /// </para>
    /// </summary>
    /// <param name="arguments">Arguments to use to generate instance.</param>
    /// <returns>This preset.</returns>
    public Builder BindArguments(Func<object?[]>? arguments)
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
    /// Default container for <see cref="Build"/> to use when no container is given to it.
    /// </summary>
    private readonly Container? _defaultContainer;

    /// <summary>
    /// Constructor for a preset to create a builder insides it.
    /// </summary>
    /// <param name="type">Bound type.</param>
    /// <param name="preset">Preset to bind.</param>
    /// <param name="container">Default container to bind.</param>
    internal Builder(Type type, Preset preset, Container? container = null) : base(preset)
    {
        Class = type;
        _defaultContainer = container;
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
    /// <param name="container">Container to inject with, if null, the passive injection will be ignored.</param>
    /// <returns>Instance, or null if failed.</returns>
    public object? Build(Container? container = null)
    {
        if (_singletonInstance != null)
            return _singletonInstance;

        var instance = _creator?.Invoke();
        if (instance != null)
        {
            if (_singletonEnable)
                _singletonInstance = instance;
            return instance;
        }
        
        if (instance == null)
        {
            if (Class.IsAbstract || Class.IsInterface)
            {
                Report.Warning("Failed to Build", "Builder is bound to an abstract class or an interface.",
                        this)
                    .AttachDetails("Builder", this)
                    .AttachDetails("Class", Class)
                    .Handle();
                return null;
            }

            container ??= _defaultContainer;

            if (container != null && Arguments == null)
            {
                instance = container.Inject(Class);
            }
            else
                instance = Activator.CreateInstance(Class, Arguments?.Invoke());
            
        }
        
        if (instance == null)
            return null;
        
        // Preset injection.
        Injection.Inject(instance);
        // Passive injection.
        container?.Inject(instance);

        if (_singletonEnable)
            _singletonInstance = instance;
        
        return instance;
    }
}
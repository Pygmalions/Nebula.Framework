using System.Reflection;
using Nebula.Reporting;

namespace Nebula.Injecting;

/// <summary>
/// A definition describes how to acquire and inject an object.
/// </summary>
public class Definition
{
    /// <summary>
    /// Cached singleton instance.
    /// </summary>
    private object? _singletonInstance;
    
    /// <summary>
    /// If not null, then this definition will use this delegate to acquire the instance.
    /// </summary>
    public Builder.Item? BoundBuilder { get; private set; }
    
    /// <summary>
    /// Class for the container or activator to instantiate the instance.
    /// </summary>
    public Type? BoundClass { get; private set; }
    
    /// <summary>
    /// Arguments for the container or activator to instantiate the instance.
    /// </summary>
    public Func<object?[]>? BoundArguments { get; private set; }
    
    /// <summary>
    /// Whether this definition works in the singleton model or not. <br />
    /// If true, this definition will cache the result after it firstly acquires and injects the instance.
    /// </summary>
    public bool Singleton { get; private set; }

    /// <summary>
    /// Injector to preset the instance.
    /// </summary>
    public Preset Injector { get; } = new();

    /// <summary>
    /// Set the enable of the singleton mode.
    /// </summary>
    /// <param name="enable">Enable of the singleton mode.</param>
    /// <returns>This definition.</returns>
    public Definition SetSingleton(bool enable)
    {
        if (!enable)
            _singletonInstance = null;

        Singleton = enable;
        return this;
    }

    /// <summary>
    /// Get the preset of this definition.
    /// </summary>
    /// <returns>Preset.</returns>
    public Preset AsPreset() => Injector;

    /// <summary>
    /// Bind a class for a container or the activator to use to construct an instance.
    /// </summary>
    /// <param name="type">Type to bind.</param>
    /// <returns>This definition.</returns>
    public Definition BindClass(Type type)
    {
        BoundClass = type;
        return this;
    }

    /// <summary>
    /// Bind arguments for a container or the activator to use to construct an instance.
    /// </summary>
    /// <param name="arguments">Arguments to bind.</param>
    /// <returns>This definition.</returns>
    public Definition BindArguments(Func<object?[]> arguments)
    {
        BoundArguments = arguments;
        return this;
    }

    /// <summary>
    /// Bind the builder delegate to acquire an instance.
    /// If the builder is bound, then the bound class and arguments will be ignored.
    /// </summary>
    /// <param name="builder">Builder delegate to bind.</param>
    /// <returns>This definition.</returns>
    public Definition BindBuilder(Builder.Item builder)
    {
        BoundBuilder = builder;
        return this;
    }

    /// <summary>
    /// Get an object according to this definition.
    /// </summary>
    /// <param name="container">
    /// Container to use if the builder is not bound. If it is null and the builder is not bound,
    /// then this preset will try to use the activator.
    /// </param>
    /// <param name="member">Optional member information of the member to inject.</param>
    /// <param name="holder">Optional holder to inject.</param>
    /// <returns>Object constructed by this definition.</returns>
    public object Get(Container? container = null, MemberInfo? member = null, object? holder = null)
    {
        if (_singletonInstance != null)
            return _singletonInstance;

        // Acquire instance.
        object? instance = null;
        if (BoundBuilder != null)
            instance = BoundBuilder(member, holder);
        else
        {
            if (BoundClass == null)
                throw Report.Error("Failed to Construct Object",
                        "Definition has no bound builder nor class.", this)
                    .NotifyAsException();
            var arguments = BoundArguments?.Invoke();
            Action action = container != null && arguments == null
                ? () => instance = container.Construct(BoundClass)
                : () => instance = Activator.CreateInstance(BoundClass, arguments);
            
            ActionReport.BeginAction("Construct Object", this)
                .DoAction(action, "Construct an object according to the definition using the activator.")
                .OnFailed(report =>
                {
                    report.AttachDetails("Class", BoundClass);
                    if (arguments != null)
                        report.AttachDetails("Arguments", arguments);
                }).FinishAction();
        }

        if (instance == null)
            throw Report.Error("Construct Object", "Failed to construct an object for a definition.",
                this).NotifyAsException();

        // Do the preset injection.
        instance = Injector.Inject(instance);

        // Do the container injection.
        container?.Inject(instance);

        if (Singleton)
            _singletonInstance = instance;

        return instance;
    }
}
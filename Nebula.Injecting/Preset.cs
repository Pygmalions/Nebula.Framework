using Nebula.Reporting;

namespace Nebula.Injecting;

public class Preset
{
    private readonly Dictionary<string, Builder.Item> _fields = new();

    private readonly Dictionary<string, Builder.Item> _properties = new();

    private readonly List<(string Method, Type[]? signature, Builder.Array Arguments)> _invocations = new();

    private readonly List<Func<object, object>> _preprocess = new();

    private readonly List<Func<object, object>> _postprocess = new();
    
    /// <summary>
    /// Preset the value of a field.
    /// </summary>
    /// <param name="field">Field to preset.</param>
    /// <param name="value">Value delegate.</param>
    /// <returns>This preset.</returns>
    public Preset SetField(string field, Builder.Item value)
    {
        _fields[field] = value;
        return this;
    }

    /// <summary>
    /// Preset the value of a property.
    /// </summary>
    /// <param name="property">Property to preset.</param>
    /// <param name="value">Value delegate.</param>
    /// <returns>This preset.</returns>
    public Preset SetProperty(string property, Builder.Item value)
    {
        _properties[property] = value;
        return this;
    }

    /// <summary>
    /// Invoke a method.
    /// </summary>
    /// <param name="method">Method to invoke.</param>
    /// <param name="arguments">Arguments delegate.</param>
    /// <param name="signature">
    /// Optional signature. If the method to invoke has many overloads,
    /// then its signature must be given to find the method to invoke.
    /// </param>
    /// <returns>This preset.</returns>
    public Preset InvokeMethod(string method, Builder.Array arguments, Type[]? signature = null)
    {
        _invocations.Add((method, signature, arguments));
        return this;
    }

    /// <summary>
    /// Add a preprocess. The preprocess will be invoked before the instance begin to be injected.
    /// </summary>
    /// <param name="preprocessor">
    /// A delegate, has the instance to inject as its parameter,
    /// and returns the instance to substitute.</param>
    /// <returns>This preset.</returns>
    public Preset Preprocess(Func<object, object> preprocessor)
    {
        _preprocess.Add(preprocessor);
        return this;
    }

    /// <summary>
    /// Add a preprocess. The postprocessor will be invoked after the instance has been injected.
    /// </summary>
    /// <param name="postprocessor">
    /// A delegate, has the instance to inject as its parameter,
    /// and returns the instance to substitute.</param>
    /// <returns>This preset.</returns>
    public Preset Postprocess(Func<object, object> postprocessor)
    {
        _postprocess.Add(postprocessor);
        return this;
    }
    
    /// <summary>
    /// Inject an object according to this preset.
    /// </summary>
    /// <param name="instance">Instance to inject.</param>
    /// <returns>
    /// Usually the instance passed in, sometimes the instance replaced by the preprocessor or postprocessor.
    /// </returns>
    public object Inject(object instance)
    {
        instance = _preprocess.Aggregate(instance, 
            (current, preprocessor) => preprocessor(current));

        var type = instance.GetType();
        
        // Inject fields.
        foreach (var (name, value) in _fields)
        {
            var field = type.GetField(name);
            if (field == null)
            {
                Report.Warning("Failed to Inject", "Can not find the field.", this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Name", name)
                    .Handle();
                continue;
            }
            if (field.IsInitOnly)
            {
                Report.Warning("Failed to Inject", "Field is initialize-only.", this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Name", name)
                    .Handle();
                continue;
            }
            field.SetValue(instance, value(field, instance));
        }
        
        // Inject properties.
        foreach (var (name, value) in _properties)
        {
            var property = type.GetProperty(name);
            if (property == null)
            {
                Report.Warning("Failed to Inject", "Can not find the property.", this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Name", name)
                    .Handle();
                continue;
            }
            if (!property.CanWrite)
            {
                Report.Warning("Failed to Inject", "Can not write the property.", this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Name", name)
                    .Handle();
                continue;
            }
            property.SetValue(instance, value(property, instance));
        }
        
        // Inject methods.
        foreach (var (name, signature, arguments) in _invocations)
        {
            var method = signature == null ? 
                type.GetMethod(name) : type.GetMethod(name, signature);
            if (method == null)
            {
                var report = Report.Warning("Failed to Inject", "Can find the matching method.",
                        this)
                    .AttachDetails("Type", type)
                    .AttachDetails("Name", name);
                if (signature != null)
                    report.AttachDetails("Signature", signature);
                report.Handle();
                continue;
            }

            method.Invoke(instance, arguments(method, instance));
        }
        
        instance = _postprocess.Aggregate(instance, 
            (current, postprocessor) => postprocessor(current));

        return instance;
    }
}
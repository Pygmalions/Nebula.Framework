using System.Reflection;

namespace Nebula.Proxying;

public class DecoratedProperty : IExtensibleProperty
{
    /// <inheritdoc />
    public event Action<AccessContext> Getting
    {
        add => _gettingHandlers.Add(value);
        remove => _gettingHandlers.Remove(value);
    }
    /// <inheritdoc />
    public event Action<AccessContext> AfterGetting
    {
        add => _afterGettingHandlers.Add(value);
        remove => _afterGettingHandlers.Remove(value);
    }
    
    /// <inheritdoc />
    public event Action<AccessContext> Setting
    {
        add => _settingHandlers.Add(value);
        remove => _settingHandlers.Remove(value);
    }
    /// <inheritdoc />
    public event Action<AccessContext> AfterSetting
    {
        add => _afterSettingHandlers.Add(value);
        remove => _afterSettingHandlers.Remove(value);
    }
    
    /// <inheritdoc />
    public object ProxiedHolder { get; }
    
    /// <inheritdoc />
    public PropertyInfo ProxiedProperty { get; }
    
    /// <inheritdoc />
    public bool NullValueAccepted { get; }

    /// <summary>
    /// Set of registered handlers for pre-process event.
    /// </summary>
    private readonly HashSet<Action<AccessContext>> _gettingHandlers = new();
    /// <summary>
    /// Set of registered handlers for post-process event.
    /// </summary>
    private readonly HashSet<Action<AccessContext>> _afterGettingHandlers = new();
    /// <summary>
    /// Set of registered handlers for pre-process event.
    /// </summary>
    private readonly HashSet<Action<AccessContext>> _settingHandlers = new();
    /// <summary>
    /// Set of registered handlers for post-process event.
    /// </summary>
    private readonly HashSet<Action<AccessContext>> _afterSettingHandlers = new();

    public DecoratedProperty(object holder, PropertyInfo property)
    {
        ProxiedHolder = holder;
        ProxiedProperty = property;
        
        NullValueAccepted = 
            ProxiedProperty.PropertyType.IsGenericType &&
            ProxiedProperty.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
    
    public object? Get()
    {
        AccessContext context = new(this, null);

        Parallel.ForEach(_gettingHandlers, action => action.Invoke(context));
        
        if (context.ThrowingException != null)
            throw context.ThrowingException;
        
        if (context.Interrupted)
        {
            if (context.AccessingValue == null && !NullValueAccepted)
                throw new Exception("Proxied property getting interrupted without acceptable value.");
            return context.AccessingValue;
        }
        if (!context.Skipped)
            context.AccessingValue = ProxiedProperty.GetValue(ProxiedHolder);
        else if (context.AccessingValue == null && !NullValueAccepted)
            throw new Exception("Proxied property getting skipped without acceptable value.");
        
        Parallel.ForEach(_afterGettingHandlers, action => action.Invoke(context));

        if (context.ThrowingException != null)
            throw context.ThrowingException;
        
        return context.AccessingValue;
    }

    public void Set(object? value)
    {
        if (!ProxiedProperty.CanWrite)
            throw new Exception("Failed to set the value of the proxied property: it is not writable.");
        
        AccessContext context = new(this, value);

        Parallel.ForEach(_settingHandlers, action => action.Invoke(context));
        
        if (context.ThrowingException != null)
            throw context.ThrowingException;

        if (context.Interrupted)
            return;
        if (!context.Skipped)
            ProxiedProperty.SetValue(ProxiedHolder, context.AccessingValue);
        
        Parallel.ForEach(_afterSettingHandlers, action => action.Invoke(context));
        
        if (context.ThrowingException != null)
            throw context.ThrowingException;
    }
}
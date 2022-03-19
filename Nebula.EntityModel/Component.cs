using System;
using System.Collections.Generic;

namespace Nebula.EntityModel;

public class Component
{
    private bool _enable;

    /// <summary>
    /// Construct this component with the given GUID.
    /// </summary>
    /// <param name="id"></param>
    protected Component(Guid id)
    {
        Id = id;
        var types = new List<Type>();
        for (var representType = GetType();
             representType != typeof(Component) && representType != null;
             representType = representType.BaseType)
            types.Add(representType);
        Categories = types;
    }

    /// <summary>
    /// Construct this component with a new GUID.
    /// </summary>
    protected Component() : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Global unique ID to identify a component in a domain.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Categories of this component when it is attached to an entity.
    /// This component can be found according to any one of these categories.
    /// </summary>
    public IEnumerable<Type> Categories { get; }

    /// <summary>
    /// The entity to which this component is attached.
    /// </summary>
    public Entity? Entity { get; private set; }

    /// <summary>
    /// Whether this component is enabled.
    /// </summary>
    public bool Enable
    {
        get => _enable;
        set
        {
            if (_enable == value || Entity == null || value && !Entity.Enable)
                return;
            if (value)
                OnEnable();
            else OnDisable();
            _enable = value;
        }
    }

    /// <summary>
    /// Attach this component to an entity.
    /// </summary>
    /// <param name="entity">Entity to attach to.</param>
    /// <exception cref="Exception">Thrown if this component has already attached to an entity.</exception>
    internal void Attach(Entity entity)
    {
        if (Entity != null)
            throw new Exception("The component has already been attached to an entity.");
        Entity = entity;
        OnAttach();
    }

    /// <summary>
    /// Detach from the parent entity.
    /// </summary>
    internal void Detach()
    {
        if (Entity == null)
            return;
        OnDetach();
        Entity = null;
    }

    /// <summary>
    /// Invoke when this component is attached to the host entity.
    /// </summary>
    protected virtual void OnAttach()
    {
    }

    /// <summary>
    /// Invoke when this component is detaching from the host entity.
    /// </summary>
    protected virtual void OnDetach()
    {
    }

    /// <summary>
    /// Invoke when this component is required to enable.
    /// </summary>
    protected virtual void OnEnable()
    {
    }

    /// <summary>
    /// Invoke when this component is required to disable.
    /// </summary>
    protected virtual void OnDisable()
    {
    }
}
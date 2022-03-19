using System.Collections.Concurrent;

namespace Nebula.EntityModel;

public class Entity
{
    /// <summary>
    /// Categorized table for components based on their categories.
    /// </summary>
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, Component>> _categories = new();

    /// <summary>
    /// Lookup table for all components of this entity.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Component> _components = new();

    private bool _enable;

    /// <summary>
    /// Construct the entity with the default Program domain location.
    /// </summary>
    public Entity()
    {
        
    }

    /// <summary>
    /// Whether this entity is enable.
    /// </summary>
    [Proxying.Proxy]
    public bool Enable
    {
        get => _enable;
        set
        {
            if (value == _enable)
                return;
            if (value)
                OnEnable();
            else OnDisable();
            foreach (var component in _components.Values) component.Enable = value;
            _enable = value;
        }
    }

    /// <summary>
    /// Add a component to this entity.
    /// </summary>
    /// <param name="component">Component to attach to this entity.</param>
    public void AddComponent(Component component)
    {
        if (_components.ContainsKey(component.Id))
            return;
        _components.TryAdd((Guid)component.Id, component);
        foreach (var componentType in component.Categories)
        {
            var category = _categories.GetOrAdd(
                componentType, _ => new ConcurrentDictionary<Guid, Component>());
            category[component.Id] = component;
        }

        component.Attach(this);
        component.Enable = Enable;
    }

    /// <summary>
    /// Remove a component from this component.
    /// </summary>
    /// <param name="component">Component to detach from this component.</param>
    public void RemoveComponent(Component component)
    {
        if (!_components.ContainsKey(component.Id))
            return;

        foreach (var componentType in component.Categories)
            if (_categories.TryGetValue(componentType,
                    out var category))
                category.TryRemove(component.Id, out _);

        if (component.Enable)
            component.Enable = false;
        component.Detach();
        _components.TryRemove(component.Id, out _);
    }

    /// <summary>
    /// Check whether this entity has the given component of not.
    /// </summary>
    /// <param name="component">Component to check.</param>
    /// <returns>True if this entity has the given component, otherwise false.</returns>
    public bool HasComponent(Component component)
    {
        return _components.ContainsKey(component.Id);
    }

    /// <summary>
    /// Acquire a component from the given category.
    /// </summary>
    /// <param name="componentType">Category of the component to get.</param>
    /// <returns>Component instance of the given type, or null if none matching.</returns>
    public Component? GetComponent(Type componentType)
    {
        if (!_categories.TryGetValue(componentType,
                out var category))
            return null;
        return category.IsEmpty ? null : category.First().Value;
    }

    /// <summary>
    /// Acquire components from the given category.
    /// </summary>
    /// <param name="componentType">Category of the components to get.</param>
    /// <returns>Component instances of the given type, or an empty array if none matching.</returns>
    public IEnumerable<Component> GetComponents(Type componentType)
    {
        return _categories.TryGetValue(componentType,
            out var category)
            ? category.Values
            : Array.Empty<Component>();
    }

    /// <summary>
    /// Invoke when this entity is required to enable.
    /// </summary>
    protected virtual void OnEnable()
    {
    }

    /// <summary>
    /// Invoke when this entity is required to disable.
    /// </summary>
    protected virtual void OnDisable()
    {
    }
}
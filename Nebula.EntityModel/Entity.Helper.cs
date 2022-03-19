namespace Nebula.EntityModel;

public static class EntityHelper
{
    /// <summary>
    /// Acquire a component from the given category.
    /// </summary>
    /// <param name="entity">Entity to get component from.</param>
    /// <typeparam name="TComponent">Component category.</typeparam>
    /// <returns>Component instance of the given type, or null if none matching.</returns>
    public static TComponent? GetComponent<TComponent>(this Entity entity)
        where TComponent : Component
    {
        return entity.GetComponent(typeof(TComponent)) as TComponent;
    }

    /// <summary>
    /// Acquire components from the given category.
    /// </summary>
    /// <param name="entity">Entity to get components from.</param>
    /// <typeparam name="TComponent">Component category.</typeparam>
    /// <returns>Components of the given category.</returns>
    public static IEnumerable<TComponent> GetComponents<TComponent>(this Entity entity)
        where TComponent : Component
    {
        return entity.GetComponents(typeof(TComponent)).Cast<TComponent>();
    }
}
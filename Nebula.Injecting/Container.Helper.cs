namespace Nebula.Injecting;

public static class ContainerHelper
{
    /// <summary>
    /// Declare an object in this container.
    /// </summary>
    /// <typeparam name="TCategory">Category type.</typeparam>
    /// <param name="container">Container to use.</param>
    /// <param name="name">Name of the object.</param>
    /// <returns>Object definition.</returns>
    public static Definition Declare<TCategory>(this Container container, string? name = null)
        => container.Declare(typeof(TCategory), name);

    /// <summary>
    /// Declare an object and bind a <see cref="Bind.Value"/> to it.
    /// /// This is equal to:
    /// <code>
    /// Declare(...).BindBuilder(Bind.Value(...));
    /// </code>
    /// </summary>
    /// <param name="container">Container to use.</param>
    /// <param name="value">Value to bind.</param>
    /// <param name="name">Name of the object.</param>
    /// <typeparam name="TCategory">Category of the object.</typeparam>
    /// <returns>Definition of the object.</returns>
    public static Definition DeclareValue<TCategory>(this Container container, object value, string? name = null)
        => container.Declare(typeof(TCategory), name).BindBuilder(Bind.Value(value));
    
    /// <summary>
    /// Declare an object and bind a <see cref="Bind.Array"/> to it.
    /// This is equal to:
    /// <code>
    /// Declare(...).BindBuilder(Bind.Value(...));
    /// </code>
    /// </summary>
    /// <param name="container">Container to use.</param>
    /// <param name="array">Array to bind.</param>
    /// <param name="name">Name of the object.</param>
    /// <typeparam name="TCategory">Category of the object.</typeparam>
    /// <returns>Definition of the object.</returns>
    public static Definition DeclareArray<TCategory>(this Container container, object?[] array, string? name = null)
        => container.Declare(typeof(TCategory), name).BindBuilder(Bind.Value(array));

    /// <summary>
    /// Revoke an object definition in this container.
    /// </summary>
    /// <typeparam name="TCategory">Category type.</typeparam>
    /// <param name="container">Container to use.</param>
    /// <param name="name">Name of the object.</param>
    public static void Revoke<TCategory>(this Container container, string? name = null)
        => container.Revoke(typeof(TCategory), name);

    /// <summary>
    /// Get an object from this container.
    /// </summary>
    /// <typeparam name="TCategory">Category type.</typeparam>
    /// <param name="container">Container to use.</param>
    /// <param name="name">Name of the object.</param>
    /// <returns>Object instance, or null if not found.</returns>
    public static TCategory? Get<TCategory>(this Container container, string? name = null)
        => (TCategory?)container.Get(typeof(TCategory), name);

    /// <summary>
    /// Construct an instance using the objects in this container to inject into the constructor.
    /// </summary>
    /// <typeparam name="TClass">Type to instantiate.</typeparam>
    /// <param name="container">Container to use.</param>
    /// <returns>Constructed instance.</returns>
    public static TClass Construct<TClass>(this Container container)
        => (TClass)container.Construct(typeof(TClass));
}
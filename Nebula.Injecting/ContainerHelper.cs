namespace Nebula.Injecting;

public static class ContainerHelper
{
    /// <summary>
    /// Get an object from the container.
    /// </summary>
    /// <param name="container">Container to use.</param>
    /// <param name="name">Optional name of the object to get.</param>
    /// <typeparam name="TType">Type of the object to get.</typeparam>
    /// <returns>Instance, or null if not found.</returns>
    public static object? Get<TType>(this Container container, string name = "") 
        => container.Get(typeof(TType), name);

    /// <summary>
    /// Preset an object in the container.
    /// </summary>
    /// <typeparam name="TCategory">Category of the object.</typeparam>
    /// <param name="container">Container to use.</param>
    /// <param name="name">Optional name of the object.</param>
    /// <returns>Preset instance.</returns>
    public static Preset Preset<TCategory>(this Container container, string name = "")
        => container.Preset(typeof(TCategory), name);

    /// <summary>
    /// Use objects in this container to inject an instance without a preset.
    /// This method <b>only</b> supports constructor injection and passive injection on public members.
    /// </summary>
    /// <typeparam name="TObject">Type of the object to instantiate and inject.</typeparam>
    /// <param name="container">Container to use.</param>
    public static TObject? Inject<TObject>(this Container container) where TObject : class
        => container.Inject(typeof(TObject)) as TObject;
}
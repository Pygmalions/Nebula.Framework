namespace Nebula.Pooling;

/// <summary>
/// Interface for object pools with only one type.
/// </summary>
/// <typeparam name="TObject">Type of pooled object.</typeparam>
public interface IPool<TObject> where TObject : class
{
    /// <summary>
    /// Get a pooled object of <typeparamref name="TObject"/> from this object pool.
    /// </summary>
    /// <returns>Pooled object taken from this pool.</returns>
    TObject Get();
    
    /// <summary>
    /// Put a pooled object of <typeparamref name="TObject"/> into this object pool.
    /// </summary>
    /// <param name="pooledObject">Pooled object to put into this object pool.</param>
    /// <returns>
    /// If true, then the given pooled object enters the pool and will be reused,
    /// otherwise it is not put into this pool.
    /// </returns>
    bool Put(TObject pooledObject);
}
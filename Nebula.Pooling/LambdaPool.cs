namespace Nebula.Pooling;

/// <summary>
/// A simple object pool implementation with lambda functions.
/// </summary>
/// <typeparam name="TObject">Type of pooled object.</typeparam>
public class LambdaPool<TObject> : IPool<TObject> where TObject : class
{
    private readonly System.Collections.Concurrent.ConcurrentBag<TObject> _pooledObjects = new();

    /// <summary>
    /// Construct a LambdaPool with the given lambda functions.
    /// </summary>
    /// <param name="creator">Optional lambda function for <see cref="Creator"/></param>
    /// <param name="cleaner">Optional lambda function for <see cref="Cleaner"/></param>
    public LambdaPool(Func<TObject>? creator = null, Func<TObject, bool>? cleaner = null)
    {
        Creator = creator ?? (() => Activator.CreateInstance<TObject>());
        Cleaner = cleaner ?? (pooledObject =>
        {
            _pooledObjects.Add(pooledObject);
            return true;
        });
    }

    /// <summary>
    /// Lambda function for create a new instance of the <typeparamref name="TObject"/>.
    /// Default behavior is to create an instance without construction arguments
    /// through <see cref="Activator"/>.CreateInstance.
    /// </summary>
    public Func<TObject> Creator;

    /// <summary>
    /// Lambda function for deciding whether an instance can be put into the pool or not,
    /// and this lambda function can reset the object before it goes into the pool.
    /// If it returns true, the given object will be allowed to put into the pool, vice versa.
    /// </summary>
    public Func<TObject, bool> Cleaner;

    /// <summary>
    /// The max count of pooled objects.
    /// If zero, then there is not limitation for the total count of pooled objects,
    /// otherwise, if the count of pooled object exceeds this number,
    /// new pooled object will not be put into this pool ignoring the return value of <see cref="Cleaner"/>.
    /// </summary>
    public uint MaxSize = 0;
    
    /// <inheritdoc />
    public TObject Get()
    {
        return _pooledObjects.TryTake(out var pooledObject) ? pooledObject : Creator();
    }

    /// <inheritdoc />
    public bool Put(TObject pooledObject)
    {
        if (MaxSize > 0 && _pooledObjects.Count < MaxSize) return false;
        if (!Cleaner(pooledObject)) return false;
        _pooledObjects.Add(pooledObject);
        return true;
    }
}
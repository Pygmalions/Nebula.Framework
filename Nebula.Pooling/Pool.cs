using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Nebula.Pooling;

public class Pool<TObject> : IPool<TObject> where TObject : class
{
    public Func<TObject> Creator { get; set; }
    
    /// <summary>
    /// Functor to clean and check whether the instance can be stored into the pool or not.
    /// If it returns false, the instance will not be stored in the pool.
    /// </summary>
    public Func<TObject, bool>? Cleaner { get; set; }

    /// <summary>
    /// Instances in this pool.
    /// </summary>
    private readonly ConcurrentBag<TObject> _instances = new();

    /// <summary>
    /// Construct a object pool with the given creator and optional cleaner.
    /// </summary>
    /// <param name="creator">Creator to create new instance.</param>
    /// <param name="cleaner">Optional cleaner to reset instance and check whether they can be stored.</param>
    /// <seealso cref="Creator"/>
    /// <seealso cref="Cleaner"/>
    public Pool(Func<TObject> creator, Func<TObject, bool>? cleaner = null)
    {
        Creator = creator;
        Cleaner = cleaner;
    }

    /// <summary>
    /// This max count of instances stored in this pool.
    /// Being 0 means that the max count is not limited.
    /// </summary>
    public uint MaxCount { get; set; } = 0;

    /// <summary>
    /// This method will try to take a instance from the pool,
    /// or use the creator to create one if the pool is empty.
    /// </summary>
    /// <returns>Instance from the pool or <see cref="Creator"/>.</returns>
    [return: NotNull]
    public virtual TObject? Acquire()
    {
        if (_instances.TryTake(out var instance))
            return instance;
        instance = Creator.Invoke();
        return instance;
    }

    /// <summary>
    /// The instance will be stored in the pool if the count has not reaches <see cref="MaxCount"/>
    /// and the <see cref="Cleaner"/> does not returns false is it exists.
    /// </summary>
    /// <param name="instance">Instance to store.</param>
    /// <returns>
    /// If true, the instance is stored in the pool;
    /// otherwise the instance is rejected.
    /// </returns>
    public virtual bool Store(TObject instance)
    {
        if (Cleaner?.Invoke(instance) == false || (MaxCount != 0 && _instances.Count >= MaxCount)) 
            return false;
        _instances.Add(instance);
        return true;
    }
}
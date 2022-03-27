namespace Nebula.Pooling;

/// <summary>
/// Interface for pools.
/// </summary>
/// <typeparam name="TObject">Type of objects stored in the pool.</typeparam>
public interface IPool<TObject> where TObject : class
{
    TObject? Acquire();

    bool Store(TObject instance);
}
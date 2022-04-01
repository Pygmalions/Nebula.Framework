using System.Reflection;

namespace Nebula.Injecting;

/// <summary>
/// Static helper factory class to wrap instances into a delegate.
/// </summary>
public static class Bind
{
    /// <summary>
    /// Wrap a value into a delegate.
    /// </summary>
    /// <param name="value">Value to bind.</param>
    /// <returns>Delegate which will always return the bound value.</returns>
    public static Builder.Item Value(object value)
        => new ValueBinder(value).Get;

    /// <summary>
    /// Wrap an array into a delegate.
    /// </summary>
    /// <param name="array">Array to bind.</param>
    /// <returns>Delegate which will always return the bound array.</returns>
    public static Builder.Array Array(params object?[] array)
        => new ArrayBinder(array).Get;

    /// <summary>
    /// Create a lazy delegate, which will only be executed for once and store the result
    /// for the following accesses.
    /// </summary>
    /// <param name="creator">Creator delegate to wrap.</param>
    /// <returns>
    /// Delegate which will execute the creator for once and returns the cached value in following accesses.
    /// </returns>
    public static Builder.Item LazyValue(Func<object> creator)
        => new LazyValueBinder(creator).Get;

    /// <summary>
    /// Create a lazy delegate, which will only be executed for once and store the result
    /// for the following accesses.
    /// </summary>
    /// <param name="creator">Creator delegate to wrap.</param>
    /// <returns>
    /// Delegate which will execute the creator for once and returns the cached value in following accesses.
    /// </returns>
    public static Builder.Array LazyArray(Func<object?[]> creator)
        => new LazyArrayBinder(creator).Get;

    /// <summary>
    /// Bind a value to a delegate.
    /// </summary>
    private class ValueBinder
    {
        private readonly object _instance;

        public ValueBinder(object instance)
        {
            _instance = instance;
        }

        public object Get(MemberInfo? member, object? holder) => _instance;
    }
    
    /// <summary>
    /// Bind an array to a delegate.
    /// </summary>
    private class ArrayBinder
    {
        private readonly object?[] _instance;

        public ArrayBinder(object?[] instance)
        {
            _instance = instance;
        }

        public object?[] Get(MemberInfo? member, object? holder) => _instance;
    }
    
    /// <summary>
    /// Provides cache / singleton support to an object delegate.
    /// </summary>
    private class LazyValueBinder
    {
        private object? _instance;

        private readonly Func<object> _creator;

        public LazyValueBinder(Func<object> creator)
        {
            _creator = creator;
        }

        public object Get(MemberInfo? member, object? holder)
        {
            if (_instance != null)
                return _instance;
            _instance = _creator();
            return _instance;
        }
    }

    /// <summary>
    /// Provides cache / singleton support to an array delegate.
    /// </summary>
    private class LazyArrayBinder
    {
        private object?[]? _instance;

        private readonly Func<object?[]> _creator;

        public LazyArrayBinder(Func<object?[]> creator)
        {
            _creator = creator;
        }

        public object?[] Get(MemberInfo? member, object? holder)
        {
            if (_instance != null)
                return _instance;
            _instance = _creator();
            return _instance;
        }
        
    }
}
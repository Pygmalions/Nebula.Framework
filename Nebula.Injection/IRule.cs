namespace Nebula.Injection;

public interface IRule
{
    /// <summary>
    /// Get an instance of the given type according to the given injection attribute.
    /// </summary>
    /// <param name="source">Source to configure the instance.</param>
    /// <param name="type">Type of the instance to get.</param>
    /// <param name="attribute">External injection information attribute.</param>
    /// <returns>Instance as required.</returns>
    object Get(ISource source, Type type, InjectionAttribute? attribute);

    /// <summary>
    /// Get an instance of the given type according to the given injection attribute.
    /// </summary>
    /// <param name="source">Source to configure the instance.</param>
    /// <param name="type">Type of the instance to get.</param>
    /// <param name="attribute">External injection information attribute.</param>
    /// <returns>
    ///  If true, this source can provide object as required,
    /// otherwise <see cref="Get"/> will throw an exception.
    /// </returns>
    bool Acceptable(ISource source, Type type, InjectionAttribute? attribute);
}
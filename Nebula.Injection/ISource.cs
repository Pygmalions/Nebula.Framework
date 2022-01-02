namespace Nebula.Injection;

public interface ISource : IConfigurator
{
    /// <summary>
    /// Acquire the instance of the given type according to the given attribute.
    /// </summary>
    /// <param name="type">Type of instance to acquire. If nullable, will be considered as its underlying type.</param>
    /// <param name="attribute">Attribute which provides external injection information.</param>
    /// <returns>Instance as required.</returns>
    object Acquire(Type type, InjectionAttribute? attribute);

    /// <summary>
    /// Check whether or not this source can provide the instance of the given type according to the given attribute.
    /// Attention, this method returning true does not means no exception will be thrown in all circumstances,
    /// for example, if some of the dependent interfaces are missing in this source,
    /// the <see cref="Acquire"/> will still throw an exception.
    /// </summary>
    /// <param name="type">Type of instance to acquire.</param>
    /// <param name="attribute">Attribute which provides external injection information.</param>
    /// <returns>
    /// If true, this source can provide object as required,
    /// otherwise <see cref="Acquire"/> will throw an exception.
    /// </returns>
    bool Acquirable(Type type, InjectionAttribute? attribute);
}
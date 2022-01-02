namespace Nebula.Injection;

public interface IConfigurator
{
    /// <summary>
    /// Scan an instance and inject fields, properties and methods as InjectionAttribute marked.
    /// </summary>
    /// <param name="instance">Object to configure and inject.</param>
    void Configure(object instance);
}
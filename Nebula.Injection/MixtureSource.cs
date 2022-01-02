namespace Nebula.Injection;

public class MixtureSource : ISource
{
    private readonly List<ISource> _sources = new();

    public IReadOnlyList<ISource> Sources => _sources;

    /// <summary>
    /// Use the first source to configure the given instance.
    /// </summary>
    /// <param name="instance">Instance to configure.</param>
    /// <exception cref="Exception">
    /// Throw if this mixture source is empty.
    /// </exception>
    public void Configure(object instance)
    {
        if (_sources.Count == 0)
            throw new Exception("No source in the mixture source.");
        _sources.First().Configure(instance);
    }

    public void Add(ISource source)
    {
        if (!_sources.Contains(source))
            _sources.Add(source);
    }

    public void Remove(ISource source)
    {
        _sources.Remove(source);
    }
    
    public object Acquire(Type type, InjectionAttribute? attribute)
    {
        if (Sources.Count == 0)
            throw new Exception("No source in the mixture source.");
        foreach (var source in _sources.Where(source => source.Acquirable(type, attribute)))
        {
            return source.Acquire(type, attribute);
        }

        throw new Exception("Can not find a source which can provide a instance " +
                            "with the given type and attribute.");
    }

    public bool Acquirable(Type type, InjectionAttribute? attribute)
    {
        return Sources.Count != 0 && _sources.Any(source => source.Acquirable(type, attribute));
    }
}
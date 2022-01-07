using System.Collections.Concurrent;

namespace Nebula.Injection.Rules;

/// <summary>
/// This ruleset will try to find the rule with the name marked in the attribute.
/// </summary>
public class NameMatchRuleset : Ruleset
{
    private readonly ConcurrentDictionary<string, IRule> _rules = new();

    public IReadOnlyDictionary<string, IRule> Rules => _rules;

    public override object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        IRule? rule;
        if (attribute is NamedInjectionAttribute namedAttribute)
        {
            if (!_rules.TryGetValue(namedAttribute.Name, out rule))
                throw new Exception($"No matching rule for the given name {namedAttribute.Name}.");
        }
        else
        {
            if (_rules.IsEmpty)
                throw new Exception("The given attribute is not a NamedInjectionAttribute, " +
                                    "and this ruleset is empty.");
            rule = _rules.First().Value;
        }
        return rule.Get(source, type, attribute);
    }

    public override bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        if (attribute is not NamedInjectionAttribute namedAttribute)
            throw new Exception("Given attribute is not a NamedInjectionAttribute.");
        return _rules.TryGetValue(namedAttribute.Name, out var rule) && rule.Acceptable(source, type, attribute);
    }

    /// <summary>
    /// Add a rule to this ruleset.
    /// </summary>
    /// <param name="rule">Rule to add.</param>
    /// <param name="name">Name of the rule.</param>
    public NameMatchRuleset Add(IRule rule, string name)
    {
        _rules[name] = rule;
        return this;
    }

    /// <summary>
    /// Remove a rule from this ruleset.
    /// </summary>
    /// <param name="name">Name of the rule to remove.</param>
    public NameMatchRuleset Remove(string name)
    {
        _rules.TryRemove(name, out _);
        return this;
    }
}
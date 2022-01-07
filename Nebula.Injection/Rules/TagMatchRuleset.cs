using System.Collections.Concurrent;

namespace Nebula.Injection.Rules;

/// <summary>
/// This ruleset will try to find the registered rule which has all the tags marked in the attribute.
/// </summary>
public class TagMatchRuleset : Ruleset
{
    private readonly ConcurrentDictionary<IRule, IReadOnlySet<string>> _rules = new();

    public IReadOnlyDictionary<IRule, IReadOnlySet<string>> Rules => _rules;

    private IRule? SearchRule(IReadOnlySet<string> requiredTgs)
    {
        foreach (var (rule, tags) in _rules)
        {
            var matched = requiredTgs.All(
                requiredTag => tags.Contains(requiredTag));

            if (!matched)
                continue;
            return rule;
        }

        return null;
    }

    public override object Get(ISource source, Type type, InjectionAttribute? attribute)
    {
        IRule? rule;
        if (attribute is TaggedInjectionAttribute taggedAttribute)
        {
            rule = SearchRule(taggedAttribute.Tags);
            if (rule == null)
                throw new Exception($"Can not find rule that matches the given tags: {taggedAttribute.Tags}");
        }
        else
        {
            if (_rules.IsEmpty)
                throw new Exception("The given attribute is not a TaggedInjectionAttribute, " +
                                    "and this ruleset is empty.");
            rule = _rules.First().Key;
        }
        return rule.Get(source, type, attribute);
    }

    public override bool Acceptable(ISource source, Type type, InjectionAttribute? attribute)
    {
        if (attribute is not TaggedInjectionAttribute taggedAttribute)
            throw new Exception("Given attribute is not a TaggedInjectionAttribute.");
        var foundRule = SearchRule(taggedAttribute.Tags);
        return foundRule != null && foundRule.Acceptable(source, type, attribute);
    }

    /// <summary>
    /// Add a rule to this ruleset.
    /// </summary>
    /// <param name="rule">Rule to add.</param>
    /// <param name="tags">Tags of the rule.</param>
    public TagMatchRuleset Add(IRule rule, params string[] tags)
    {
        _rules[rule] = new HashSet<string>(tags);
        return this;
    }

    /// <summary>
    /// Remove a rule from this ruleset.
    /// </summary>
    /// <param name="rule">Rule to remove.</param>
    public TagMatchRuleset Remove(IRule rule)
    {
        _rules.TryRemove(rule, out _);
        return this;
    }
}
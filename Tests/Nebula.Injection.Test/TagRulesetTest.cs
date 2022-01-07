using Nebula.Injection.Rules;
using Nebula.Injection.Test.Samples;
using NUnit.Framework;

namespace Nebula.Injection.Test;

public class TagMatchRulesetTest
{
    [Test]
    public void TagMatchTest()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new TagMatchRuleset()
                .Add(new SimpleRule<ComponentA>(), "Component", "A")
                .Add(new SimpleRule<ComponentB>(), "Component", "B")
        };
        var result = source.Acquire<SampleTaggedInjection>();
        Assert.NotNull(result.UntaggedComponent);
        Assert.IsInstanceOf<ComponentA>(result.ComponentA);
        Assert.IsInstanceOf<ComponentB>(result.ComponentB);
    }
}
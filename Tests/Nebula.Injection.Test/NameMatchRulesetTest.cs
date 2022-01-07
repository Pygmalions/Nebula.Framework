using Nebula.Injection.Rules;
using Nebula.Injection.Test.Samples;
using NUnit.Framework;

namespace Nebula.Injection.Test;

public class NameMatchRulesetTest
{
    [Test]
    public void NameMatchTest()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new NameMatchRuleset()
                .Add(new SimpleRule<ComponentA>(), "ComponentA")
                .Add(new SimpleRule<ComponentB>(), "ComponentB")
        };
        var result = source.Acquire<SampleNamedInjection>();
        Assert.NotNull(result.UnnamedComponent);
        Assert.IsInstanceOf<ComponentA>(result.ComponentA);
        Assert.IsInstanceOf<ComponentB>(result.ComponentB);
    }
}
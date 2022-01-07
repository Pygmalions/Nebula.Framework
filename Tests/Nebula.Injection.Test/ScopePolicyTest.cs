using Nebula.Injection.Rules;
using Nebula.Injection.Test.Samples;
using NUnit.Framework;

namespace Nebula.Injection.Test;

public class ScopePolicyTest
{
    [Test]
    public void ScopeTest()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new ScopePolicy(new LambdaRule((_, _, _) => new ComponentA()))
        };
        var component1 = source.Acquire<ISampleComponent>();
        var component2 = source.Acquire<ISampleComponent>();
        Assert.AreSame(component1, component2);
    }
}
using Nebula.Injection.Rules;
using Nebula.Injection.Test.Samples;
using NUnit.Framework;

namespace Nebula.Injection.Test;

public class SingletonPolicyTest
{
    [Test]
    public void SingletonTest()
    {
        var source1 = new RuleSource
        {
            [typeof(ISampleComponent)] = new SingletonPolicy(typeof(ComponentA),
                new LambdaRule((_, _, _) => new ComponentA()))
        };
        var component1 = source1.Acquire<ISampleComponent>();
        var component2 = source1.Acquire<ISampleComponent>();
        Assert.AreSame(component1, component2);
        
        var source2 = new RuleSource
        {
            [typeof(ISampleComponent)] = new SingletonPolicy(typeof(ComponentA),
                new LambdaRule((_, _, _) => new ComponentA()))
        };
        var component3 = source2.Acquire<ISampleComponent>();
        var component4 = source2.Acquire<ISampleComponent>();
        Assert.AreSame(component3, component4);
        Assert.AreSame(component1, component3);
    }
}
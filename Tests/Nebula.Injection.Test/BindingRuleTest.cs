using System.ComponentModel;
using Nebula.Injection.Rules;
using NUnit.Framework;
using Nebula.Injection.Test.Samples;

namespace Nebula.Injection.Test;

public class BindingRuleTest
{
    [Test]
    public void ConstructBindingRule()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new BindingRule(typeof(ComponentA))
        };
        ISampleComponent? component = null;
        Assert.DoesNotThrow(() =>
        {
            component = source.Acquire<ISampleComponent>();
        });
        Assert.NotNull(component);
        Assert.AreEqual(component?.GetComponentName(), "A");

        source[typeof(ISampleComponent)] = new BindingRule(typeof(ComponentB));
        Assert.DoesNotThrow(() =>
        {
            component = source.Acquire<ISampleComponent>();
        });
        Assert.NotNull(component);
        Assert.AreEqual(component?.GetComponentName(), "B");
    }

    [Test]
    public void ConstructorInjection()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new BindingRule(typeof(ComponentA))
        };
        SampleConstructorInjection? result = null;
        Assert.DoesNotThrow(() =>
        {
            result = source.Acquire<SampleConstructorInjection>();
        });
        Assert.NotNull(result);
        Assert.AreEqual("A", result?.ComponentName);
    }

    [Test]
    public void MemberInjection()
    {
        var source = new RuleSource
        {
            [typeof(ISampleComponent)] = new BindingRule(typeof(ComponentA))
        };
        SampleMemberInjection? result = null;
        Assert.DoesNotThrow(() =>
        {
            result = source.Acquire<SampleMemberInjection>() as SampleMemberInjection;
        });
        Assert.NotNull(result);
        Assert.True(result?.FieldComponent is ComponentA);
        Assert.True(result?.PropertyComponent is ComponentA);
        Assert.True(result?.MethodComponent is ComponentA);
    }
}
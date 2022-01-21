using System;
using Nebula.Proxying;
using NUnit.Framework;

namespace Nebula.Prism.Test;

public class DynamicGeneratorTest
{

    [Test]
    public void GeneratedCodeCorrectness()
    {
        Type? refraction = null;
        Assert.DoesNotThrow(() =>
        {
            refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        });
        Assert.NotNull(refraction);
        Assert.DoesNotThrow(() => Activator.CreateInstance(refraction!));
    }
    
    [Test]
    public void InterfaceCompleteness()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
    }

    [Test]
    public void GetFunctionProxy()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        // Verify constructor function (change the Number from default 0 to -1).
        Assert.AreEqual(-1, testSample!.Number);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        // Verify the existence of the proxy.
        Assert.NotNull(
            provider!.GetMethodProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!));
    }
    
    [Test]
    public void GetPropertyProxy()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        // Verify constructor function (change the Number from default 0 to -1).
        Assert.AreEqual(-1, testSample!.Number);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        // Verify the existence of the proxy.
        Assert.NotNull(
            provider!.GetPropertyProxy(typeof(SampleObject).GetProperty(nameof(SampleObject.Number))!));
    }

    [Test]
    public void ProxiedProperty()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var number = testSample!.Number;
        Assert.AreEqual(-1, number);
        testSample.Number = 2;
        Assert.AreEqual(2, testSample.Number);
    }
}
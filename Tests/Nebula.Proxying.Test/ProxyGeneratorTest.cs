using System;
using NUnit.Framework;

namespace Nebula.Proxying.Test;

[TestFixture]
public class GeneratorTest
{
    [Test]
    public void GeneratedCodeCorrectness()
    {
        Type? refraction = null;
        Assert.DoesNotThrow(() => { refraction = ProxyGenerator.Get<SampleObject>(); });
        Assert.NotNull(refraction);
        Assert.DoesNotThrow(() => Activator.CreateInstance(refraction!));
    }

    [Test]
    public void InterfaceCompleteness()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        var result = provider!.GetProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!);
        Assert.NotNull(result);
    }

    [Test]
    public void GetFunctionProxy()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        // Verify constructor function (change the Number from default 0 to -1).
        Assert.AreEqual(-1, testSample!.Number);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        // Verify the existence of the proxy.
        Assert.NotNull(
            provider!.GetProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!));
    }

    [Test]
    public void GetPropertyProxy()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        // Verify constructor function (change the Number from default 0 to -1).
        Assert.AreEqual(-1, testSample!.Number);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        // Verify the existence of the proxy.
        Assert.NotNull(
            provider!.GetProxy(typeof(SampleObject).GetProperty(nameof(SampleObject.Number))!.SetMethod!));
    }

    [Test]
    public void ProxiedPropertyGet()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var number = testSample!.Number;
        Assert.AreEqual(-1, number);
    }

    [Test]
    public void ProxiedPropertySet()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        testSample!.Number = 2;
        Assert.AreEqual(2, testSample.PropertyNumber);
    }

    [Test]
    public void ProxiedMethod()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var number = testSample!.PropertyNumber;
        Assert.AreEqual(number + 1, testSample.AddNumber(1));
        Assert.AreEqual(number + 1, testSample.PropertyNumber);
    }

    [Test]
    public void MethodProxyFunction()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        var proxy = provider!.GetProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!);
        Assert.NotNull(proxy);
        bool invokingTriggered = false, invokedTriggered = false;
        var argument = -1;
        proxy!.Invoking += context =>
        {
            invokingTriggered = true;
            if (context.Arguments[0] is int parameter)
                argument = parameter;
            context.Arguments[0] = 5;
        };
        var returningValue = -1;
        proxy.Invoked += context =>
        {
            invokedTriggered = true;
            if (context.Result is int value)
                returningValue = value;
            context.Return(10);
        };
        var initialNumber = testSample!.Number;
        var result = testSample.AddNumber(3);
        Assert.True(invokingTriggered);
        Assert.AreEqual(3, argument);
        Assert.True(invokedTriggered);
        Assert.AreEqual(initialNumber + 5, returningValue);
        Assert.AreEqual(10, result);
    }

    [Test]
    public void PluginTest()
    {
        var refraction = ProxyGenerator.Get<SampleObject>();
        Activator.CreateInstance(refraction);
        Assert.IsNotEmpty(SampleGeneratorPlugin.GeneratedProxies);
    }
}
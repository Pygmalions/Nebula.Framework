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
    
    [Test]
    public void ProxiedMethod()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var number = testSample!.Number;
        Assert.AreEqual(number + 1, testSample.AddNumber(1));
        Assert.AreEqual(number + 1, testSample.Number);
    }

    [Test]
    public void MethodProxyFunction()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        var proxy = provider!.GetMethodProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!)
            as IExtensibleMethod;
        Assert.NotNull(proxy);
        bool invokingTriggered = false, invokedTriggered = false;
        var argument = -1;
        proxy!.Invoking += (context) =>
        {
            invokingTriggered = true;
            if (context.Arguments[0] is int parameter)
                argument = parameter;
            context.Arguments[0] = 5;
        };
        var returningValue = -1;
        proxy.Invoked += (context) =>
        {
            invokedTriggered = true;
            if (context.ReturningValue is int value)
                returningValue = value;
            context.ReturningValue = 10;
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
    public void PropertyProxySettingFunction()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        var proxy = provider!.GetPropertyProxy(typeof(SampleObject).GetProperty(nameof(SampleObject.Number))!)
            as IExtensibleProperty;
        Assert.NotNull(proxy);
        bool settingTriggered = false, afterSettingTriggered = false;
        var settingValue = -1;
        proxy!.Setting += (context) =>
        {
            settingTriggered = true;
            if (context.AccessingValue is int number)
                settingValue = number;
            context.AccessingValue = 10;
        };
        proxy.AfterSetting += (context) =>
        {
            afterSettingTriggered = true;
        };
        testSample!.Number = 3;
        Assert.True(settingTriggered);
        Assert.AreEqual(3, settingValue);
        Assert.AreEqual(10, testSample.Number);
        Assert.True(afterSettingTriggered);
    }
    
    [Test]
    public void PropertyProxyGettingFunction()
    {
        var refraction = DynamicGenerator.GlobalInstance.GetRefraction<SampleObject>();
        var testSample = Activator.CreateInstance(refraction) as SampleObject;
        Assert.NotNull(testSample);
        var provider = testSample as IProxiedObject;
        Assert.NotNull(provider);
        var proxy = provider!.GetPropertyProxy(typeof(SampleObject).GetProperty(nameof(SampleObject.Number))!)
            as IExtensibleProperty;
        Assert.NotNull(proxy);
        bool gettingTriggered = false, afterGettingTriggered = false;
        var gettingValue = -1;
        proxy!.Getting += (context) =>
        {
            gettingTriggered = true;
        };
        proxy.AfterGetting += (context) =>
        {
            afterGettingTriggered = true;
            if (context.AccessingValue is int number)
                gettingValue = number;
            context.AccessingValue = 10;
        };
        var result = testSample.Number;
        Assert.True(gettingTriggered);
        Assert.AreEqual(10, result);
        Assert.AreEqual(-1, gettingValue);
        Assert.True(afterGettingTriggered);
    }
}
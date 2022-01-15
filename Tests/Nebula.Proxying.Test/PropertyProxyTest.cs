using NUnit.Framework;

namespace Nebula.Proxying.Test;

public class PropertyProxyTest
{
    [Test]
    public void ProxiesPropertyFunction()
    {
        // Test setting.
        var testTarget = new SampleObject
        {
            Number = 3
        };
        Assert.AreEqual(3, testTarget.NumberBody);
        // Test getting.
        Assert.AreEqual(3, testTarget.Number);
    }

    [Test]
    public void PreprocessingGet()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;

        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Getting += context =>
        {
            proxyInvoked = true;
        };
        var _ = testTarget.Number;
        Assert.True(proxyInvoked);
    }

    [Test]
    public void PostprocessingGet()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;
        var propertyValue = -1;
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);

        testTarget.NumberBody = 3;
        
        proxy!.AfterGetting += context =>
        {
            proxyInvoked = true;
            if (context.AccessingValue is int number)
                propertyValue = number;
            context.AccessingValue = 5;
        };
        
        var modifiedNumber = testTarget.Number;
        Assert.True(proxyInvoked);
        Assert.AreEqual(3, propertyValue);
        Assert.AreEqual(5, modifiedNumber);
    }
    
    [Test]
    public void PreprocessingSet()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;
        var accessingValue = -1;
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);

        proxy!.Setting += context =>
        {
            proxyInvoked = true;
            if (context.AccessingValue is int number)
                accessingValue = number;
            context.AccessingValue = 8;
        };

        testTarget.Number = 5;
        Assert.True(proxyInvoked);
        Assert.AreEqual(5, accessingValue);
        Assert.AreEqual(8, testTarget.NumberBody);
    }

    [Test]
    public void PostprocessingSet()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;
        var propertyValue = -1;
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Setting += context =>
        {
            proxyInvoked = true;
            if (context.AccessingValue is int number)
                propertyValue = number;
        };
        testTarget.Number = 5;
        Assert.True(proxyInvoked);
        Assert.AreEqual(5, propertyValue);
    }

    [Test]
    public void SkipGet()
    {
        var testTarget = new SampleObject();
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Getting += context =>
        {
            context.Skip();
            context.AccessingValue = 8;
        };

        var postProxyInvoked = false;
        proxy.AfterGetting += context =>
        {
            postProxyInvoked = true;
        };

        var number = testTarget.Number;
        Assert.AreEqual(8, number);
        Assert.True(postProxyInvoked);
    }
    
    [Test]
    public void StopGet()
    {
        var testTarget = new SampleObject();
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Getting += context =>
        {
            context.Stop();
            context.AccessingValue = 8;
        };

        var stopped = true;
        proxy.AfterGetting += context =>
        {
            stopped = false;
        };

        var number = testTarget.Number;
        Assert.AreEqual(8, number);
        Assert.True(stopped);
    }
    
    [Test]
    public void SkipSet()
    {
        var testTarget = new SampleObject();
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Setting += context =>
        {
            context.Skip();
            context.AccessingValue = 5;
        };

        var postProxyInvoked = false;
        proxy.AfterSetting += context =>
        {
            postProxyInvoked = true;
        };

        testTarget.NumberBody = 1;
        testTarget.Number = 3;
        Assert.AreEqual(1, testTarget.NumberBody);
        Assert.True(postProxyInvoked);
    }
    
    [Test]
    public void StopSet()
    {
        var testTarget = new SampleObject();
        
        var proxy = testTarget.GetPropertyProxy(testTarget.GetType().GetProperty(nameof(testTarget.Number))!)
            as DecoratedProperty;

        Assert.NotNull(proxy);
        
        proxy!.Setting += context =>
        {
            context.Stop();
            context.AccessingValue = 8;
        };

        var stopped = true;
        proxy.AfterSetting += context =>
        {
            stopped = false;
        };

        testTarget.NumberBody = 1;
        testTarget.Number = 3;
        Assert.AreEqual(1, testTarget.NumberBody);
        Assert.True(stopped);
    }
}
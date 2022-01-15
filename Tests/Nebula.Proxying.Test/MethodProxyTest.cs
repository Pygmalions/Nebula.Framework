using NUnit.Framework;

namespace Nebula.Proxying.Test;

public class MethodProxyTest
{
    [Test]
    public void ProxiedMethodFunction()
    {
        var testTarget = new SampleObject();
        Assert.AreEqual("test", testTarget.GetEchoString("test"));
    }
    
    [Test]
    public void Preprocessing()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;
        var argumentText = "";

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.GetEchoString))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);
        
        methodProxy!.Invoking += context =>
        {
            proxyInvoked = true;
            if (context.Arguments[0] is string text)
                argumentText = text;
        };

        var echo = testTarget.GetEchoString("test");
        
        Assert.AreEqual("test", echo);
        Assert.AreEqual("test", argumentText);
        Assert.True(proxyInvoked);
    }
    
    [Test]
    public void Postprocessing()
    {
        var testTarget = new SampleObject();
        
        var proxyInvoked = false;
        var returningText = "";

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.GetEchoString))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);
        
        methodProxy!.Invoked += context =>
        {
            proxyInvoked = true;
            if (context.ReturningValue is string text)
                returningText = text;
        };

        var echo = testTarget.GetEchoString("test");
        
        Assert.AreEqual("test", echo);
        Assert.AreEqual("test", returningText);
        Assert.True(proxyInvoked);
    }

    [Test]
    public void ReplaceParameter()
    {
        var testTarget = new SampleObject();

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.GetEchoString))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);
        
        methodProxy!.Invoking += (context) =>
        {
            context.Arguments[0] = "new text";
        };

        var echo = testTarget.GetEchoString("test");
        
        Assert.AreEqual("new text", echo);
    }

    [Test]
    public void ReplaceReturningValue()
    {
        var testTarget = new SampleObject();

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.GetEchoString))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);
        
        methodProxy!.Invoked += (context) =>
        {
            context.ReturningValue = "new text";
        };

        var echo = testTarget.GetEchoString("test");
        
        Assert.AreEqual("new text", echo);
    }

    [Test]
    public void SkipInvocation()
    {
        var testTarget = new SampleObject();

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.AddTick))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);

        testTarget.AddTick(1);
        
        Assert.AreEqual(1, testTarget.Tick);
        
        var postprocessorInvoked = false;
        
        methodProxy!.Invoking += context =>
        {
            context.ReturningValue = 20;
            context.Skip();
        };

        methodProxy.Invoked += _ =>
        {
            postprocessorInvoked = true;
        };
        
        var returningTick = testTarget.AddTick(1);
        Assert.AreEqual(1, testTarget.Tick);
        Assert.AreEqual(20, returningTick);
        Assert.True(postprocessorInvoked);
    }
    
    [Test]
    public void StopInvocation()
    {
        var testTarget = new SampleObject();

        var methodProxy = testTarget.GetMethodProxy(testTarget.GetType().GetMethod(nameof(testTarget.AddTick))!)
            as ExtensibleMethod;

        Assert.NotNull(methodProxy);

        testTarget.AddTick(1);
        
        Assert.AreEqual(1, testTarget.Tick);
        
        var postprocessorInvoked = false;
        
        methodProxy!.Invoking += context =>
        {
            context.ReturningValue = 20;
            context.Stop();
        };

        methodProxy.Invoked += _ =>
        {
            postprocessorInvoked = true;
        };

        var returningTick = testTarget.AddTick(1);
        Assert.AreEqual(1, testTarget.Tick);
        Assert.AreEqual(20,  returningTick);
        Assert.False(postprocessorInvoked);
    }
}
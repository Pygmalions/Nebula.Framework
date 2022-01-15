using NUnit.Framework;

namespace Nebula.Proxying.Test;

public class ProxyManagerTest
{
    [Test]
    public void WholeFunctionTest()
    {
        var testTarget = new SampleObject();

        var methodProxy = testTarget.GetMethodProxy(
            testTarget.GetType().GetMethod(nameof(SampleObject.GetEchoString))!);
        Assert.NotNull(methodProxy);

        var propertyProxy = testTarget.GetPropertyProxy(
            testTarget.GetType().GetProperty(nameof(SampleObject.Number))!);
        Assert.NotNull(propertyProxy);
    }
}
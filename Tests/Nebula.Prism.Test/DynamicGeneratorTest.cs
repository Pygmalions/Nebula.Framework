using System;
using Nebula.Proxying;
using NUnit.Framework;

namespace Nebula.Prism.Test;

public class DynamicGeneratorTest
{
    [Test]
    public void ConstructorFunction()
    {
        var refraction = DynamicGenerator.GetRefraction<SampleObject>();
        var testObject = Activator.CreateInstance(refraction);
        var testSample = testObject as SampleObject;
        Assert.NotNull(testObject);
        // Verify constructor function (change the Number from default 0 to -1).
        Assert.AreEqual(-1, testSample!.Number);
        var provider = testObject as IProxiedObject;
        Assert.NotNull(provider);
        // Verify the existence of the proxy.
        Assert.NotNull(
            provider!.GetMethodProxy(typeof(SampleObject).GetMethod(nameof(SampleObject.AddNumber))!));
        // Verify the normal function of AddNumber.
        var result = testSample.AddNumber(1);
        Assert.AreEqual(0, result);
        Assert.AreEqual(0, testSample.Number);

        testSample.AddNumber(1);
    }
}
using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class ContainerTest
{
    [Test]
    public void ContainerPresetTest()
    {
        var container = new Container();
        container.Declare<SampleObject>()
            .AsPreset()
            .SetField("SampleField", Bind.Value(1))
            .SetProperty("SampleProperty", Bind.Value(2));
        var instance = container.Get<SampleObject>();
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance!.SampleField);
        Assert.AreEqual(2, instance.SampleProperty);
    }

    [Test]
    public void ContainerPresetPassiveTest()
    {
        var container = new Container();
        container.DeclareValue<int>(1, "SampleField");
        container.DeclareValue<int>(2, "SampleProperty");
        container.DeclareValue<int>(3, "SampleMethod");
        container.DeclareValue<int>(4, "SampleConstructor");
        container.Declare<PassiveSampleObject>();
        var instance = container.Get<PassiveSampleObject>()!;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance.SampleField);
        Assert.AreEqual(2, instance.SampleProperty);
        Assert.AreEqual(3, instance.MethodValue);
        Assert.AreEqual(4, instance.ConstructorValue);
    }
}
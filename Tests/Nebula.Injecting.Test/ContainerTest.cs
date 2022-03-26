using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class ContainerTest
{
    [Test]
    public void ContainerPresetTest()
    {
        var container = new Container();
        var instance = container.Preset<SampleObject>()
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?
            .BindArguments(() => new object?[] {4})
            .Build() as SampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance?.SampleField);
        Assert.AreEqual(2, instance?.SampleProperty);
        Assert.AreEqual(3, instance?.MethodValue);
        Assert.AreEqual(4, instance?.ConstructorValue);
    }
    
    [Test]
    public void ContainerPresetAcquireTest()
    {
        var container = new Container();
        container.Preset<SampleObject>()
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?
            .BindArguments(() => new object?[] {4});
        var instance = container.Get<SampleObject>() as SampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance?.SampleField);
        Assert.AreEqual(2, instance?.SampleProperty);
        Assert.AreEqual(3, instance?.MethodValue);
        Assert.AreEqual(4, instance?.ConstructorValue);
    }
    
    [Test]
    public void ContainerPresetPassiveTest()
    {
        var container = new Container();
        container.Preset<int>("SampleField").SetBuilder().AsBuilder()?
            .BindInstance(1);
        container.Preset<int>("SampleProperty").SetBuilder().AsBuilder()?
            .BindInstance(2);
        container.Preset<int>("SampleMethod").SetBuilder().AsBuilder()?
            .BindInstance(3);
        container.Preset<int>("SampleConstructor").SetBuilder().AsBuilder()?
            .BindInstance(4);
        var instance = container.Preset<PassiveSampleObject>()
            .SetBuilder()
            .AsBuilder()?
            .Build() as PassiveSampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance?.SampleField);
        Assert.AreEqual(2, instance?.SampleProperty);
        Assert.AreEqual(3, instance?.MethodValue);
        Assert.AreEqual(4, instance?.ConstructorValue);
    }
    
    [Test]
    public void ContainerPresetPassiveAcquireTest()
    {
        var container = new Container();
        container.Preset<int>("SampleField").SetBuilder().AsBuilder()?
            .BindInstance(1);
        container.Preset<int>("SampleProperty").SetBuilder().AsBuilder()?
            .BindInstance(2);
        container.Preset<int>("SampleMethod").SetBuilder().AsBuilder()?
            .BindInstance(3);
        container.Preset<int>("SampleConstructor").SetBuilder().AsBuilder()?
            .BindInstance(4);
        container.Preset<PassiveSampleObject>().SetBuilder();
        var instance = container.Get<PassiveSampleObject>() as PassiveSampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance?.SampleField);
        Assert.AreEqual(2, instance?.SampleProperty);
        Assert.AreEqual(3, instance?.MethodValue);
        Assert.AreEqual(4, instance?.ConstructorValue);
    }
}
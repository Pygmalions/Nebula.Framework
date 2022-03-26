using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class Tests
{
    [Test]
    public void PresetInject()
    {
        var instance = new Preset(typeof(SampleObject))
            .PresetField("SampleField", () => 1)
            .PresetProperty("SampleProperty", () => 2)
            .InvokeMethod("SampleMethod", () => new object?[] { 3 })
            .SetBuilder()
            .AsBuilder()?.Build() as SampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(1, instance?.SampleField);
        Assert.AreEqual(2, instance?.SampleProperty);
        Assert.AreEqual(3, instance?.MethodValue);
    }

    [Test]
    public void ConstructorInject()
    {
        var instance = new Preset(typeof(SampleObject))
            .SetBuilder()
            .AsBuilder()?.BindArguments(() => new object?[]{4})
            .Build() as SampleObject;
        Assert.NotNull(instance);
        Assert.AreEqual(4, instance?.ConstructorValue);
    }
}
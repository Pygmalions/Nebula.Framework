using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class Tests
{
    [Test]
    public void PresetInject()
    {
        var injector = new Preset()
            .SetField("SampleField", Bind.Value(1))
            .SetProperty("SampleProperty", Bind.Value(2))
            .InvokeMethod("SampleMethod", Bind.Array(3));
        var instance = new SampleObject();
        injector.Inject(instance);
        Assert.AreEqual(1, instance.SampleField);
        Assert.AreEqual(2, instance.SampleProperty);
        Assert.AreEqual(3, instance.MethodValue);
    }
}
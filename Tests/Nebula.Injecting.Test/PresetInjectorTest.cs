using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class PresetInjectorTest
{
    [Test]
    public void InjectField()
    {
        var builder = new InjectionBuilder();
        builder.InjectObject(nameof(SampleObject.InjectionField), 1);

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, builder.BuildPreset());

        Assert.AreEqual(1, sampleObject.InjectionField);
        
        builder.Clear();
        
        builder.InjectObject(nameof(SampleObject.InjectionField), 2);
        
        Injector.Inject(sampleObject, builder.BuildPreset());
        
        Assert.AreEqual(2, sampleObject.InjectionField);
    }
    
    [Test]
    public void InjectProperty()
    {
        var builder = new InjectionBuilder();
        builder.InjectObject(nameof(SampleObject.InjectionProperty), 1);

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, builder.BuildPreset());

        Assert.AreEqual(1, sampleObject.InjectionProperty);
        
        builder.Clear();
        
        builder.InjectObject(nameof(SampleObject.InjectionProperty), 2);
        
        Injector.Inject(sampleObject, builder.BuildPreset());
        
        Assert.AreEqual(2, sampleObject.InjectionProperty);
    }
    
    [Test]
    public void InjectMethod()
    {
        var builder = new InjectionBuilder();
        builder.InvokeMethod(nameof(SampleObject.InjectionMethod), 1);

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, builder.BuildPreset());

        Assert.AreEqual(1, sampleObject.MethodValue);
        
        builder.Clear();
        
        builder.InvokeMethod(nameof(SampleObject.InjectionMethod), 2);
        
        Injector.Inject(sampleObject, builder.BuildPreset());
        
        Assert.AreEqual(2, sampleObject.MethodValue);
    }
}
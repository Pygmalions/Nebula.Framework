using Nebula.Resource;
using Nebula.Resource.Identifiers;
using Nebula.Resource.Sources;
using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class ContainerInjectorTest
{
    [Test]
    public void InjectField()
    {
        var container = new Container();
        var source = new CacheSource();
        container.AddSource(source);
        
        source.SetCache<int>(Scope.Program, new NameIdentifier("SampleField"), 2);
        

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, container);
        
        Assert.AreEqual(2, sampleObject.InjectionField);
    }
    
    [Test]
    public void InjectProperty()
    {
        var container = new Container();
        var source = new CacheSource();
        container.AddSource(source);
        
        source.SetCache<int>(Scope.Program, new NameIdentifier("SampleProperty"), 3);

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, container);
        
        Assert.AreEqual(3, sampleObject.InjectionProperty);
    }
    
    [Test]
    public void InjectMethod()
    {
        var container = new Container();
        var source = new CacheSource();
        container.AddSource(source);
        
        source.SetCache<int>(Scope.Program, new NameIdentifier("SampleMethodValue"), 4);

        var sampleObject = new SampleObject();
        Injector.Inject(sampleObject, container);

        Assert.AreEqual(4, sampleObject.MethodValue);
    }
    
    [Test]
    public void InjectConstrucktor()
    {
        var container = new Container();
        var source = new CacheSource();
        container.AddSource(source);
        
        source.SetCache<int>(Scope.Program, new NameIdentifier("SampleConstructorValue"), 5);

        var sampleObject = Injector.Inject<SampleObject>(container);

        Assert.AreEqual(5, sampleObject?.ConstructorValue);
    }
}
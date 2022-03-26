using NUnit.Framework;

namespace Nebula.Injecting.Test;

public class SourceTest
{
    [Test]
    public void SourceFunctionTest()
    {
        var container = new Container();
        container.AddSource(new SampleSource());

        var number = container.Get<int>("SampleNumber");
        
        Assert.AreEqual(1, number);
    }
}
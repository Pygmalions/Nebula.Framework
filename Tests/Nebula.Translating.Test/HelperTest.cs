using NUnit.Framework;

namespace Nebula.Translating.Test;

public class HelperTest
{
    [Test]
    public void ExtensiveMethodsTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator<int, string>(new SampleTranslator());
        var result = translators.Translate<string>(3);
        Assert.NotNull(result);
        Assert.AreEqual("3", result);
    }

    [Test]
    public void SingletonTest()
    {
        var result = Translators.Translate<string>(3);
        Assert.NotNull(result);
        Assert.AreEqual("3", result);
    }
}
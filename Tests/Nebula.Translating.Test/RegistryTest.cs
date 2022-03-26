using NUnit.Framework;

namespace Nebula.Translating.Test;

public class Tests
{
    [Test]
    public void BasicSearchingTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator(typeof(int), typeof(string), new SampleTranslator());
        var translation = translators.Translate(3, typeof(string));
        var result = translation as string;
        Assert.NotNull(result);
        Assert.AreEqual("3", result);
    }

    [Test]
    public void ProtocolSearchingTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator(typeof(int), typeof(string), new SampleTranslator(), "I2S");
        var translation = translators.Translate(3, typeof(string), "I2S");
        var result = translation as string;
        Assert.NotNull(result);
        Assert.AreEqual("3", result);
    }
    
    [Test]
    public void ProtocolVagueSearchingTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator(typeof(int), typeof(string), new SampleTranslator(), "I2S");
        var translation = translators.Translate(3, typeof(string));
        var result = translation as string;
        Assert.NotNull(result);
        Assert.AreEqual("3", result);
    }
    
    [Test]
    public void NonTranslatorTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator(typeof(int), typeof(string), 2, "I2S");
        Assert.IsEmpty(translators.GetProtocols(typeof(int), typeof(string)));
    }
    
    [Test]
    public void InvalidTranslatorTest()
    {
        var translators = new TranslatorRegistry();
        translators.RegisterTranslator(typeof(int), typeof(double), new SampleTranslator());
        Assert.IsEmpty(translators.GetProtocols(typeof(int), typeof(double)));
    }
}
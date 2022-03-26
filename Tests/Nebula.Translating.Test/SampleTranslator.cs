using Nebula.Extending;

// Only necessary in unit tests, because the entrance assembly may not be the test runner rather than this assembly.
[assembly: PluginAssembly]

namespace Nebula.Translating.Test;

[Translator]
public class SampleTranslator : ITranslator<int, string>
{
    public string Translate(int original)
    {
        return original.ToString();
    }
}
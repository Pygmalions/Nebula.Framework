namespace Nebula.Translating;

/// <summary>
/// Translator classes marked with this attributes will be auto registered into <see cref="Translators"/>.
/// Multiple TranslatorAttribute will make the translator registered in multiple translating directions.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TranslatorAttribute : Attribute
{
    /// <summary>
    /// Original type.
    /// </summary>
    public Type? FromType { get; }
    
    /// <summary>
    /// Destination type.
    /// </summary>
    public Type? ToType { get; }
    
    /// <summary>
    /// Protocol that this translator supports.
    /// </summary>
    public string Protocol { get; }

    /// <summary>
    /// Mark this translator to be auto discovered and registered in <see cref="Translators"/>.
    /// </summary>
    /// <param name="from">Original type.</param>
    /// <param name="to">Destination type.</param>
    /// <param name="protocol">Protocol of this translator.</param>
    public TranslatorAttribute(Type from, Type to, string protocol = "")
    {
        FromType = from;
        ToType = to;
        Protocol = protocol;
    }
    
    /// <summary>
    /// Mark this translator to be auto discovered and use it declaring types as its original and destination
    /// types to register.
    /// </summary>
    /// <param name="protocol">Protocol of this translator.</param>
    public TranslatorAttribute(string protocol = "")
    {
        Protocol = protocol;
    }
}
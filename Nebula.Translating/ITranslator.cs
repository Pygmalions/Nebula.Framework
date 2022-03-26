namespace Nebula.Translating;

public interface ITranslator<in TFrom, out TTo>
{
    TTo? Translate(TFrom original);
}
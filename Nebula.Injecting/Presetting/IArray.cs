namespace Nebula.Injecting.Presetting;

public interface IArray<out TElement>
{
    TElement[] Translate();
}
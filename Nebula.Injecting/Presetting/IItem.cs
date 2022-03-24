namespace Nebula.Injecting.Presetting;

public interface IItem<out TContent>
{
    TContent Translate();
}
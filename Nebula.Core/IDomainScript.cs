namespace Nebula.Core;

public interface IDomainScript
{
    void Execute(string trigger);
}
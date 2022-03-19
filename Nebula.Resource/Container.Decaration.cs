namespace Nebula.Resource;

public partial class Container
{
    private record Declaration(Type Category, IIdentifier Identifier, Scope Scope);
}
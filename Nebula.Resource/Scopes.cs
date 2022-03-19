namespace Nebula.Resource;

public enum Scope : byte
{
    Instance = 1 << 0,
    Program = 1 << 1,
    Host = 1 << 2,
    Workspace = 1 << 3,
}

[Flags]
public enum Scopes : byte
{
    Instance = Scope.Instance,
    Program = Scope.Program,
    Host = Scope.Host,
    Workspace = Scope.Workspace,
    Any = Instance | Program | Host | Workspace
}
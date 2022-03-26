namespace Nebula.Reporting;

public static class GlobalReporters
{
    public static readonly Reporter Error = new();

    public static readonly Reporter Warning = new();

    public static readonly Reporter Information = new();
}
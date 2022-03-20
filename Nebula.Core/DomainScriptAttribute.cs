namespace Nebula.Core;

[AttributeUsage(AttributeTargets.Class)]
public class DomainScriptAttribute : Attribute
{
    /// <summary>
    /// Event to trigger this script.
    /// e.g., Initialize, Launch, Finish.
    /// </summary>
    public string[] Triggers { get; }

    public DomainScriptAttribute(params string[] triggers)
    {
        Triggers = triggers;
    }
}
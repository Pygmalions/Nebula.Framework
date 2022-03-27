using System;

namespace Nebula.Injecting.Test;

public class SampleSource : Source
{
    protected override void OnInstall()
    {
        Declare<int>("SampleNumber")?.SetSingleton(true);
    }
    
    protected override object? Get(Declaration declaration, Type type, string name = "")
    {
        return name == "SampleNumber" ? 1 : null;
    }
}
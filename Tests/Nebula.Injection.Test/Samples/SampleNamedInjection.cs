namespace Nebula.Injection.Test.Samples;

public class SampleNamedInjection
{
    [Injection] 
    public ISampleComponent? UnnamedComponent;
    
    [NamedInjection("ComponentA")] 
    public ISampleComponent? ComponentA;

    [NamedInjection("ComponentB")] 
    public ISampleComponent? ComponentB;
}
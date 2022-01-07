namespace Nebula.Injection.Test.Samples;

public class SampleTaggedInjection
{
    [Injection] 
    public ISampleComponent? UntaggedComponent;
    
    [TaggedInjection("Component", "A")]
    public ISampleComponent? ComponentA;

    [TaggedInjection("Component", "B")] 
    public ISampleComponent? ComponentB;
}
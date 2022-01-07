namespace Nebula.Injection.Test.Samples;

public class SampleConstructorInjection
{
    public readonly string ComponentName;
    
    public SampleConstructorInjection(ISampleComponent component)
    {
        ComponentName = component.GetComponentName();
    }
}
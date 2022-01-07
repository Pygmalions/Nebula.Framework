namespace Nebula.Injection.Test.Samples;

public class SampleMemberInjection
{
    [Injection]
    public ISampleComponent? FieldComponent;

    [Injection]
    public ISampleComponent? PropertyComponent { get; set; }
    
    public ISampleComponent? MethodComponent { get; private set; }

    [Injection]
    public void InjectComponent(ISampleComponent component)
    {
        MethodComponent = component;
    }
}
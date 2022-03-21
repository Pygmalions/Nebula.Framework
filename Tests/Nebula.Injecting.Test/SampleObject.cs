namespace Nebula.Injecting.Test;

public class SampleObject
{
    [Injection(Name = "SampleField")]
    public int InjectionField;
    
    [Injection(Name = "SampleProperty")]
    public int InjectionProperty { get; set; }

    public int MethodValue;

    
    [Injection]
    public void InjectionMethod([Injection(Name = "SampleMethodValue")] int value)
    {
        MethodValue = value;
    }

    public SampleObject()
    {
        InjectionField = -1;
        InjectionProperty = -1;
        InjectionMethod(-1);
    }

    public readonly int ConstructorValue;
    
    [Injection]
    public SampleObject([Injection(Name = "SampleConstructorValue")] int value)
    {
        InjectionField = -1;
        InjectionProperty = -1;
        InjectionMethod(-1);
        ConstructorValue = value;
    }
}
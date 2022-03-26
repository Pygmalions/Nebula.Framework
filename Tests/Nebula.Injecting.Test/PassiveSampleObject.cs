namespace Nebula.Injecting.Test;

public class PassiveSampleObject
{
    [Injection("SampleField")]
    public int SampleField;
    
    [Injection("SampleProperty")]
    public int SampleProperty { get; set; }

    public int MethodValue;
    
    [Injection]
    public void SampleMethod([Injection("SampleMethod")] int value)
    {
        MethodValue = value;
    }

    public int ConstructorValue;
    
    public PassiveSampleObject()
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = -1;
    }
    
    [Injection]
    public PassiveSampleObject([Injection("SampleConstructor")] int value)
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = value;
    }
}
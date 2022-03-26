namespace Nebula.Injecting.Test;

public class SampleObject
{
    public int SampleField;
    
    public int SampleProperty { get; set; }

    public int MethodValue;
    
    public void SampleMethod(int value)
    {
        MethodValue = value;
    }

    public int ConstructorValue;
    
    public SampleObject()
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = -1;
    }
    
    public SampleObject(int value)
    {
        SampleField = -1;
        SampleProperty = -1;
        MethodValue = -1;
        ConstructorValue = value;
    }
}
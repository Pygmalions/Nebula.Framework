namespace Nebula.Injecting.Test;

public class SampleObject
{
    public int InjectionField;
    
    public int InjectionProperty { get; set; }

    public int MethodValue;

    public void InjectionMethod(int value)
    {
        MethodValue = value;
    }

    public SampleObject()
    {
        InjectionField = -1;
        InjectionProperty = -1;
        InjectionMethod(-1);
    }
}
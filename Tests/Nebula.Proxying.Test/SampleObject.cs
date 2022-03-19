using Nebula.Proxying;

namespace Nebula.Proxying.Test;

public class SampleObject
{
    public int PropertyNumber;

    public SampleObject()
    {
        PropertyNumber = -1;
    }

    public SampleObject(int propertyNumber)
    {
        PropertyNumber = propertyNumber;
    }

    [Proxy]
    public virtual int Number
    {
        get => PropertyNumber;
        set => PropertyNumber = value;
    }

    [Proxy]
    public virtual int AddNumber(int increment)
    {
        PropertyNumber += increment;
        return PropertyNumber;
    }
}
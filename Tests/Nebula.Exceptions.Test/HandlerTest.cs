using NUnit.Framework;

namespace Nebula.Exceptions.Test;

public class Tests
{
    [Test]
    public void ContextFunction()
    {
        var handler = new SampleHandler();
        ErrorCenter.RegisterHandler(typeof(UserError), handler);
        
        Assert.DoesNotThrow(() =>
        {
            ErrorCenter.Report<UserError>(Importance.Ignoring, "This exception will not be thrown.");
        });

        Assert.Throws<UserError>(() =>
        {
            ErrorCenter.Report<UserError>(Importance.Error, "This exception will be thrown.");
        });
        
        Assert.Throws<RuntimeError>(() =>
        {
            ErrorCenter.Report<RuntimeError>(Importance.Error, "This exception will be thrown.");
        });
    }
}
using NUnit.Framework;

namespace Nebula.Reporting.Test;

public class ReportTest
{
    [Test]
    public void EventTest()
    {
        var received = false;
        GlobalReporters.Warning.Received += document =>
        {
            if (document.Title == "Test report.")
                received = true;
        };
        Report.Warning("Test report.", "No description.").Notify(false);
        Assert.True(received);
    }
    
    [Test]
    public void ErrorTest()
    {
        var received = false;
        GlobalReporters.Error.Received += document =>
        {
            if (document.Title == "Test report.")
                received = true;
        };
        Assert.Throws<ReportException>(() =>
            throw Report.Error("Test report.", "No description.").Notify().AsException());
        Assert.True(received);
    }
}
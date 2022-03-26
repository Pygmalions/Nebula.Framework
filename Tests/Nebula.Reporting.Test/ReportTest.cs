using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
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
        Report.Warning("Test report.", "No description.").GloballyNotify(false);
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
        Assert.Throws<ReportExceptionWrapper>(() =>
            throw Report.Error("Test report.", "No description.").GloballyNotify().AsException());
        Assert.True(received);
    }
}
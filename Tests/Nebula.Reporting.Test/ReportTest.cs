using System;
using NUnit.Framework;

namespace Nebula.Reporting.Test;

public class ReportTest
{
    [Test]
    public void EventTest()
    {
        var reported = false;
        var message = "";

        var reporter = new Reporter<string>();
        reporter.Reported += (text, guid) =>
        {
            reported = true;
            message = text;
        };

        reporter.Report("A message.");
        Assert.AreEqual("A message.", message);
        Assert.True(reported);
    }
    
    [Test]
    public void ErrorTest()
    {
        var reported = false;
        Report.ErrorReporter.Reported += (error, id) =>
        {
            reported = true;
        };
        Assert.Throws<Exception>(() => Report.Error(new Exception()));
        Assert.True(reported);
    }
}
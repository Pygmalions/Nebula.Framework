using System;
using NUnit.Framework;

namespace Nebula.Reporting.Test;

public class ActionReportTest
{
    [Test]
    public void FinishTest()
    {
        var reported = false;
        GlobalReporters.Error.Received += _ => reported = true;

        Assert.Throws<ReportException>(() => ActionReport.BeginAction()
            .DoAction(() => throw new Exception())
            .FinishAction());
        
        Assert.True(reported);
    }
    
    [Test]
    public void CaptureTest()
    {
        var reported = false;
        GlobalReporters.Error.Received += _ => reported = true;

        Assert.DoesNotThrow(() => ActionReport.BeginAction()
            .DoAction(() => throw new Exception())
            .OnFailed(report => report.Notify(false)));
        
        Assert.True(reported);
    }
}
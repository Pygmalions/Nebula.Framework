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

        Assert.Throws<ReportExceptionWrapper>(() => ActionReport.Begin()
            .DoAction(() => throw new Exception())
            .Finish());
        
        Assert.True(reported);
    }
    
    [Test]
    public void CaptureTest()
    {
        var reported = false;
        GlobalReporters.Error.Received += _ => reported = true;

        Assert.DoesNotThrow(() => ActionReport.Begin()
            .DoAction(() => throw new Exception())
            .Failed?.GloballyNotify(false));
        
        Assert.True(reported);
    }
}
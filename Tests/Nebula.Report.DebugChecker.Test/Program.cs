using Nebula.Reporting;

// An exception will be thrown in Debug configuration.
Report.Warning("Warning Occured", "Should be an exception in debug mode..").Handle();

Console.WriteLine("In Release Configuration.");
using Nebula.Pooling;

namespace Nebula.Reporting;

/// <summary>
/// An action report contains a series of actions.
/// A exception thrown by them can be captured and rethrow, and the execution of them will be interrupted.
/// Action report use a global static instance pool, so it can only be gotten from
/// <see cref="Begin"/> method, and use <see cref="Finish"/> method to handle the captured exception or
/// return it to the instance pool.
/// </summary>
public class ActionReport : Report
{
    private static readonly Lazy<Pool<ActionReport>> ActionReportPool = new(()=> new Pool<ActionReport>(
        () => new ActionReport(), 
        report =>
        {
            report.Reset();
            return true;
        }){MaxCount = 10});

    public static ActionReport Begin()
    {
        return ActionReportPool.Value.Acquire();
    }
    
    /// <summary>
    /// Attention, constructor of action report do not require to set the title and description,
    /// set them manually after the <see cref="Failed"/> check.
    /// An action report can only be gotten from <see cref="Report.StartAction"/> method.
    /// </summary>
    internal ActionReport()
        : base("Action Failed", null, null, Importance.Error)
    {}

    /// <summary>
    /// Reset method for object pool.
    /// </summary>
    private void Reset()
    {
        Title = "Action Failed";
        Description = "";
        Owner = null;
        Level = Importance.Error;
        Attachments.Clear();
        ActionException = null;
    }
    
    /// <summary>
    /// The captured exception thrown by the actions.
    /// </summary>
    public Exception? ActionException { get; private set; }

    /// <summary>
    /// Returns this action report if an exception has been thrown and captured yet;
    /// otherwise null.
    /// </summary>
    public ActionReport? Failed => ActionException != null ? this : null;

    /// <summary>
    /// Returns this action report if no exception has been thrown and captured yet;
    /// otherwise null.
    /// </summary>
    public ActionReport? Succeeded => ActionException == null ? this : null;

    /// <summary>
    /// Do an action. < br/>
    /// If this action throws an exception,
    /// then the exception will be captured and can be rethrow by <see cref="Rethrow"/>;
    /// following DoAction(...) be skipped. < br/>
    /// That means, this method will do nothing if an exception has already been captured.
    /// </summary>
    /// <param name="action">Action to perform.</param>
    /// <returns>This report.</returns>
    public ActionReport DoAction(Action action)
    {
        if (ActionException != null)
            return this;
        try
        {
            action();
        }
        catch (Exception exception)
        {
            ActionException = exception;
        }

        return this;
    }

    /// <summary>
    /// Finish the process of these actions.
    /// If a exception is thrown, the this method will notify the global reporters if notifiable is true,
    /// and then it will rethrow the captured exception. <br />
    /// <br />
    /// This method will store this report into the global action reports pool.
    /// </summary>
    /// <param name="notifiable">Decides whether this report should be globally notified.</param>
    /// <exception cref="ReportExceptionWrapper">
    /// The wrapper of this report, and it will be thrown if any action throws a exception;
    /// the exception will be stored as inner exception.
    /// </exception>
    public void Finish(bool notifiable = true)
    {
        if (ActionException == null)
        {
            ActionReportPool.Value.Store(this);
            return;
        }
        
        if (notifiable)
            GloballyNotify(false);
        throw AsException(ActionException);
    }
}
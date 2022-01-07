using System.Collections.Concurrent;

namespace Nebula.Reporting;

/// <summary>
/// A reporter group is a reporter itself, but will redirect the message and level to all reporters in this group.
/// </summary>
/// <typeparam name="TMessage">Type of message to report.</typeparam>
/// <typeparam name="TLevel">Type of level.</typeparam>
public class ReporterGroup<TMessage, TLevel> : IReporter<TMessage, TLevel> where TLevel : Level
{
    private readonly ReaderWriterLockSlim _reportersLock = new();
    private readonly HashSet<IReporter<TMessage, TLevel>> _reporters = new();

    /// <summary>
    /// Add a reporter to this group.
    /// </summary>
    /// <param name="reporter">Reporter to add.</param>
    public void Add(IReporter<TMessage, TLevel> reporter)
    {
        _reportersLock.EnterWriteLock();
        _reporters.Add(reporter);
        _reportersLock.ExitWriteLock();
    }

    /// <summary>
    /// Remove a reporter from this group.
    /// </summary>
    /// <param name="reporter">Reporter to remove.</param>
    public void Remove(IReporter<TMessage, TLevel> reporter)
    {
        _reportersLock.EnterWriteLock();
        _reporters.Remove(reporter);
        _reportersLock.ExitWriteLock();
    }

    /// <summary>
    /// Remove all reporters in this group.
    /// </summary>
    public void Clear()
    {
        _reportersLock.EnterWriteLock();
        _reporters.Clear();
        _reportersLock.ExitWriteLock();
    }

    /// <summary>
    /// Check whether a reporter is in this group or not.
    /// </summary>
    /// <param name="reporter">Reporter to check.</param>
    /// <returns>True if this group contains this reporter, otherwise false.</returns>
    public bool Contains(IReporter<TMessage, TLevel> reporter)
    {
        _reportersLock.EnterReadLock();
        var contained = _reporters.Contains(reporter);
        _reportersLock.ExitReadLock();
        return contained;
    }
    
    /// <summary>
    /// Broadcast a message to all the reporters in this group.
    /// </summary>
    /// <param name="message">Message to report.</param>
    /// <param name="level">Level of this report.</param>
    public void Report(TMessage message, TLevel level)
    {
        _reportersLock.EnterReadLock();
        Parallel.ForEach(_reporters, (reporter) =>
        {
            reporter.Report(message, level);
        });
        _reportersLock.ExitReadLock();
    }
}
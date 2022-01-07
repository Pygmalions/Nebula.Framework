namespace Nebula.Reporting;

/// <summary>
/// This is the interface for reporters which can report values or instances of a certain type.
/// </summary>
/// <typeparam name="TMessage">Type of message to report.</typeparam>
/// <typeparam name="TLevel">Type of level.</typeparam>
public interface IReporter<in TMessage, in TLevel> where TLevel : Level
{
    /// <summary>
    /// Report an instance of <typeparamref name="TMessage"/> with the given level.
    /// </summary>
    /// <param name="message">Instance of <typeparamref name="TMessage"/> to report.</param>
    /// <param name="level">Level of this report.</param>
    void Report(TMessage message, TLevel level);
}
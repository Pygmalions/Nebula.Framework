# Nebula Reporting

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides a central interface for error, warning, message
reporting and handling.

## How to Use

In most common scenarios, just use the static class **Report**.
```C#
// Report an error.
throw Report.Error(yourException);
// Report an warning.
Report.Warning(yourException);
// Report an message.
Report.Message("This is a message.");
// Report an debug message.
Report.Debug("This message will only appear in debug mode.");
```

Subscribe the **Reported** event to get notified when an error is reported.
```C#
Report.ErrorReporter.Reported += yourAction;
```

Also, there are **MessageReporter** to handle message and debug message.

## Concepts

### Report Types

There are three types of reports: Error, Warning, Message and Debug.
- **Error**: An error is an Exception, and will be thrown after triggered the Reported event.
- **Warning**: An warning is also an Exception, but will be thrown only in Debug mode; 
in release mode, it will be converted into a text and reported into the MessageReporter.
- **Message**: Plain text.
- **Debug**: The message will be reported only in Debug mode.

#### Error or Warning?

It is quite easy to separate these two types: 

- If an exception is caused by the users' wrong use (invalid parameter, etc), 
then we recommend you to make it an **Warning**;
- If the exception is caused by reason not related to user (Internet disconnection, system failure, etc),
then it is better to make it an **Error**.

### Associated GUID

All reporting methods in **Report** has an optional parameter *id*, 
its type is Guid.
You can specify it as the GUID of the transaction, or the object which reports this message;
the error and message handler may categorize this stuff according to the GUID,
and it also makes it easier to trace the information of a transaction or an action on
a distributing system.

## Remarks

This library is under rapid development, thus its API may be very unstable, 
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.
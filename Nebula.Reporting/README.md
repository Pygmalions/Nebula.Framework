# Nebula Reporting

*Fundamental library of [Pygmalions](https://github.com/Pygmalions)' [Nebula Framework](https://github.com/Pygmalions/Nebula.Framework).*

This library provides a modular document-style log or error reporting and handling mechanism.
    
## How to Use

In most common scenarios, just use the static class **Report**.
```C#
// Report an warninig.
Report.Warning("A Custom Warninig", "Here is the description", objectWhichReportsThis)
    .AttachDetails("An details", relatedObject)
    .GloballyNotify().InDebug?.Throw();
```

In this case, *objectWhichReportsThis* is the object which occurs this log or error;
**AttachDetails** is an extensive method which allow you to attach related objects with a specific name.
There are other extensive methods, such as **AttachGuid**.

Method **GloballyNotify** will send this report to the global reports in static class **GlobalReporters**;
**InDebug** will returns null in Release mode; **Throw** will wrap this report into an exception and throw it.

The last three methods are really common, so we provide the extensive method **Handle** to notify the report
and decide whether to throw an excpetion or not based on the level of the report.

```c#
// Report an warninig.
Report.Warning("A Custom Warninig", "Here is the description", objectWhichReportsThis)
    .AttachDetails("An details", relatedObject)
    .Handle();
```

Subscribe the **Received** event to get notified when an error is reported.
```C#
GlobalReporters.Error.Received += yourAction;
```

## Concepts

### Report Types

There are three types of reports: Error, Warning, and Message.
- **Error**: What happened will stop the program from working normally.
- **Warning**: The incident is tolerable, but still need to pay attention to it.
- **Message**: Something normal but should be logged.

#### Error or Warning?

It is quite easy to separate these two types: 

- If an exception is caused by the users' wrong use (invalid parameter, etc), 
then we recommend you to make it an **Warning**;
- If the exception is caused by reason not related to user (Internet disconnection, system failure, etc),
then it is better to make it an **Error**.

### Attachments

Attachments allow you to attach optional information to a report to make it more detailed.
The dictionary member **Attachments** contains all kinds of attachments with a unique key name.

To support a kind of attachments, we suggest you provide a extensive method such as **AttachGuid**
to help user to add a instance of this kind of attachments, and provide a extensive method such as **GetGuidAttachments**
to get all instances of this kind of attachments.

This library provides built-in support for *Details* and *Guid* attachments.

## Remarks

This library is under rapid development, thus its API may be very unstable, 
**DO NOT** use it in the production environment,
until its version reaches 1.0.0.
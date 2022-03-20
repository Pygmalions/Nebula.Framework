namespace Nebula.Exceptions.Test;

public class SampleHandler : IErrorHandler
{
    public void Handle(ErrorContext context)
    {
        if (context.Level == Importance.Ignoring || context.Level == Importance.Information)
            context.Continue();
    }
}
namespace Nebula.Exceptions;

public interface IErrorHandler
{
    void Handle(ErrorContext context);
}
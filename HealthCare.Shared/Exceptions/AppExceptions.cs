namespace HealthCare.Shared.Exceptions;

public static class AppExceptions
{
    public class NotFoundException(string message) : Exception(message);
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, List<string> errors) : base(message)
        {
            Errors = errors;
        }
        public List<string>? Errors { get; }
    }

    public class UnauthorizedException(string message = "No autorizado.") : Exception(message);

    public class ForbiddenException(string message = "Acceso denegado.") : Exception(message);

    public class ConflictException(string message) : Exception(message);
    
}
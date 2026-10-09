namespace GameBacklog.Api.Exceptions {
    public abstract class ApiException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }

    public class NotFoundException(string message) : ApiException(StatusCodes.Status404NotFound, message);

    public class ConflictException(string message) : ApiException(StatusCodes.Status409Conflict, message);

    public class UnauthorizedException(string message) : ApiException(StatusCodes.Status401Unauthorized, message);
}

using Microsoft.AspNetCore.Diagnostics;

namespace AuthService.ExceptionHandlers;

public class ArgumentExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ArgumentException argumentException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(argumentException.Message, cancellationToken);

            return true;
        }

        return false;
    }
}

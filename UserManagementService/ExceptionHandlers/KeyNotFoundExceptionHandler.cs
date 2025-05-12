using Microsoft.AspNetCore.Diagnostics;

namespace UserManagementService.ExceptionHandlers;

public class KeyNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is KeyNotFoundException keyNotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync(keyNotFoundException.Message, cancellationToken);

            return true;
        }

        return false;
    }
}

using Microsoft.AspNetCore.Diagnostics;

namespace UserManagementService.ExceptionHandlers;

public class InvalidOperationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is InvalidOperationException invalidOperationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(invalidOperationException.Message, cancellationToken);

            return true;
        }

        return false;
    }
}

namespace CryptoLab.Api.Filters;

public class SessionValidationFilter : IEndpointFilter
{
    private readonly SessionStorage _sessions;

    public SessionValidationFilter(SessionStorage sessions) => _sessions = sessions;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;

        if (!request.Headers.TryGetValue("x-session-id", out var header)
            || string.IsNullOrWhiteSpace(header))
            return Results.BadRequest("x-session-id is required");

        var session = _sessions.GetSession(header.ToString());
        if (session is null)
            return Results.NotFound("Session not found");

        if (session.ExpiredAt <= DateTime.UtcNow)
        {
            session.IsValid = false;
            return Results.StatusCode(440);
        }

        return await next(context);
    }
}
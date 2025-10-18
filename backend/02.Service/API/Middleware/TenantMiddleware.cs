using Microsoft.AspNetCore.Http;

namespace API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("tenant", out var tenant))
        {
            context.Items["Tenant"] = tenant.ToString();
        }
        await _next(context);
    }
}

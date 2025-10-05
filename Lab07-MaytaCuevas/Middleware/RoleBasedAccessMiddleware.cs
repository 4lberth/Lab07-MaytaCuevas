using System.Security.Claims;

namespace Lab07_MaytaCuevas.Middleware;

public class RoleBasedAccessMiddleware
{
    private readonly RequestDelegate _next;

    public RoleBasedAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var publicPaths = new[]
        {
            "/api/auth/login",
            "/swagger",
            "/swagger/index.html",
            "/swagger/v1/swagger.json"
        };

        if (publicPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }
        
        var userRole = context.User?.Claims?.FirstOrDefault(c => 
            c.Type == "role" || 
            c.Type == ClaimTypes.Role ||
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        )?.Value;

        if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Acceso denegado. El rol del usuario no tiene permisos.");
            return;
        }

        await _next(context);
    }
}
namespace UserManagementAPI.Middleware
{
    public class TokenAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private const string TokenHeaderName = "Authorization";
        private const string ExpectedToken = "Bearer secret-token-123";

        public TokenAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(TokenHeaderName, out var extractedToken) || 
                extractedToken != ExpectedToken)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid or missing token." });
                return;
            }

            await _next(context);
        }
    }
}
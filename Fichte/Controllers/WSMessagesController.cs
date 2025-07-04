using Fichte.Models;
using Fichte.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fichte.Controllers
{
    [Route("api/[controller]")]
    public class WSMessagesController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {
        [HttpGet]
        public async Task Get(string token)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var principal = JwtTokenProvider.ValidateTokenAndGetPrincipal(token, _configuration["JWT:SecureKey"]!);

            if (principal == null || !principal.Identity!.IsAuthenticated)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            if (!principal.IsInRole("User"))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            HttpContext.User = principal;

            var userId = principal.FindFirstValue(ClaimTypes.Name);
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await WebSocketProvider.ReceiveMessages(_context, webSocket, userId!);
        }
    }
}
using Fichte.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fichte.Controllers
{
    [ApiController]
    public class BaseController(IConfiguration configuration, DatabaseContext context) : ControllerBase
    {
        protected readonly IConfiguration _configuration = configuration;
        protected readonly DatabaseContext _context = context;

        protected int GetUserId()
        {
            var claim = (HttpContext.User.Identity as ClaimsIdentity)!.FindFirst(ClaimTypes.Name);
            return claim == null ? throw new UnauthorizedAccessException("User ID claim is missing") : int.Parse(claim.Value);
        }
    }
}

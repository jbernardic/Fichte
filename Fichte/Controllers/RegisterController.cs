using Microsoft.AspNetCore.Mvc;
using Fichte.Models;
using Fichte.Dtos;
using BCrypt.Net;
using System.Linq;

namespace Fichte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {
        [HttpPost]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully.");
        }
    }
}

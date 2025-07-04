using Fichte.Dtos;
using Fichte.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Fichte.Controllers
{
    [Route("api/[controller]")]
    public class UsersController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {
        [Authorize(Roles = "User")]
        [HttpGet("me")]
        public ActionResult<UserDto> GetCurrentUser()
        {
            var userId = GetUserId();
            var user = _context.Users.First(x => x.IDUser == userId);
            return Ok(new UserDto
            {
                Id = user.IDUser,
                Username = user.Username,
                Email = user.Email,
                IsOnline = user.IsOnline,
                LastSeen = user.LastSeen,
                CreatedAt = user.CreatedAt
            });
        }

        [Authorize(Roles = "User")]
        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUser(string username)
        {
            var user = await _context.Users.Where(user => user.Username == username).FirstOrDefaultAsync();
            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.IDUser,
                Username = user.Username,
                Email = user.Email,
                IsOnline = user.IsOnline,
                LastSeen = user.LastSeen,
                CreatedAt = user.CreatedAt
            });
        }

        [Authorize(Roles = "User")]
        [HttpGet("online")]
        public async Task<ActionResult<List<UserDto>>> GetOnlineUsers()
        {
            var onlineUsers = await _context.Users
                .Where(u => u.IsOnline)
                .Select(u => new UserDto
                {
                    Id = u.IDUser,
                    Username = u.Username,
                    Email = u.Email,
                    IsOnline = u.IsOnline,
                    LastSeen = u.LastSeen,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(onlineUsers);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.IDUser,
                    Username = u.Username,
                    Email = u.Email,
                    IsOnline = u.IsOnline,
                    LastSeen = u.LastSeen,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
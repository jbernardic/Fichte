using Fichte.Dtos;
using Fichte.Models;
using Fichte.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Fichte.Controllers
{
    [Route("api/[controller]")]
    public class AuthController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {

        [HttpPost("[action]")]
        public async Task<ActionResult> Register(RegisterDto register)
        {
            if (string.IsNullOrWhiteSpace(register.Username) || string.IsNullOrWhiteSpace(register.Password))
                return BadRequest("Username and password are required");

            if (await _context.Users.AnyAsync(x => x.Username.Equals(register.Username)))
                return BadRequest($"Username {register.Username} already exists");

            var user = new User
            {
                Username = register.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            _context.Add(user);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> RegisterWithEmail(RegisterDto register, string email)
        {
            if (string.IsNullOrWhiteSpace(register.Username) || string.IsNullOrWhiteSpace(register.Password) || string.IsNullOrWhiteSpace(email))
                return BadRequest("Username, password, and email are required");

            if (await _context.Users.AnyAsync(x => x.Username.Equals(register.Username)))
                return BadRequest($"Username {register.Username} already exists");

            if (await _context.Users.AnyAsync(x => x.Email != null && x.Email.Equals(email)))
                return BadRequest($"Email {email} already exists");

            var user = new User
            {
                Username = register.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Email = email,
                IsEmailVerified = false
            };

            _context.Add(user);
            await _context.SaveChangesAsync();
            
            // TODO: Send verification email
            
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginDto login)
        {
            login.Username = login.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username.Equals(login.Username));
            if (user != null && !string.IsNullOrEmpty(user.PasswordHash))
            {
                if(BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                {
                    user.IsOnline = true;
                    user.LastSeen = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    var jwt = JwtTokenProvider.CreateToken(_configuration["JWT:SecureKey"]!, 60*30,
                        user.IDUser.ToString(), "User");
                    return Ok(jwt);
                }
            }

            return BadRequest("Incorrect username or password");
        }

        [HttpPost("[action]")]
        public ActionResult LoginWithOAuth(OAuthLoginDto oauthLogin)
        {
            try
            {
                // TODO: Implement actual OAuth verification with provider APIs
                // string? externalUserId = null;
                // string? email = null;
                // string? username = null;

                if (oauthLogin.Provider.ToLower() == "google")
                {
                    return BadRequest("Google OAuth not fully implemented yet");
                }
                else if (oauthLogin.Provider.ToLower() == "github")
                {
                    return BadRequest("GitHub OAuth not fully implemented yet");
                }
                else
                {
                    return BadRequest("Unsupported OAuth provider");
                }

                /* TODO: This code will be reached when OAuth is fully implemented
                // Find or create user
                User? user = null;
                if (oauthLogin.Provider.ToLower() == "google")
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == externalUserId);
                }
                else if (oauthLogin.Provider.ToLower() == "github")
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.GitHubId == externalUserId);
                }

                if (user == null && !string.IsNullOrEmpty(email))
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null)
                    {
                        // Link OAuth account to existing user
                        if (oauthLogin.Provider.ToLower() == "google")
                            user.GoogleId = externalUserId;
                        else if (oauthLogin.Provider.ToLower() == "github")
                            user.GitHubId = externalUserId;
                    }
                }

                if (user == null)
                {
                    // Create new user
                    user = new User
                    {
                        Username = username ?? $"user_{Guid.NewGuid().ToString()[..8]}",
                        Email = email,
                        IsEmailVerified = true, // OAuth providers verify emails
                        GoogleId = oauthLogin.Provider.ToLower() == "google" ? externalUserId : null,
                        GitHubId = oauthLogin.Provider.ToLower() == "github" ? externalUserId : null
                    };

                    _context.Users.Add(user);
                }

                user.IsOnline = true;
                user.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var jwt = JwtTokenProvider.CreateToken(_configuration["JWT:SecureKey"]!, 60*30,
                    user.IDUser.ToString(), "User");
                return Ok(jwt);
                */
            }
            catch (Exception ex)
            {
                return BadRequest($"OAuth login failed: {ex.Message}");
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Logout(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsOnline = false;
                user.LastSeen = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

    }
}
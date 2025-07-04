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

            if (await _context.Users.AnyAsync(x => x.Username.Equals(register.Username)))
                return BadRequest($"Username {register.Username} already exists");

            if (!string.IsNullOrEmpty(register.Email) && await _context.Users.AnyAsync(x => x.Email != null && x.Email.Equals(register.Email)))
                return BadRequest($"Email {register.Email} already exists");

            var user = new User
            {
                Username = register.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Email = register.Email,
                IsEmailVerified = false
            };

            if (!string.IsNullOrEmpty(register.Email))
            {
                var verificationToken = Guid.NewGuid().ToString();
                user.EmailVerificationToken = verificationToken;
                user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            }

            _context.Add(user);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(register.Email))
            {
                SmtpProvider.SendVerificationEmail(_configuration, register.Email, register.Username, user.EmailVerificationToken!);
            }

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

        [HttpGet("[action]")]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid verification token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            if (user == null)
                return BadRequest("Invalid verification token");

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return BadRequest("Verification token has expired");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            await _context.SaveChangesAsync();

            return Ok("Email verified successfully");
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> ResendVerification()
        {
            int userId;
            try
            {
                userId = GetUserId();
            }
            catch
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (user.IsEmailVerified)
                return BadRequest("Email is already verified");

            if (string.IsNullOrEmpty(user.Email))
                return BadRequest("No email address associated with this account");

            var verificationToken = Guid.NewGuid().ToString();
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync();

            SmtpProvider.SendVerificationEmail(_configuration, user.Email, user.Username, verificationToken);

            return Ok("Verification email sent");
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
using Fichte.Dtos;
using Fichte.Models;
using Fichte.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;

namespace Fichte.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {

        [Authorize(Roles = "User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Message>>> GetUserMessages(int? groupId = null, int? recipientUserId = null, int page = 1, int pageSize = 50)
        {
            var userId = GetUserId();
            
            IQueryable<Message> query = _context.Messages
                .Include(m => m.User)
                .Include(m => m.Group)
                .Include(m => m.RecipientUser)
                .Where(m => !m.IsDeleted);

            if (groupId.HasValue)
            {
                var isUserInGroup = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupID == groupId && gm.UserID == userId);
                
                if (!isUserInGroup)
                    return Forbid("User is not a member of this group");
                
                query = query.Where(m => m.GroupID == groupId);
            }
            else if (recipientUserId.HasValue)
            {
                query = query.Where(m => 
                    (m.UserID == userId && m.RecipientUserID == recipientUserId) ||
                    (m.UserID == recipientUserId && m.RecipientUserID == userId));
            }
            else
            {
                query = query.Where(m => m.UserID == userId || m.RecipientUserID == userId);
            }

            var messages = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult> SendMessage(SendMessageDto sendMessage)
        {
            var userId = GetUserId();
            
            if (sendMessage.GroupID.HasValue && sendMessage.RecipientUserID.HasValue)
                return BadRequest("Message cannot be both group and private");
            
            if (!sendMessage.GroupID.HasValue && !sendMessage.RecipientUserID.HasValue)
                return BadRequest("Message must specify either group or recipient");

            if (sendMessage.GroupID.HasValue)
            {
                var isUserInGroup = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupID == sendMessage.GroupID && gm.UserID == userId);
                
                if (!isUserInGroup)
                    return Forbid("User is not a member of this group");
            }
            
            if (sendMessage.RecipientUserID.HasValue)
            {
                var recipientExists = await _context.Users
                    .AnyAsync(u => u.IDUser == sendMessage.RecipientUserID);
                
                if (!recipientExists)
                    return NotFound("Recipient user not found");
            }

            var message = new Message 
            { 
                Body = sendMessage.Body, 
                UserID = userId,
                GroupID = sendMessage.GroupID,
                RecipientUserID = sendMessage.RecipientUserID
            };
            
            _context.Add(message);
            await _context.SaveChangesAsync();

            var messageWithDetails = await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Group)
                .Include(m => m.RecipientUser)
                .FirstAsync(m => m.IDMessage == message.IDMessage);

            WebSocketProvider.BroadcastMessage(messageWithDetails);

            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult> DeleteMessage(int messageId)
        {
            var userId = GetUserId();
            
            var message = await _context.Messages
                .Include(m => m.Group)
                .ThenInclude(g => g != null ? g.Members : null)
                .FirstOrDefaultAsync(m => m.IDMessage == messageId);

            if (message == null)
                return NotFound("Message not found");

            var canDelete = message.UserID == userId;
            
            if (message.GroupID.HasValue && message.Group != null)
            {
                var isAdmin = message.Group.Members
                    .Any(gm => gm.UserID == userId && gm.IsAdmin);
                canDelete = canDelete || isAdmin;
            }

            if (!canDelete)
                return Forbid("User cannot delete this message");

            message.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Message>>> SearchMessages(string query, int? groupId = null, int page = 1, int pageSize = 20)
        {
            var userId = GetUserId();
            
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query is required");

            IQueryable<Message> messageQuery = _context.Messages
                .Include(m => m.User)
                .Include(m => m.Group)
                .Include(m => m.RecipientUser)
                .Where(m => !m.IsDeleted && m.Body.Contains(query));

            if (groupId.HasValue)
            {
                var isUserInGroup = await _context.GroupMembers
                    .AnyAsync(gm => gm.GroupID == groupId && gm.UserID == userId);
                
                if (!isUserInGroup)
                    return Forbid("User is not a member of this group");
                
                messageQuery = messageQuery.Where(m => m.GroupID == groupId);
            }
            else
            {
                messageQuery = messageQuery.Where(m => 
                    m.UserID == userId || 
                    m.RecipientUserID == userId ||
                    (m.GroupID.HasValue && m.Group!.Members.Any(gm => gm.UserID == userId)));
            }

            var messages = await messageQuery
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages);
        }
    }
}
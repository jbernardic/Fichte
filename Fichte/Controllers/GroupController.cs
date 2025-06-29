using Fichte.Dtos;
using Fichte.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fichte.Controllers
{
    [Route("api/[controller]")]
    public class GroupController(IConfiguration configuration, DatabaseContext context) : BaseController(configuration, context)
    {
        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult<GroupDto>> CreateGroup(CreateGroupDto createGroup)
        {
            var userId = GetUserId();
            
            var group = new Group
            {
                Name = createGroup.Name,
                Description = createGroup.Description,
                CreatedByUserID = userId,
                MaxMembers = createGroup.MaxMembers,
                IsPrivate = createGroup.IsPrivate
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var groupMember = new GroupMember
            {
                GroupID = group.IDGroup,
                UserID = userId,
                IsAdmin = true
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            var groupDto = new GroupDto
            {
                Id = group.IDGroup,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                CreatedByUserID = group.CreatedByUserID,
                MaxMembers = group.MaxMembers,
                IsPrivate = group.IsPrivate,
                MemberCount = 1,
                IsUserMember = true,
                IsUserAdmin = true
            };

            return Ok(groupDto);
        }

        [Authorize(Roles = "User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<GroupDto>>> GetUserGroups()
        {
            var userId = GetUserId();
            
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserID == userId)
                .Include(gm => gm.Group)
                .Select(gm => new GroupDto
                {
                    Id = gm.Group.IDGroup,
                    Name = gm.Group.Name,
                    Description = gm.Group.Description,
                    CreatedAt = gm.Group.CreatedAt,
                    CreatedByUserID = gm.Group.CreatedByUserID,
                    MaxMembers = gm.Group.MaxMembers,
                    IsPrivate = gm.Group.IsPrivate,
                    MemberCount = gm.Group.Members.Count,
                    IsUserMember = true,
                    IsUserAdmin = gm.IsAdmin
                })
                .ToListAsync();

            return Ok(groups);
        }

        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult> JoinGroup(int groupId)
        {
            var userId = GetUserId();
            
            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.IDGroup == groupId);

            if (group == null)
                return NotFound("Group not found");

            if (group.Members.Count >= group.MaxMembers)
                return BadRequest("Group is full");

            var existingMembership = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupID == groupId && gm.UserID == userId);

            if (existingMembership != null)
                return BadRequest("User is already a member of this group");

            var groupMember = new GroupMember
            {
                GroupID = groupId,
                UserID = userId,
                IsAdmin = false
            };

            _context.GroupMembers.Add(groupMember);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult> LeaveGroup(int groupId)
        {
            var userId = GetUserId();
            
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupID == groupId && gm.UserID == userId);

            if (groupMember == null)
                return NotFound("User is not a member of this group");

            _context.GroupMembers.Remove(groupMember);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<UserDto>>> GetGroupMembers(int groupId)
        {
            var userId = GetUserId();
            
            var isUserMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupID == groupId && gm.UserID == userId);

            if (!isUserMember)
                return Forbid("User is not a member of this group");

            var members = await _context.GroupMembers
                .Where(gm => gm.GroupID == groupId)
                .Include(gm => gm.User)
                .Select(gm => new UserDto
                {
                    Id = gm.User.IDUser,
                    Username = gm.User.Username,
                    Email = gm.User.Email,
                    IsOnline = gm.User.IsOnline,
                    LastSeen = gm.User.LastSeen,
                    CreatedAt = gm.User.CreatedAt
                })
                .ToListAsync();

            return Ok(members);
        }
    }
}
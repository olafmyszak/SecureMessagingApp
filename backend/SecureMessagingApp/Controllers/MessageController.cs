using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureMessagingApp.Models;

namespace SecureMessagingApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageController(AppDbContext context) : ControllerBase
{
    [Authorize]
    [HttpGet("history/{recipientId:int}")]
    public async Task<ActionResult<List<Message>>> GetMessageHistory(int recipientId)
    {
        bool userExists = await context.Users.AnyAsync(u => u.Id == recipientId);

        if (!userExists)
        {
            return NotFound($"User id {recipientId} not found");
        }

        int senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

        List<Message> messages = await context.Messages
            .Where(m =>
                (m.SenderId == senderId && m.RecipientId == recipientId)
                        ||
                (m.SenderId == recipientId && m.RecipientId == senderId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return Ok(messages);
    }

    [Authorize]
    [HttpGet("sentTo/{recipientId:int}")]
    public async Task<ActionResult<List<Message>>> GetMessagesSentTo(int recipientId)
    {
        bool userExists = await context.Users.AnyAsync(u => u.Id == recipientId);

        int senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

        if (!userExists)
        {
            return NotFound($"User id {recipientId} not found");
        }

        List<Message> messages =
            await context.Messages
                .Where(m => m.SenderId == senderId && m.RecipientId == recipientId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

        return Ok(messages);
    }

    [Authorize]
    [HttpGet("receivedFrom/{senderId:int}")]
    public async Task<ActionResult<List<Message>>> GetMessagesReceivedFrom(int senderId)
    {
        bool userExists = await context.Users.AnyAsync(u => u.Id == senderId);

        int recipientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty);

        if (!userExists)
        {
            return NotFound($"User id {senderId} not found");
        }

        List<Message> messages =
            await context.Messages
                .Where(m => m.SenderId == senderId && m.RecipientId == recipientId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

        return Ok(messages);
    }
}
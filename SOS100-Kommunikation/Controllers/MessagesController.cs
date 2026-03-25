using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Data;
using SOS100_Kommunikation.Models;

namespace SOS100_Kommunikation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly CommunicationDbContext _context;

    public MessagesController(CommunicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{email}")]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessagesForUser(string email)
    {
        return await _context.Messages
            .Where(m => m.SenderEmail == email || m.ReceiverEmail == email)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessage>> SendMessage(ChatMessage message)
    {
        message.Id = Guid.NewGuid();
        message.SentAt = DateTime.UtcNow;
        message.IsRead = false;

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMessagesForUser), new { email = message.SenderEmail }, message);
    }
}

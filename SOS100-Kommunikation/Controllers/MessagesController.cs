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
        var emailLower = email.ToLower();
        return await _context.Messages
            .Where(m => (m.SenderEmail.ToLower() == emailLower && !m.DeletedBySender) || 
                        (m.ReceiverEmail.ToLower() == emailLower && !m.DeletedByReceiver))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ChatMessage>> SendMessage(ChatMessage message)
    {
        try
        {
            var currentLower = message.SenderEmail.ToLower();
            var contactLower = message.ReceiverEmail.ToLower();
            var subjLower = (message.Subject ?? "utan ämne").ToLower();

            // Återställ historik! Om någon av parterna raderat chatten tidigare, 
            // men en av dem skickar ett nytt meddelande i samma tråd, 
            // ska HELA historiken visas igen. Vi sätter flaggorna till false.
            var history = await _context.Messages
                .Where(m => ((m.SenderEmail.ToLower() == currentLower && m.ReceiverEmail.ToLower() == contactLower) || 
                             (m.SenderEmail.ToLower() == contactLower && m.ReceiverEmail.ToLower() == currentLower)) &&
                            ((m.Subject == null ? "utan ämne" : m.Subject.ToLower()) == subjLower))
                .ToListAsync();

            foreach (var m in history)
            {
                m.DeletedBySender = false;
                m.DeletedByReceiver = false;
            }

            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;
            message.IsRead = false;
            message.DeletedBySender = false;
            message.DeletedByReceiver = false;

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessagesForUser), new { email = message.SenderEmail }, message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Error = "Databasfel i Kommunikations-appen.", 
                Message = ex.InnerException?.Message ?? ex.Message 
            });
        }
    }

    [HttpDelete("thread")]
    public async Task<IActionResult> DeleteThread([FromQuery] string currentUserEmail, [FromQuery] string contactEmail, [FromQuery] string subject)
    {
        var currentLower = currentUserEmail?.ToLower() ?? "";
        var contactLower = contactEmail?.ToLower() ?? "";
        var subjLower = subject?.ToLower() ?? "utan ämne";

        var messages = await _context.Messages
            .Where(m => ((m.SenderEmail.ToLower() == currentLower && m.ReceiverEmail.ToLower() == contactLower) || 
                         (m.SenderEmail.ToLower() == contactLower && m.ReceiverEmail.ToLower() == currentLower)) &&
                        ((m.Subject == null ? "utan ämne" : m.Subject.ToLower()) == subjLower))
            .ToListAsync();

        if (!messages.Any())
            return NotFound("Inga meddelanden hittades för denna tråd.");

        foreach (var m in messages)
        {
            if (m.SenderEmail.ToLower() == currentLower)
                m.DeletedBySender = true;

            if (m.ReceiverEmail.ToLower() == currentLower)
                m.DeletedByReceiver = true;
        }

        await _context.SaveChangesAsync();
        return Ok("Tråden har tagits bort för din profil.");
    }
}

using System.ComponentModel.DataAnnotations;

namespace SOS100_Kommunikation.Models;

public class ChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Grundläggande information (avsändare)
    [Required]
    public string SenderEmail { get; set; } = string.Empty;
    [Required]
    public string SenderFirstName { get; set; } = string.Empty;
    [Required]
    public string SenderLastName { get; set; } = string.Empty;

    // Mottagare
    [Required]
    public string ReceiverEmail { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;

    // Håller koll på om avsändaren raderat meddelandet för sin egen vy
    public bool DeletedBySender { get; set; } = false;

    // Håller koll på om mottagaren raderat meddelandet för sin egen vy
    public bool DeletedByReceiver { get; set; } = false;
}

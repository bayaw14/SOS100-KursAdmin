namespace SOS100_Inloggning.Models;

public class Enrollment
{
    public Guid   Id        { get; set; }
    public Guid   UserId    { get; set; }
    public int    CourseId  { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public User User { get; set; } = null!;
}
namespace SOS100_Inloggning.Models;

public class User
{
    public Guid Id { get; set; }
    public string FirstName     { get; set; } = "";
    public string LastName      { get; set; } = "";
    public string PersonNumber  { get; set; } = "";
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    
    // Relation till Enrollments
    public List<Enrollment> Enrollments { get; set; } = new();

}
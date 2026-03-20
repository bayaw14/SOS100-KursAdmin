namespace SOS100_KursAdmin_MVC.Models;

public class UserDTO
{
    public Guid   Id            { get; set; }
    public string FirstName     { get; set; } = "";
    public string LastName      { get; set; } = "";
    public string PersonNumber  { get; set; } = "";
    public string Email         { get; set; } = "";
    public string Role          { get; set; } = "";
    public string FullName      => $"{FirstName} {LastName}";
}
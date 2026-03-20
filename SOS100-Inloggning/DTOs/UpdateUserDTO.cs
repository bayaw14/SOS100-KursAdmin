namespace SOS100_Inloggning.DTOs;

public class UpdateUserDTO
{
    public string FirstName     { get; set; } = "";
    public string LastName      { get; set; } = "";
    public string PersonNumber  { get; set; } = "";
    public string? Email    { get; set; }
    public string? Password { get; set; }
    public string? Role     { get; set; } 
}
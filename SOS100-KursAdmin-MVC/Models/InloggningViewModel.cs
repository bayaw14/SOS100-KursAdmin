using System.ComponentModel.DataAnnotations;

namespace SOS100_KursAdmin_MVC.Models;

public class InloggningViewModel
{
    [Required(ErrorMessage = "E-post krävs")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Lösenord krävs")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
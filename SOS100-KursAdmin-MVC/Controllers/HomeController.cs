using Microsoft.AspNetCore.Mvc;
using SOS100_KursAdmin_MVC.Models;

namespace SOS100_KursAdmin_MVC.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _config;

    public HomeController(IConfiguration config)
    {
        _config = config;
    }

    // GET /Home
    public IActionResult Index()
    {
        // Skydda sidan – skicka tillbaka till login om ej inloggad
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Inloggning");

        var model = new DashboardViewModel
        {
            UserName = HttpContext.Session.GetString("UserName") ?? "",
            UserRole = HttpContext.Session.GetString("UserRole") ?? ""
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Kommunikation()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Inloggning");

        var backUrl = $"{Request.Scheme}://{Request.Host}";
        var name = HttpContext.Session.GetString("UserName") ?? "Okänd";
        var role = HttpContext.Session.GetString("UserRole") ?? "Student";

        // Omdirigera till den fristående frontend-appen med token, profilinfo och back-url
        var kommunikationUrl = _config["KommunikationAppUrl"] ?? "http://localhost:5097";
        return Redirect($"{kommunikationUrl}/?t={token}&back={backUrl}&name={Uri.EscapeDataString(name)}&role={Uri.EscapeDataString(role)}");
    }
}
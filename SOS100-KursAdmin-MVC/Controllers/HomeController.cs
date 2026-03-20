using Microsoft.AspNetCore.Mvc;
using SOS100_KursAdmin_MVC.Models;

namespace SOS100_KursAdmin_MVC.Controllers;

public class HomeController : Controller
{
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
}
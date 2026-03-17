using Microsoft.AspNetCore.Mvc;
using SOS100_KursAdmin_MVC.Models;
using System.Text;
using System.Text.Json;

namespace SOS100_KursAdmin_MVC.Controllers;

public class InloggningController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public InloggningController(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    // GET /Inloggning
    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new InloggningViewModel());
    }

    // POST /Inloggning
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(InloggningViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_config["ApiBaseUrl"]!);

        var body = JsonSerializer.Serialize(new
        {
            email    = model.Email,
            password = model.Password
        });

        var response = await client.PostAsync(
            "/api/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Fel e-post eller lösenord.");
            return View(model);
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);

        var token = data.GetProperty("token").GetString()!;
        var role  = data.GetProperty("role").GetString()!;
        var name  = data.GetProperty("name").GetString()!;

        // Spara token i session
        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("UserRole",  role);
        HttpContext.Session.SetString("UserName",  name);

        return RedirectToAction("Index", "Home");
    }

    // GET /Inloggning/Logout
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
}
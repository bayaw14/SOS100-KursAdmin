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

    private HttpClient ApiClient()
    {
        var client = _http.CreateClient();
        client.BaseAddress = new Uri(_config["ApiBaseUrl"]!);
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── LOGIN ────────────────────────────────────────────

    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new InloggningViewModel());
    }

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

        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("UserRole",  role);
        HttpContext.Session.SetString("UserName",  name);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }

    // ── CRUD ANVÄNDARE (Admin) ───────────────────────────

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index");

        var role = HttpContext.Session.GetString("UserRole");
        if (role != "Admin")
            return RedirectToAction("Index", "Home");

        var client   = ApiClient();
        var response = await client.GetAsync("/api/auth/users");
        var json     = await response.Content.ReadAsStringAsync();
        var users    = JsonSerializer.Deserialize<List<UserDTO>>(json,
                           new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<UserDTO>();

        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(
        string firstName, string lastName, string personNumber,
        string email, string password, string role)
    {
        var client   = ApiClient();
        var body     = JsonSerializer.Serialize(new
        {
            firstName, lastName, personNumber, email, password, role
        });
        var response = await client.PostAsync("/api/auth/create-user",
            new StringContent(body, Encoding.UTF8, "application/json"));

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? $"{role} skapad!" : "Något gick fel.";

        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUser(
        Guid id, string firstName, string lastName,
        string personNumber, string email, string password, string role)
    {
        var client   = ApiClient();
        var body     = JsonSerializer.Serialize(new
        {
            firstName, lastName, personNumber, email, password, role
        });
        var response = await client.PutAsync($"/api/auth/update-user/{id}",
            new StringContent(body, Encoding.UTF8, "application/json"));

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Användare uppdaterad!" : "Något gick fel.";

        return RedirectToAction("Users");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var client   = ApiClient();
        var response = await client.DeleteAsync($"/api/auth/delete-user/{id}");

        TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
            response.IsSuccessStatusCode ? "Användare borttagen!" : "Något gick fel.";

        return RedirectToAction("Users");
    }
}
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SOS100_KursAdmin_MVC.Models;

namespace SOS100_KursAdmin_MVC.Controllers;

public class CoursesController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public CoursesController(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private HttpClient ApiClient()
    {
        var client = _http.CreateClient();

        var apiUrl = _config["KurserApiUrl"];
        if (string.IsNullOrWhiteSpace(apiUrl))
            throw new Exception("Konfigurationen 'KurserApiUrl' saknas i appsettings.");

        client.BaseAddress = new Uri(apiUrl);
        return client;
    }

    private IActionResult? RequireLogin()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Inloggning");

        return null;
    }

    public async Task<IActionResult> Index()
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        try
        {
            var client = ApiClient();

            var courses = await client.GetFromJsonAsync<List<Course>>("api/Courses")
                          ?? new List<Course>();

            return View(courses);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Kunde inte hämta kurser från API: {ex.Message}";
            return View(new List<Course>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Course course)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!ModelState.IsValid)
            return View(course);

        try
        {
            var client = ApiClient();
            var response = await client.PostAsJsonAsync("api/Courses", course);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kunde inte skapa kurs.");
            return View(course);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Fel vid kontakt med API: {ex.Message}");
            return View(course);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        try
        {
            var client = ApiClient();
            var course = await client.GetFromJsonAsync<Course>($"api/Courses/{id}");

            if (course == null)
                return NotFound();

            return View(course);
        }
        catch (Exception ex)
        {
            return Content($"Fel vid kontakt med API: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!ModelState.IsValid)
            return View(course);

        try
        {
            var client = ApiClient();
            var response = await client.PutAsJsonAsync($"api/Courses/{id}", course);

            if (response.IsSuccessStatusCode)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Kunde inte uppdatera kurs.");
            return View(course);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Fel vid kontakt med API: {ex.Message}");
            return View(course);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        try
        {
            var client = ApiClient();
            await client.DeleteAsync($"api/Courses/{id}");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            return Content($"Fel vid kontakt med API: {ex.Message}");
        }
    }
}
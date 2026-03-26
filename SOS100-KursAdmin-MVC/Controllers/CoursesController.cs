using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SOS100_KursAdmin_MVC.Models;

namespace SOS100_KursAdmin_MVC.Controllers;

public class CoursesController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private const string RegisteredCoursesSessionKey = "RegisteredCourseIds";

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

        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        var apiKey = _config["ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }

        return client;
    }

    private IActionResult? RequireLogin()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Inloggning");

        return null;
    }

    private string? GetUserRole()
    {
        return HttpContext.Session.GetString("UserRole");
    }

    private bool IsAdmin()
    {
        return GetUserRole() == "Admin";
    }

    private bool IsTeacher()
    {
        return GetUserRole() == "Teacher";
    }

    private bool IsStudent()
    {
        return GetUserRole() == "Student";
    }

    private bool CanManageCourses()
    {
        return IsAdmin() || IsTeacher();
    }

    private List<int> GetRegisteredCourseIds()
    {
        var json = HttpContext.Session.GetString(RegisteredCoursesSessionKey);

        if (string.IsNullOrWhiteSpace(json))
            return new List<int>();

        try
        {
            return JsonSerializer.Deserialize<List<int>>(json) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    private void SaveRegisteredCourseIds(List<int> ids)
    {
        var json = JsonSerializer.Serialize(ids);
        HttpContext.Session.SetString(RegisteredCoursesSessionKey, json);
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

            ViewBag.RegisteredCourseIds = GetRegisteredCourseIds();

            return View(courses);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Kunde inte hämta kurser från API: {ex.Message}";
            ViewBag.RegisteredCourseIds = new List<int>();
            return View(new List<Course>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyCourses()
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsStudent())
            return RedirectToAction(nameof(Index));

        try
        {
            var client = ApiClient();

            var allCourses = await client.GetFromJsonAsync<List<Course>>("api/Courses")
                             ?? new List<Course>();

            var registeredIds = GetRegisteredCourseIds();

            var myCourses = allCourses
                .Where(c => registeredIds.Contains(c.Id))
                .ToList();

            return View(myCourses);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Kunde inte hämta dina kurser från API: {ex.Message}";
            return View(new List<Course>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!CanManageCourses())
            return RedirectToAction(nameof(Index));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!CanManageCourses())
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
            return View(course);

        try
        {
            var client = ApiClient();
            var response = await client.PostAsJsonAsync("api/Courses", course);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kurs skapad.";
                return RedirectToAction(nameof(Index));
            }

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

        if (!CanManageCourses())
            return RedirectToAction(nameof(Index));

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
            TempData["Error"] = $"Fel vid kontakt med API: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course course)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!CanManageCourses())
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
            return View(course);

        try
        {
            var client = ApiClient();
            var response = await client.PutAsJsonAsync($"api/Courses/{id}", course);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Kurs uppdaterad.";
                return RedirectToAction(nameof(Index));
            }

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsAdmin())
            return RedirectToAction(nameof(Index));

        try
        {
            var client = ApiClient();
            var response = await client.DeleteAsync($"api/Courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var registeredIds = GetRegisteredCourseIds();
                if (registeredIds.Contains(id))
                {
                    registeredIds.Remove(id);
                    SaveRegisteredCourseIds(registeredIds);
                }

                TempData["Success"] = "Kurs borttagen.";
            }
            else
            {
                TempData["Error"] = "Kunde inte ta bort kurs.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Fel vid kontakt med API: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsStudent())
            return RedirectToAction(nameof(Index));

        var registeredIds = GetRegisteredCourseIds();

        if (!registeredIds.Contains(id))
        {
            registeredIds.Add(id);
            SaveRegisteredCourseIds(registeredIds);
            TempData["Success"] = "Du har anmält dig till kursen.";
        }
        else
        {
            TempData["Error"] = "Du är redan anmäld till kursen.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Unregister(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsStudent())
            return RedirectToAction(nameof(Index));

        var registeredIds = GetRegisteredCourseIds();

        if (registeredIds.Contains(id))
        {
            registeredIds.Remove(id);
            SaveRegisteredCourseIds(registeredIds);
            TempData["Success"] = "Du har avregistrerat dig från kursen.";
        }
        else
        {
            TempData["Error"] = "Du är inte registrerad på kursen.";
        }

        return RedirectToAction(nameof(MyCourses));
    }
}
using System.Net.Http.Headers;
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

    private HttpClient CoursesApiClient()
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

    private HttpClient AuthApiClient()
    {
        var client = _http.CreateClient();

        var apiUrl = _config["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(apiUrl))
            throw new Exception("Konfigurationen 'ApiBaseUrl' saknas i appsettings.");

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

    private class EnrollmentResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    private class EnrollmentCountResponse
    {
        public int CourseId { get; set; }
        public int Count { get; set; }
    }

    private async Task<List<int>> GetMyEnrollmentCourseIdsAsync()
    {
        try
        {
            if (!IsStudent())
                return new List<int>();

            var client = AuthApiClient();

            var enrollments = await client.GetFromJsonAsync<List<EnrollmentResponse>>("api/auth/my-enrollments")
                              ?? new List<EnrollmentResponse>();

            return enrollments
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();
        }
        catch
        {
            return new List<int>();
        }
    }

    private async Task<Dictionary<int, int>> GetEnrollmentCountsAsync(List<Course> courses)
    {
        var result = new Dictionary<int, int>();

        if (!IsTeacher() && !IsAdmin())
            return result;

        var client = AuthApiClient();

        foreach (var course in courses)
        {
            try
            {
                var response = await client.GetFromJsonAsync<EnrollmentCountResponse>(
                    $"api/auth/course-enrollment-count/{course.Id}");

                result[course.Id] = response?.Count ?? 0;
            }
            catch
            {
                result[course.Id] = 0;
            }
        }

        return result;
    }

    public async Task<IActionResult> Index()
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        try
        {
            var client = CoursesApiClient();

            var courses = await client.GetFromJsonAsync<List<Course>>("api/Courses")
                          ?? new List<Course>();

            ViewBag.RegisteredCourseIds = await GetMyEnrollmentCourseIdsAsync();
            ViewBag.EnrollmentCounts = await GetEnrollmentCountsAsync(courses);

            return View(courses);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Kunde inte hämta kurser från API: {ex.Message}";
            ViewBag.RegisteredCourseIds = new List<int>();
            ViewBag.EnrollmentCounts = new Dictionary<int, int>();
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
            var coursesClient = CoursesApiClient();
            var allCourses = await coursesClient.GetFromJsonAsync<List<Course>>("api/Courses")
                             ?? new List<Course>();

            var registeredIds = await GetMyEnrollmentCourseIdsAsync();

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
            var client = CoursesApiClient();
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
            var client = CoursesApiClient();
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
            var client = CoursesApiClient();
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
            var client = CoursesApiClient();
            var response = await client.DeleteAsync($"api/Courses/{id}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Kurs borttagen.";
            else
                TempData["Error"] = "Kunde inte ta bort kurs.";

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
    public async Task<IActionResult> Register(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsStudent())
            return RedirectToAction(nameof(Index));

        try
        {
            var client = AuthApiClient();
            var response = await client.PostAsJsonAsync("api/auth/enroll", new { courseId = id });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Du har anmält dig till kursen.";
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                TempData["Error"] = string.IsNullOrWhiteSpace(message)
                    ? "Kunde inte anmäla dig till kursen."
                    : message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Fel vid kontakt med anmälnings-API: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unregister(int id)
    {
        var loginRedirect = RequireLogin();
        if (loginRedirect != null)
            return loginRedirect;

        if (!IsStudent())
            return RedirectToAction(nameof(Index));

        try
        {
            var client = AuthApiClient();
            var response = await client.DeleteAsync($"api/auth/unenroll/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Du har avregistrerat dig från kursen.";
            }
            else
            {
                var message = await response.Content.ReadAsStringAsync();
                TempData["Error"] = string.IsNullOrWhiteSpace(message)
                    ? "Kunde inte avregistrera dig från kursen."
                    : message;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Fel vid kontakt med anmälnings-API: {ex.Message}";
        }

        return RedirectToAction(nameof(MyCourses));
    }
}
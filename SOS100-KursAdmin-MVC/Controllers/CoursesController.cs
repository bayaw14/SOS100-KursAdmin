using Microsoft.AspNetCore.Mvc;

namespace SOS100_KursAdmin_MVC.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
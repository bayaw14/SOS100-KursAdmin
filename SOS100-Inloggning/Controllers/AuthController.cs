using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SOS100_Inloggning.DTOs;
using SOS100_Inloggning.Services;
using System.Security.Claims;

namespace SOS100_Inloggning.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;

    public AuthController(AuthService authService, JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    // CREATE
    [Authorize(Roles = "Admin")]
    [HttpPost("create-user")]
    public IActionResult CreateUser(CreateUserDTO dto)
    {
        var result = _authService.CreateUser(dto);
        return Ok(result);
    }

    // READ – hämta alla användare
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = _authService.GetAllUsers();
        return Ok(users);
    }

    // SÖK - API Key Only
    [HttpGet("search-users")]
    public IActionResult SearchUsers(string q)
    {
        var users = _authService.SearchUsers(q);
        return Ok(users);
    }

    // UPDATE
    [Authorize(Roles = "Admin")]
    [HttpPut("update-user/{id}")]
    public IActionResult UpdateUser(Guid id, UpdateUserDTO dto)
    {
        var result = _authService.UpdateUser(id, dto);
        if (result == null) return NotFound("User not found");
        return Ok(result);
    }

    // DELETE
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-user/{id}")]
    public IActionResult DeleteUser(Guid id)
    {
        var result = _authService.DeleteUser(id);
        if (!result) return NotFound("User not found");
        return Ok("User deleted");
    }

    // LOGIN
    [HttpPost("login")]
    public IActionResult Login(LoginDTO dto)
    {
        var user = _authService.Login(dto);
        if (user == null) return Unauthorized("Invalid login");

        var token = _jwtService.GenerateToken(user);

        return Ok(new
        {
            token,
            role = user.Role,
            name = $"{user.FirstName} {user.LastName}",
            userId = user.Id
        });
    }

    // POST /api/auth/enroll
    [Authorize(Roles = "Student")]
    [HttpPost("enroll")]
    public IActionResult Enroll(EnrollDTO dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User ID saknas i token.");

        var userId = Guid.Parse(userIdClaim);

        var result = _authService.EnrollUser(userId, dto);

        if (result == "Already enrolled")
            return BadRequest(result);

        if (result == "User not found")
            return NotFound(result);

        return Ok(result);
    }

    // GET /api/auth/my-enrollments
    [Authorize(Roles = "Student")]
    [HttpGet("my-enrollments")]
    public IActionResult GetMyEnrollments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User ID saknas i token.");

        var userId = Guid.Parse(userIdClaim);

        var result = _authService.GetEnrollments(userId);
        return Ok(result);
    }

    // DELETE /api/auth/unenroll/{courseId}
    [Authorize(Roles = "Student")]
    [HttpDelete("unenroll/{courseId}")]
    public IActionResult Unenroll(int courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized("User ID saknas i token.");

        var userId = Guid.Parse(userIdClaim);

        var result = _authService.UnenrollUser(userId, courseId);
        if (!result) return NotFound("Ingen registrering hittades.");

        return Ok("Avregistrering lyckades.");
    }

    // GET /api/auth/course-enrollment-count/{courseId}
    [Authorize(Roles = "Teacher,Admin")]
    [HttpGet("course-enrollment-count/{courseId}")]
    public IActionResult GetCourseEnrollmentCount(int courseId)
    {
        var count = _authService.GetEnrollmentCountForCourse(courseId);
        return Ok(new { courseId, count });
    }

    // POST /api/auth/change-password
    [Authorize]
    [HttpPost("change-password")]
    public IActionResult ChangePassword(ChangePasswordDTO dto)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var result = _authService.ChangePassword(email!, dto);

        if (result == null) return BadRequest("Fel nuvarande lösenord");
        return Ok(result);
    }
}
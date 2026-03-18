using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SOS100_Inloggning.DTOs;
using SOS100_Inloggning.Services;

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
            name = $"{user.FirstName} {user.LastName}"
        });
        
    }
}


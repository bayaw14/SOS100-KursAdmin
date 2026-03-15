using SOS100_Inloggning.Data;
using SOS100_Inloggning.Models;
using SOS100_Inloggning.DTOs;
using BCrypt.Net;

namespace SOS100_Inloggning.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public string CreateUser(CreateUserDTO dto)
    {
        if(dto.Role != "Teacher" && dto.Role != "Student")
        {
            return "Only Teacher or Student accounts can be created";
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return $"{dto.Role} created successfully";
    }

    public User? Login(LoginDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);

        if (user == null)
            return null;

        bool validPassword =
            BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        if (!validPassword)
            return null;

        return user;
    }
}
using SOS100_Inloggning.Data;
using SOS100_Inloggning.Models;
using SOS100_Inloggning.DTOs;

namespace SOS100_Inloggning.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    // CREATE
    public string CreateUser(CreateUserDTO dto)
    {
        if (dto.Role != "Teacher" && dto.Role != "Student")
            return "Only Teacher or Student accounts can be created";

        var user = new User
        {
            Id           = Guid.NewGuid(),
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            PersonNumber = dto.PersonNumber,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = dto.Role
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return $"{dto.Role} created successfully";
    }

    // READ
    public List<UserDTO> GetAllUsers()
    {
        return _context.Users
            .Select(u => new UserDTO
            {
                Id           = u.Id,
                FirstName    = u.FirstName,
                LastName     = u.LastName,
                PersonNumber = u.PersonNumber,
                Email        = u.Email,
                Role         = u.Role
            })
            .ToList();
    }

    public List<UserDTO> SearchUsers(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return new List<UserDTO>();

        var lowerQuery = q.ToLower();

        return _context.Users
            .Where(u => u.FirstName.ToLower().Contains(lowerQuery) ||
                        u.LastName.ToLower().Contains(lowerQuery) ||
                        u.Email.ToLower().Contains(lowerQuery))
            .Select(u => new UserDTO
            {
                Id        = u.Id,
                FirstName = u.FirstName,
                LastName  = u.LastName,
                Email     = u.Email,
                Role      = u.Role
            })
            .ToList();
    }

    // UPDATE
    public string? UpdateUser(Guid id, UpdateUserDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(dto.FirstName))    user.FirstName    = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName))     user.LastName     = dto.LastName;
        if (!string.IsNullOrEmpty(dto.PersonNumber)) user.PersonNumber = dto.PersonNumber;
        if (!string.IsNullOrEmpty(dto.Email))        user.Email        = dto.Email;

        if (!string.IsNullOrEmpty(dto.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        if (!string.IsNullOrEmpty(dto.Role) &&
            (dto.Role == "Teacher" || dto.Role == "Student" || dto.Role == "Admin"))
        {
            user.Role = dto.Role;
        }

        _context.SaveChanges();
        return $"{user.Email} updated successfully";
    }

    // DELETE
    public bool DeleteUser(Guid id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;

        var enrollments = _context.Enrollments.Where(e => e.UserId == id).ToList();
        if (enrollments.Any())
        {
            _context.Enrollments.RemoveRange(enrollments);
        }

        _context.Users.Remove(user);
        _context.SaveChanges();

        return true;
    }

    // LOGIN
    public User? Login(LoginDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null) return null;

        bool validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!validPassword) return null;

        return user;
    }

    // ENROLL
    public string EnrollUser(Guid userId, EnrollDTO dto)
    {
        var userExists = _context.Users.Any(u => u.Id == userId);
        if (!userExists) return "User not found";

        var exists = _context.Enrollments
            .Any(e => e.UserId == userId && e.CourseId == dto.CourseId);

        if (exists) return "Already enrolled";

        _context.Enrollments.Add(new Enrollment
        {
            Id       = Guid.NewGuid(),
            UserId   = userId,
            CourseId = dto.CourseId
        });

        _context.SaveChanges();
        return "Enrolled successfully";
    }

    public List<Enrollment> GetEnrollments(Guid userId)
    {
        return _context.Enrollments
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToList();
    }

    public bool UnenrollUser(Guid userId, int courseId)
    {
        var enrollment = _context.Enrollments
            .FirstOrDefault(e => e.UserId == userId && e.CourseId == courseId);

        if (enrollment == null) return false;

        _context.Enrollments.Remove(enrollment);
        _context.SaveChanges();

        return true;
    }

    public int GetEnrollmentCountForCourse(int courseId)
    {
        return _context.Enrollments.Count(e => e.CourseId == courseId);
    }

    public string? ChangePassword(string email, ChangePasswordDTO dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return null;

        bool valid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
        if (!valid) return null;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _context.SaveChanges();

        return "Lösenord uppdaterat";
    }
}
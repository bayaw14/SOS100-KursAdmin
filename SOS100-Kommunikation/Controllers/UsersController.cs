using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Data;
using SOS100_Kommunikation.Models;

namespace SOS100_Kommunikation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AuthDbContext _context;

    public UsersController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<User>());
        }

        try
        {
            var lowerQuery = q.ToLower();
            var users = await _context.Users
                .Where(u => u.FirstName.ToLower().Contains(lowerQuery) || 
                            u.LastName.ToLower().Contains(lowerQuery) || 
                            u.Email.ToLower().Contains(lowerQuery))
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = "Ett databasfel uppstod när sökningen skulle göras (vanligt vid publicering om filen inte hittas).", 
                Error = ex.Message,
                DbConnection = _context.Database.GetDbConnection().ConnectionString
            });
        }
    }
}

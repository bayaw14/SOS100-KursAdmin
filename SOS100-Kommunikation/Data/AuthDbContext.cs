using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Models;

namespace SOS100_Kommunikation.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}

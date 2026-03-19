using Microsoft.EntityFrameworkCore;
using SOS100_Inloggning.Models;

namespace SOS100_Inloggning.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

}
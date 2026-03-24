using Microsoft.EntityFrameworkCore;
using SOS100_Kurser.Models;

namespace SOS100_Kurser.Data;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) 
        : base(options) { }
    
    public DbSet<Course> Courses { get; set; }
}
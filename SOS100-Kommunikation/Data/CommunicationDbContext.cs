using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Models;

namespace SOS100_Kommunikation.Data;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatMessage> Messages { get; set; }
}

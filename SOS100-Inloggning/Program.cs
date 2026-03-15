using BCrypt.Net;
using SOS100_Inloggning.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SOS100_Inloggning.Data;
using SOS100_Inloggning.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================
// Database (SQLite)
// ======================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=kursadmin.db"));


// ======================
// Services
// ======================

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();


// ======================
// Controllers
// ======================

builder.Services.AddControllers();


// ======================
// JWT Authentication
// ======================

var key = Encoding.UTF8.GetBytes("SUPER_SECRET_KEY_123456");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });


// ======================
// Authorization
// ======================

builder.Services.AddAuthorization();


// ======================
// Build App
// ======================

var app = builder.Build();


// ======================
// Middleware
// ======================

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


// ======================
// Map Controllers
// ======================

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// ======================
// Run
// ======================

app.Run();
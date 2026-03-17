using BCrypt.Net;
using SOS100_Inloggning.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SOS100_Inloggning.Data;
using SOS100_Inloggning.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=kursadmin.db"));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtService>();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
        policy.WithOrigins(
                "https://localhost:5001",
                "http://localhost:5000",
                "https://localhost:7002",
                "http://localhost:5002",
                "http://localhost:5015",
                "https://localhost:5015")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
var secret = builder.Configuration["Jwt:Secret"]
             ?? "SUPER_SECRET_KEY_123456_ERSATT_I_PRODUKTION";

var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = false,
        ValidateAudience         = false,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapOpenApi();

// UseHttpsRedirection borttagen — backend körs på http lokalt
app.UseCors("AllowMVC");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var admin = new User
        {
            Id           = Guid.NewGuid(),
            Email        = "admin@eduadmin.se",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role         = "Admin"
        };
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

app.Run();
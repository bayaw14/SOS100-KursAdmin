using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Data;

var builder = WebApplication.CreateBuilder(args);

// Säkra SQLite i Azure (hanterar Zip-Deploy där wwwroot är skrivskyddad)
var commConnectionString = builder.Configuration.GetConnectionString("CommunicationConnection") ?? "Data Source=kommunikation.db";
if (Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") != null) // Om vi är i Azure
{
    var homePath = Environment.GetEnvironmentVariable("HOME") ?? Path.GetTempPath();
    var safeDbPath = Path.Combine(homePath, "kommunikation.db");
    commConnectionString = $"Data Source={safeDbPath}";
}

// Lägg till Databas för meddelanden
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlite(commConnectionString));

// Lägg till HttpClient för proxyanrop
builder.Services.AddHttpClient();

// Add services to the container.

builder.Services.AddControllersWithViews();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapDefaultControllerRoute();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
    db.Database.EnsureCreated();

    try
    {
        // Enkel och säker databasmigrering för att lägga till de nya radering-kolumnerna
        // Detta körs bara en gång, om kolumnerna redan finns slänger SQLite ett fel 
        // som vi medvetet ignorerar. Så slipper man ta bort databasfilen manuellt!
        db.Database.ExecuteSqlRaw("ALTER TABLE Messages ADD COLUMN DeletedBySender INTEGER NOT NULL DEFAULT 0;");
        db.Database.ExecuteSqlRaw("ALTER TABLE Messages ADD COLUMN DeletedByReceiver INTEGER NOT NULL DEFAULT 0;");
    }
    catch { /* Ignorera om kolumnerna redan existerar */ }
}

app.Run();
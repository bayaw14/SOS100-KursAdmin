using Microsoft.EntityFrameworkCore;
using SOS100_Kommunikation.Data;

var builder = WebApplication.CreateBuilder(args);

// Lägg till Databas för meddelanden
builder.Services.AddDbContext<CommunicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CommunicationConnection") ?? "Data Source=kommunikation.db"));

// Lägg till Databas för användare
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AuthConnection") ?? "Data Source=../SOS100-Inloggning/kursadmin.db"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommunicationDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
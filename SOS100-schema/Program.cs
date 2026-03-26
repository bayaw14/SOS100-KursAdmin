using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ScheduleDbContext>(options =>
    options.UseSqlite("Data Source=schedule.db"));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();

    db.Database.EnsureCreated();

    if (!db.ScheduleItems.Any())
    {
        db.ScheduleItems.AddRange(
            new ScheduleItem
            {
                CourseId = 1,
                Title = "Introduktion till programmering",
                Date = DateTime.Today,
                StartTime = "09:00",
                EndTime = "11:00",
                Room = "A101",
                ActivityType = "Föreläsning"
            },
            new ScheduleItem
            {
                CourseId = 2,
                Title = "Webbutveckling med React",
                Date = DateTime.Today.AddDays(1),
                StartTime = "13:00",
                EndTime = "15:00",
                Room = "B202",
                ActivityType = "Workshop"
            },
            new ScheduleItem
            {
                CourseId = 3,
                Title = "Databaser och SQL",
                Date = DateTime.Today.AddDays(2),
                StartTime = "10:00",
                EndTime = "12:00",
                Room = "C303",
                ActivityType = "Laboration"
            }
        );

        db.SaveChanges();
    }
}

app.Run();
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ScheduleDbContext _context;

    public ScheduleController(ScheduleDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleItem>>> GetAll()
    {
        return await _context.ScheduleItems.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ScheduleItem>> GetById(int id)
    {
        var item = await _context.ScheduleItems.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleItem>> Create(ScheduleItem item)
    {
        _context.ScheduleItems.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ScheduleItem item)
    {
        if (id != item.Id)
        {
            return BadRequest();
        }

        var existingItem = await _context.ScheduleItems.FindAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.CourseId = item.CourseId;
        existingItem.Title = item.Title;
        existingItem.Date = item.Date;
        existingItem.StartTime = item.StartTime;
        existingItem.EndTime = item.EndTime;
        existingItem.Room = item.Room;
        existingItem.ActivityType = item.ActivityType;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.ScheduleItems.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        _context.ScheduleItems.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
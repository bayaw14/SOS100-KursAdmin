public class ScheduleItem
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public string Room { get; set; } = "";
    public string ActivityType { get; set; } = "";
}
namespace SOA_Gruppuppgift.Models;

public class CourseListItemVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Teacher { get; set; } = "";
    public decimal CreditsHp { get; set; }
}

public class CourseDetailsVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Teacher { get; set; } = "";
    public decimal CreditsHp { get; set; }
    public string Description { get; set; } = "";
    public List<ScheduleItemVm> Schedule { get; set; } = new();
}

public class ScheduleItemVm
{
    public string CourseName { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string Room { get; set; } = "";
}
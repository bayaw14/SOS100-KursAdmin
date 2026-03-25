namespace SOS100_KursAdmin_MVC.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Credits { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        public bool IsForStaff { get; set; }
    }
}
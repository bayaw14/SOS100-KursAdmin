namespace SOS100_KursAdmin_MVC.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string CourseCode { get; set; } = "";

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public double Credits { get; set; }

        public string TeacherName { get; set; } = "";

        public bool IsForStaff { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        public string CourseCode { get; set; } = string.Empty;
        
        [Required]
        public string CourseName { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int CreditHours { get; set; }
        
        public string Department { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
        public ICollection<StudentCourseRegistration> StudentRegistrations { get; set; } = new List<StudentCourseRegistration>();
        public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}

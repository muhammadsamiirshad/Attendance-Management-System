using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        
        [Required]
        public string AppUserId { get; set; } = string.Empty;
        
        public AppUser AppUser { get; set; } = null!;
        
        [Required]
        public string TeacherNumber { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;
        
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
        public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
    }
}

using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class Student
    {
        public int Id { get; set; }
        
        [Required]
        public string AppUserId { get; set; } = string.Empty;
        
        public AppUser AppUser { get; set; } = null!;
        
        [Required]
        public string StudentNumber { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Phone { get; set; } = string.Empty;
        
        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public ICollection<StudentSection> StudentSections { get; set; } = new List<StudentSection>();
        public ICollection<StudentCourseRegistration> CourseRegistrations { get; set; } = new List<StudentCourseRegistration>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}

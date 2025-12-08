using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class StudentCourseRegistration
    {
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        public Student Student { get; set; } = null!;
        
        [Required]
        public int CourseId { get; set; }
        
        public Course Course { get; set; } = null!;
        
        public DateTime RegisteredDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}

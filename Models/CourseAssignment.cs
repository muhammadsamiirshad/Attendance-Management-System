using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class CourseAssignment
    {
        public int Id { get; set; }
        
        [Required]
        public int TeacherId { get; set; }
        
        public Teacher Teacher { get; set; } = null!;
        
        [Required]
        public int CourseId { get; set; }
        
        public Course Course { get; set; } = null!;
        
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}

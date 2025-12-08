using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused
    }

    public class Attendance
    {
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        public Student Student { get; set; } = null!;
        
        [Required]
        public int CourseId { get; set; }
        
        public Course Course { get; set; } = null!;
        
        [Required]
        public DateTime Date { get; set; }
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        public string? Remarks { get; set; }
        
        // Helper property for backward compatibility
        public bool IsPresent => Status == AttendanceStatus.Present;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; } = string.Empty;
    }
}

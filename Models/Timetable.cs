using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public enum DayOfWeekEnum
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }

    public class Timetable
    {
        public int Id { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        public Course Course { get; set; } = null!;
        
        [Required]
        public int TeacherId { get; set; }
        
        public Teacher Teacher { get; set; } = null!;
        
        [Required]
        public int SectionId { get; set; }
        
        public Section Section { get; set; } = null!;
        
        [Required]
        public DayOfWeekEnum Day { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        public string Classroom { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}

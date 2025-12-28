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
        
        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }
        
        public Course? Course { get; set; }
        
        [Required(ErrorMessage = "Teacher is required")]
        public int TeacherId { get; set; }
        
        public Teacher? Teacher { get; set; }
        
        [Required(ErrorMessage = "Section is required")]
        public int SectionId { get; set; }
        
        public Section? Section { get; set; }
        
        [Required(ErrorMessage = "Day is required")]
        public DayOfWeekEnum Day { get; set; }
        
        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }
        
        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }
        
        public string Classroom { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class Section
    {
        public int Id { get; set; }
        
        [Required]
        public string SectionName { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public int Capacity { get; set; } = 50;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<StudentSection> StudentSections { get; set; } = new List<StudentSection>();
        public ICollection<SessionSection> SessionSections { get; set; } = new List<SessionSection>();
        public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
    }
}

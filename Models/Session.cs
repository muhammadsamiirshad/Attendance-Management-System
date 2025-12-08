using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class Session
    {
        public int Id { get; set; }
        
        [Required]
        public string SessionName { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int Year => StartDate.Year;
        
        // Navigation Properties
        public ICollection<SessionSection> SessionSections { get; set; } = new List<SessionSection>();
    }
}

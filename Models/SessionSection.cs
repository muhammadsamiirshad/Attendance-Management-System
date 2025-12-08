using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class SessionSection
    {
        public int Id { get; set; }
        
        [Required]
        public int SessionId { get; set; }
        
        public Session Session { get; set; } = null!;
        
        [Required]
        public int SectionId { get; set; }
        
        public Section Section { get; set; } = null!;
        
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}

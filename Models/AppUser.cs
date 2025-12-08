using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        public bool FirstLogin { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

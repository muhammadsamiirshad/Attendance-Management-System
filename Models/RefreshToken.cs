using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public string JwtId { get; set; } = string.Empty;
        
        public bool IsUsed { get; set; }
        
        public bool IsRevoked { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public AppUser? User { get; set; }
    }
}

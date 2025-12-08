namespace AMS.Models
{
    public class AuthResult
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class TokenRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

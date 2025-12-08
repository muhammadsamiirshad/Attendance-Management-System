using AMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AMS.Services
{
    public interface IJwtService
    {
        Task<AuthResult> GenerateJwtTokenAsync(AppUser user);
        Task<AuthResult> VerifyAndGenerateTokenAsync(RefreshTokenRequest tokenRequest);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            TokenValidationParameters tokenValidationParameters)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _context = context;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<AuthResult> GenerateJwtTokenAsync(AppUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("userId", user.Id),
                new Claim("fullName", user.FullName)
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                Token = GenerateRandomString(35),
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
                IsUsed = false,
                IsRevoked = false
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResult> VerifyAndGenerateTokenAsync(RefreshTokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validate JWT token format
                var principal = GetPrincipalFromToken(tokenRequest.Token);
                if (principal == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid token" }
                    };
                }

                var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid token" }
                    };
                }

                // Check if refresh token exists
                var storedRefreshToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == tokenRequest.RefreshToken);
                if (storedRefreshToken == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Refresh token does not exist" }
                    };
                }

                // Check if refresh token is expired
                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Refresh token has expired" }
                    };
                }

                // Check if refresh token has been used
                if (storedRefreshToken.IsUsed)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Refresh token has been used" }
                    };
                }

                // Check if refresh token has been revoked
                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Refresh token has been revoked" }
                    };
                }

                // Validate refresh token JwtId matches
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "Token doesn't match" }
                    };
                }

                // Mark refresh token as used
                storedRefreshToken.IsUsed = true;
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                // Generate new token
                var userId = principal.Claims.FirstOrDefault(x => x.Type == "userId")?.Value;
                var user = await _userManager.FindByIdAsync(userId ?? string.Empty);
                if (user == null)
                {
                    return new AuthResult
                    {
                        Success = false,
                        Errors = new List<string> { "User not found" }
                    };
                }

                return await GenerateJwtTokenAsync(user);
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Server error", ex.Message }
                };
            }
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
                
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private string GenerateRandomString(int length)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }
    }
}

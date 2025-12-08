using AMS.Models;
using AMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;

        public AuthController(UserManager<AppUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Login endpoint - generates JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid request" }
                });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid email or password" }
                });
            }

            var authResult = await _jwtService.GenerateJwtTokenAsync(user);
            return Ok(authResult);
        }

        /// <summary>
        /// Refresh token endpoint
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid request" }
                });
            }

            var result = await _jwtService.VerifyAndGenerateTokenAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current user info (requires authentication)
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                roles = roles
            });
        }

        /// <summary>
        /// Register new user (Admin only or open registration)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Email already in use" }
                });
            }

            var newUser = new AppUser
            {
                Email = request.Email,
                UserName = request.Email,
                FullName = request.FullName,
                FirstLogin = true
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new AuthResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            // Assign default role (Student) if no role specified
            var role = string.IsNullOrEmpty(request.Role) ? "Student" : request.Role;
            await _userManager.AddToRoleAsync(newUser, role);

            var authResult = await _jwtService.GenerateJwtTokenAsync(newUser);
            return Ok(authResult);
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Role { get; set; }
    }
}

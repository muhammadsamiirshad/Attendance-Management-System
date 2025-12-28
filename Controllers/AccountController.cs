using AMS.Models;
using AMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IStudentRepository _studentRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IJwtService _jwtService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IStudentRepository studentRepo,
            ITeacherRepository teacherRepo,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _studentRepo = studentRepo;
            _teacherRepo = teacherRepo;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user, 
                        model.Password, 
                        model.RememberMe, 
                        false);
                    
                    if (result.Succeeded)
                    {
                        // Store JWT token in cookie for API calls from the web app
                        var authResult = await _jwtService.GenerateJwtTokenAsync(user);
                        if (!string.IsNullOrEmpty(authResult.Token))
                        {
                            // Set JWT token cookie - persistent across browser sessions
                            var jwtCookieOptions = new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true, // Ensure HTTPS
                                SameSite = SameSiteMode.Strict,
                                Expires = model.RememberMe 
                                    ? DateTimeOffset.UtcNow.AddDays(30) // 30 days persistent
                                    : DateTimeOffset.UtcNow.AddHours(12), // 12 hours session
                                IsEssential = true // Essential cookie for authentication
                            };
                            
                            Response.Cookies.Append("jwt_token", authResult.Token, jwtCookieOptions);
                            
                            // Set refresh token cookie if available - always persistent
                            if (!string.IsNullOrEmpty(authResult.RefreshToken))
                            {
                                Response.Cookies.Append("refresh_token", authResult.RefreshToken, new CookieOptions
                                {
                                    HttpOnly = true,
                                    Secure = true,
                                    SameSite = SameSiteMode.Strict,
                                    Expires = DateTimeOffset.UtcNow.AddDays(30), // 30 days for refresh token
                                    IsEssential = true
                                });
                            }
                            
                            // Store RememberMe preference
                            Response.Cookies.Append("remember_me", model.RememberMe.ToString(), new CookieOptions
                            {
                                HttpOnly = false,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddDays(30),
                                IsEssential = true
                            });
                        }

                        if (user.FirstLogin)
                        {
                            return RedirectToAction("ChangePassword");
                        }

                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Teacher"))
                        {
                            return RedirectToAction("Index", "Teacher");
                        }
                        else if (roles.Contains("Student"))
                        {
                            return RedirectToAction("Index", "Student");
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
                
                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    
                    if (result.Succeeded)
                    {
                        user.FirstLogin = false;
                        await _userManager.UpdateAsync(user);
                        
                        TempData["SuccessMessage"] = "Password changed successfully!";
                        
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Teacher"))
                        {
                            return RedirectToAction("Index", "Teacher");
                        }
                        else if (roles.Contains("Student"))
                        {
                            return RedirectToAction("Index", "Student");
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                    
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Sign out from Identity
            await _signInManager.SignOutAsync();
            
            // Remove JWT token cookie with all options
            Response.Cookies.Delete("jwt_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            
            // Remove refresh token cookie with all options
            Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            
            // Clear all authentication cookies
            Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
            
            // Clear login message cookie if it exists
            Response.Cookies.Delete("LoginMessage");
            
            // Clear session if available
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Clear();
            }
            
            TempData["Info"] = "You have been successfully logged out.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

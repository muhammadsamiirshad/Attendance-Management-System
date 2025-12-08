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
                            Response.Cookies.Append("jwt_token", authResult.Token, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
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
            await _signInManager.SignOutAsync();
            
            // Remove JWT token cookie
            Response.Cookies.Delete("jwt_token");
            
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Identity;
using AMS.Models;
using AMS.Services;
using System.IdentityModel.Tokens.Jwt;

namespace AMS.Middleware
{
    /// <summary>
    /// Middleware to validate JWT token from cookie and ensure proper authentication
    /// Ensures professional token management with forced re-login when token is missing or invalid
    /// </summary>
    public class JwtCookieAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtCookieAuthenticationMiddleware> _logger;

        public JwtCookieAuthenticationMiddleware(RequestDelegate next, ILogger<JwtCookieAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, SignInManager<AppUser> signInManager, IJwtService jwtService, UserManager<AppUser> userManager)
        {
            // Skip for login, logout, and public pages
            var path = context.Request.Path.Value?.ToLower() ?? "";
            if (path.Contains("/account/login") || 
                path.Contains("/account/logout") || 
                path.Contains("/account/accessdenied") ||
                path.Contains("/api/auth/login") ||
                path.Contains("/home/") ||
                path.StartsWith("/css/") ||
                path.StartsWith("/js/") ||
                path.StartsWith("/lib/") ||
                path.Contains("favicon.ico"))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated via cookie
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Verify JWT token exists in cookie
                var jwtToken = context.Request.Cookies["jwt_token"];
                
                if (string.IsNullOrEmpty(jwtToken))
                {
                    // JWT token is missing - this means token was deleted from browser
                    // Force complete logout and redirect to login
                    _logger.LogWarning("JWT token missing for authenticated user {User}. Forcing logout.", 
                        context.User.Identity.Name);
                    
                    await SignOutUserCompletely(context, signInManager);
                    
                    // Add message to notify user
                    context.Response.Cookies.Append("LoginMessage", "Your session has expired. Please login again.", 
                        new CookieOptions { 
                            HttpOnly = false, 
                            Secure = true, 
                            SameSite = SameSiteMode.Strict,
                            MaxAge = TimeSpan.FromSeconds(30)
                        });
                    
                    context.Response.Redirect("/Account/Login");
                    return;
                }
                
                // Validate the JWT token
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    
                    // Check if token can be read
                    if (!tokenHandler.CanReadToken(jwtToken))
                    {
                        _logger.LogWarning("Invalid JWT token format for user {User}. Forcing logout.", 
                            context.User.Identity.Name);
                        await SignOutUserCompletely(context, signInManager);
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                    
                    var jwtSecurityToken = tokenHandler.ReadJwtToken(jwtToken);
                    
                    // Check if token is expired or about to expire (within 5 minutes)
                    var expirationBuffer = TimeSpan.FromMinutes(5);
                    var isExpired = jwtSecurityToken.ValidTo < DateTime.UtcNow;
                    var isAboutToExpire = jwtSecurityToken.ValidTo < DateTime.UtcNow.Add(expirationBuffer);
                    
                    if (isExpired)
                    {
                        // Try to refresh the token using refresh token
                        var refreshToken = context.Request.Cookies["refresh_token"];
                        
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            _logger.LogInformation("JWT token expired. Attempting automatic refresh for user {User}.", 
                                context.User.Identity.Name);
                            
                            try
                            {
                                // Attempt to refresh the token
                                var refreshRequest = new RefreshTokenRequest 
                                { 
                                    Token = jwtToken, 
                                    RefreshToken = refreshToken 
                                };
                                
                                var refreshResult = await jwtService.VerifyAndGenerateTokenAsync(refreshRequest);
                                
                                if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.Token))
                                {
                                    // Update cookies with new tokens
                                    var rememberMe = context.Request.Cookies["remember_me"] == "True";
                                    
                                    var jwtCookieOptions = new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = true,
                                        SameSite = SameSiteMode.Strict,
                                        Expires = rememberMe 
                                            ? DateTimeOffset.UtcNow.AddDays(30) 
                                            : DateTimeOffset.UtcNow.AddHours(12),
                                        IsEssential = true
                                    };
                                    
                                    context.Response.Cookies.Append("jwt_token", refreshResult.Token, jwtCookieOptions);
                                    
                                    if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                                    {
                                        context.Response.Cookies.Append("refresh_token", refreshResult.RefreshToken, new CookieOptions
                                        {
                                            HttpOnly = true,
                                            Secure = true,
                                            SameSite = SameSiteMode.Strict,
                                            Expires = DateTimeOffset.UtcNow.AddDays(30),
                                            IsEssential = true
                                        });
                                    }
                                    
                                    _logger.LogInformation("JWT token successfully refreshed for user {User}.", 
                                        context.User.Identity.Name);
                                    
                                    // Continue with the request using the new token
                                    await _next(context);
                                    return;
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to refresh JWT token for user {User}. Forcing logout.", 
                                        context.User.Identity.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error refreshing JWT token for user {User}. Forcing logout.", 
                                    context.User.Identity.Name);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("JWT token expired and no refresh token available for user {User}. Forcing logout.", 
                                context.User.Identity.Name);
                        }
                        
                        // If we reach here, refresh failed - force logout
                        await SignOutUserCompletely(context, signInManager);
                        
                        context.Response.Cookies.Append("LoginMessage", "Your session has expired. Please login again.", 
                            new CookieOptions { 
                                HttpOnly = false, 
                                Secure = true, 
                                SameSite = SameSiteMode.Strict,
                                MaxAge = TimeSpan.FromSeconds(30)
                            });
                        
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                    else if (isAboutToExpire && !isExpired)
                    {
                        // Token is about to expire - refresh it proactively in the background
                        var refreshToken = context.Request.Cookies["refresh_token"];
                        
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            _logger.LogInformation("JWT token expiring soon. Proactively refreshing for user {User}.", 
                                context.User.Identity.Name);
                            
                            // Fire and forget - don't block the request
                            _ = Task.Run(async () =>
                            {
                                try
                                {
                                    var refreshRequest = new RefreshTokenRequest 
                                    { 
                                        Token = jwtToken, 
                                        RefreshToken = refreshToken 
                                    };
                                    
                                    var refreshResult = await jwtService.VerifyAndGenerateTokenAsync(refreshRequest);
                                    
                                    if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.Token))
                                    {
                                        _logger.LogInformation("JWT token proactively refreshed for user {User}.", 
                                            context.User.Identity.Name);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "Failed to proactively refresh JWT token for user {User}.", 
                                        context.User.Identity.Name);
                                }
                            });
                        }
                    }
                    
                    // Validate token has required claims
                    var userIdClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "userId");
                    var emailClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
                    
                    if (userIdClaim == null || emailClaim == null)
                    {
                        _logger.LogWarning("JWT token missing required claims for user {User}. Forcing logout.", 
                            context.User.Identity.Name);
                        await SignOutUserCompletely(context, signInManager);
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid JWT token detected for user {User}. Forcing logout.", 
                        context.User.Identity?.Name ?? "Unknown");
                    await SignOutUserCompletely(context, signInManager);
                    
                    context.Response.Cookies.Append("LoginMessage", "Authentication error. Please login again.", 
                        new CookieOptions { 
                            HttpOnly = false, 
                            Secure = true, 
                            SameSite = SameSiteMode.Strict,
                            MaxAge = TimeSpan.FromSeconds(30)
                        });
                    
                    context.Response.Redirect("/Account/Login");
                    return;
                }
            }
            else
            {
                // User is not authenticated - ensure all tokens are cleared
                if (context.Request.Cookies.ContainsKey("jwt_token") || 
                    context.Request.Cookies.ContainsKey("refresh_token"))
                {
                    _logger.LogInformation("Clearing orphaned tokens for unauthenticated user.");
                    context.Response.Cookies.Delete("jwt_token");
                    context.Response.Cookies.Delete("refresh_token");
                }
            }

            await _next(context);
        }
        
        /// <summary>
        /// Completely signs out the user and clears all authentication tokens
        /// </summary>
        private async Task SignOutUserCompletely(HttpContext context, SignInManager<AppUser> signInManager)
        {
            // Sign out from Identity
            await signInManager.SignOutAsync();
            
            // Remove all authentication cookies
            context.Response.Cookies.Delete("jwt_token", new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict 
            });
            
            context.Response.Cookies.Delete("refresh_token", new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Strict 
            });
            
            context.Response.Cookies.Delete(".AspNetCore.Identity.Application", new CookieOptions 
            { 
                HttpOnly = true, 
                Secure = true, 
                SameSite = SameSiteMode.Lax 
            });
            
            // Clear any session data
            if (context.Session != null)
            {
                await context.Session.CommitAsync();
                context.Session.Clear();
            }
        }
    }

    public static class JwtCookieAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieAuthenticationMiddleware>();
        }
    }
}

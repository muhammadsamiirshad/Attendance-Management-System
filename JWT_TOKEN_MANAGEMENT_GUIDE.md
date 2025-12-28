# JWT Token Management & Persistent Login - Complete Guide

## Overview
This document explains how the JWT token management and persistent login functionality works in the Attendance Management System (AMS).

## Token Architecture

### 1. **Dual Authentication System**
The system uses a hybrid authentication approach:
- **ASP.NET Core Identity Cookies**: For MVC web authentication
- **JWT Tokens**: For API calls and additional security layer

### 2. **Token Storage**
All tokens are stored as **HTTP-only secure cookies**:

| Cookie Name | Purpose | Duration | HTTP-Only | Secure |
|-------------|---------|----------|-----------|--------|
| `.AspNetCore.Identity.Application` | Identity authentication | 30 days (sliding) | ✅ Yes | ✅ Yes |
| `jwt_token` | JWT access token | 30 days or 12 hours | ✅ Yes | ✅ Yes |
| `refresh_token` | JWT refresh token | 30 days | ✅ Yes | ✅ Yes |
| `remember_me` | Remember me preference | 30 days | ❌ No | ✅ Yes |

## Persistent Login Implementation

### Configuration in `appsettings.json`
```json
{
  "Jwt": {
    "Key": "AMS_SecureKey_2024_MustBe32CharactersOrMore_ForJWT",
    "Issuer": "AMSApplication",
    "Audience": "AMSUsers",
    "ExpiryMinutes": 720,        // 12 hours
    "RefreshTokenExpiryDays": 30  // 30 days
  }
}
```

### Cookie Configuration in `Program.cs`
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Cookie valid for 30 days
    options.SlidingExpiration = true; // Refresh cookie on each request
});
```

## Login Flow

### When User Logs In:

1. **User submits credentials** → `AccountController.Login()`

2. **Identity authenticates user** → `SignInManager.PasswordSignInAsync()`

3. **JWT tokens are generated**:
   - Access Token (JWT) - Valid for 12 hours
   - Refresh Token - Valid for 30 days

4. **Cookies are set based on "Remember Me"**:

   **If "Remember Me" is CHECKED:**
   ```csharp
   // Identity cookie: 30 days persistent
   // JWT token: 30 days persistent
   // Refresh token: 30 days persistent
   Expires = DateTimeOffset.UtcNow.AddDays(30)
   ```

   **If "Remember Me" is NOT CHECKED:**
   ```csharp
   // Identity cookie: 30 days persistent (always)
   // JWT token: 12 hours session only
   // Refresh token: 30 days persistent
   Expires = DateTimeOffset.UtcNow.AddHours(12)
   ```

5. **User is redirected to their dashboard** based on role

## Token Validation & Refresh Flow

### Middleware: `JwtCookieAuthenticationMiddleware`

On every request, the middleware:

1. **Checks if user is authenticated**
   - If not → Allow request (public pages)
   - If yes → Validate JWT token

2. **Validates JWT token exists in cookie**
   - If missing → Force logout and redirect to login
   - If exists → Continue validation

3. **Checks token expiration**:
   
   **If Token is Expired:**
   - Attempts to refresh using refresh token
   - If refresh succeeds → Updates cookies with new tokens
   - If refresh fails → Force logout and redirect to login

   **If Token is About to Expire (within 5 minutes):**
   - Proactively refreshes token in background
   - Request continues without interruption

4. **Validates token claims**
   - Checks for required claims (userId, email, role)
   - If invalid → Force logout

## Persistent Login Behavior

### ✅ **What Users Experience:**

1. **User logs in with "Remember Me" checked**
   - Closes browser completely
   - Opens browser again after hours/days
   - **User is STILL LOGGED IN** ✅
   - Can access dashboard directly without re-login

2. **User logs in without "Remember Me"**
   - Closes browser completely
   - Opens browser again
   - **User is STILL LOGGED IN** ✅
   - Can access dashboard (Identity cookie persists)
   - JWT token expires after 12 hours (will auto-refresh if refresh token valid)

3. **User manually deletes cookies in browser**
   - Opens application
   - **User is logged out** ❌
   - Must login again

4. **Token expires naturally after 30 days**
   - User opens application
   - **User is logged out** ❌
   - Must login again

### Token Refresh Process

**Automatic Token Refresh:**
```
User Request → Middleware checks JWT
              ↓
         JWT expired?
              ↓
         Yes → Get refresh token
              ↓
         Validate refresh token
              ↓
         Generate new JWT + Refresh token
              ↓
         Update cookies
              ↓
         Continue request
```

## Security Features

### 1. **HTTP-Only Cookies**
- ✅ Prevents JavaScript access to tokens
- ✅ Protects against XSS attacks
- ✅ Tokens cannot be stolen via client-side scripts

### 2. **Secure Flag**
- ✅ Cookies only sent over HTTPS
- ✅ Prevents man-in-the-middle attacks
- ✅ Production-ready security

### 3. **SameSite Policy**
- ✅ Strict/Lax SameSite policy
- ✅ Prevents CSRF attacks
- ✅ Cookies not sent on cross-site requests

### 4. **Token Validation**
- ✅ Signature validation
- ✅ Issuer validation
- ✅ Audience validation
- ✅ Expiration validation
- ✅ Claims validation

### 5. **Refresh Token Security**
- ✅ One-time use tokens
- ✅ Revocation support
- ✅ Stored in database with expiry
- ✅ Automatic cleanup of expired tokens

## Logout Process

### When User Logs Out:

1. **User clicks Logout** → `AccountController.Logout()`

2. **Identity signs out** → `SignInManager.SignOutAsync()`

3. **All cookies are deleted**:
   - Identity cookie
   - JWT token cookie
   - Refresh token cookie
   - Session cookies

4. **Session is cleared**

5. **User is redirected to Login page**

## Token Expiration Scenarios

| Scenario | Token State | Result |
|----------|-------------|--------|
| User active, token valid | Valid | ✅ Access granted |
| User active, token expired, refresh valid | Expired | ✅ Auto-refresh, access granted |
| User inactive 12 hours, refresh valid | Expired | ✅ Auto-refresh on next request |
| User inactive 30 days | Expired | ❌ Must login again |
| User deletes cookies | Missing | ❌ Must login again |
| Refresh token used/revoked | Invalid | ❌ Must login again |

## Browser Behavior

### ✅ **Persistent Across:**
- Browser restart
- Computer restart
- Network changes
- Multiple tabs
- Private browsing exit (cookies saved if "Remember Me")

### ❌ **NOT Persistent:**
- Manual cookie deletion
- Browser data clear
- Token expiration (30 days)
- Explicit logout
- Token revocation

## Testing Persistent Login

### Test Case 1: Browser Restart
1. Login with "Remember Me" checked
2. Close all browser windows
3. Open browser and navigate to application
4. **Expected**: User is logged in automatically ✅

### Test Case 2: Long Inactivity
1. Login with "Remember Me" checked
2. Leave application idle for 24 hours
3. Access application
4. **Expected**: User is logged in, token auto-refreshed ✅

### Test Case 3: Manual Cookie Deletion
1. Login successfully
2. Open browser DevTools → Application → Cookies
3. Delete `jwt_token` cookie
4. Refresh page
5. **Expected**: User is logged out, redirected to login ❌

### Test Case 4: Token Expiration
1. Login successfully
2. Wait 30+ days (or manually expire tokens in DB)
3. Access application
4. **Expected**: User is logged out, must re-login ❌

## Database Tables

### RefreshTokens Table
Stores refresh tokens with metadata:

```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    JwtId NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL,
    IsRevoked BIT NOT NULL
)
```

## Monitoring & Logging

The middleware logs all authentication events:

- ✅ Token validation success
- ✅ Token refresh attempts
- ✅ Token expiration events
- ✅ Forced logout events
- ⚠️ Invalid token warnings
- ❌ Authentication failures

## Configuration Options

### For Developers:

**Change token expiration time:**
```json
"Jwt": {
    "ExpiryMinutes": 720,        // Change JWT expiration
    "RefreshTokenExpiryDays": 30  // Change refresh token expiration
}
```

**Change cookie persistence:**
```csharp
options.ExpireTimeSpan = TimeSpan.FromDays(30); // Change in Program.cs
```

## Troubleshooting

### Problem: User keeps getting logged out
**Solution**: Check if cookies are being blocked by browser or privacy settings

### Problem: Token refresh fails
**Solution**: Check database for expired/revoked refresh tokens, ensure clock sync

### Problem: Cookies not persisting after browser restart
**Solution**: Verify "Remember Me" is checked, check cookie expiration settings

### Problem: "Your session has expired" message
**Solution**: This is normal after 30 days or if tokens are manually deleted

## Summary

✅ **JWT tokens are stored in HTTP-only secure cookies**
✅ **Login persists across browser restarts for 30 days**
✅ **Automatic token refresh when expired**
✅ **Secure against XSS, CSRF, and other attacks**
✅ **User stays logged in until explicit logout or token expiration**
✅ **Manual cookie deletion = forced logout (as expected)**

The system ensures users remain authenticated as long as their tokens are valid, providing a seamless experience while maintaining high security standards.

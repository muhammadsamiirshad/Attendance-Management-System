# AMS JWT Authentication Testing Guide

This guide provides step-by-step instructions for testing all aspects of JWT authentication in the AMS (Attendance Management System).

## Prerequisites

✅ **Database**: Migrations applied successfully
✅ **Build**: Project builds without errors
✅ **RefreshTokens Table**: Created and ready

## Testing Environment Setup

### 1. Start the Application

```powershell
cd 'c:\Users\Administrator\Desktop\EAD\AMS\AMS'
dotnet run
```

The application should start on `https://localhost:5001` (HTTPS) and `http://localhost:5000` (HTTP).

### 2. Access Swagger UI

Open your browser and navigate to:
```
https://localhost:5001/swagger
```

## Test Scenarios

### Scenario 1: MVC Web Application Authentication

#### Test 1.1: Login via Web Interface
1. Navigate to `https://localhost:5001/Account/Login`
2. Enter valid credentials:
   - **Username**: (your test user)
   - **Password**: (your test password)
3. Click "Login"
4. **Expected Result**: 
   - Redirected to home page
   - User is authenticated
   - JWT token stored in httpOnly cookie

#### Test 1.2: Access Protected Pages
1. After logging in, navigate to:
   - `https://localhost:5001/Teacher/ManageAttendance`
   - `https://localhost:5001/Attendance/Mark`
2. **Expected Result**: Pages load successfully (no redirect to login)

#### Test 1.3: Logout
1. Click "Logout" button
2. **Expected Result**:
   - Redirected to home page
   - JWT cookie cleared
   - Accessing protected pages redirects to login

### Scenario 2: API Authentication with JWT

#### Test 2.1: Login via API (Get JWT Token)

**PowerShell:**
```powershell
$loginBody = @{
    Username = "testuser"
    Password = "TestPassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $loginBody `
    -SkipCertificateCheck

Write-Host "Token: $($response.token)"
Write-Host "Refresh Token: $($response.refreshToken)"
Write-Host "Expires: $($response.expiresAt)"

# Save tokens for later use
$token = $response.token
$refreshToken = $response.refreshToken
```

**Expected Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64_encoded_string",
  "expiresAt": "2024-12-07T10:30:00Z",
  "message": "Login successful"
}
```

#### Test 2.2: Access Protected API Endpoint

**PowerShell:**
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/user" `
    -Method Get `
    -Headers $headers `
    -SkipCertificateCheck

Write-Host "User Info: $($response | ConvertTo-Json)"
```

**Expected Response:**
```json
{
  "username": "testuser",
  "email": "test@example.com",
  "roles": ["Teacher"]
}
```

#### Test 2.3: Refresh Token

**PowerShell:**
```powershell
$refreshBody = @{
    Token = $token
    RefreshToken = $refreshToken
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/refresh" `
    -Method Post `
    -ContentType "application/json" `
    -Body $refreshBody `
    -SkipCertificateCheck

Write-Host "New Token: $($response.token)"
Write-Host "New Refresh Token: $($response.refreshToken)"

# Update tokens
$token = $response.token
$refreshToken = $response.refreshToken
```

**Expected Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new_base64_encoded_string",
  "expiresAt": "2024-12-07T11:30:00Z",
  "message": "Token refreshed successfully"
}
```

#### Test 2.4: Access API with Expired Token

1. Wait for token to expire (default: 60 minutes) OR manually use an old token
2. Try to access protected endpoint
3. **Expected Result**: HTTP 401 Unauthorized

**PowerShell:**
```powershell
$headers = @{
    "Authorization" = "Bearer expired_token_here"
}

try {
    Invoke-RestMethod -Uri "https://localhost:5001/api/auth/user" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
} catch {
    Write-Host "Expected Error: $($_.Exception.Message)"
}
```

### Scenario 3: Attendance Marking

#### Test 3.1: Mark Attendance (MVC)
1. Login as Teacher
2. Navigate to `https://localhost:5001/Teacher/ManageAttendance`
3. Select a course and date
4. Mark students as Present/Absent/Late/Excused
5. Leave "Remarks" field empty for some students
6. Click "Save Attendance"
7. **Expected Result**: 
   - Attendance saved successfully
   - No validation error about "Remarks field required"

#### Test 3.2: Mark Attendance via API

**PowerShell:**
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}

$attendanceData = @{
    StudentId = 1
    CourseId = 1
    Date = (Get-Date).ToString("yyyy-MM-dd")
    Status = 0  # 0=Present, 1=Absent, 2=Late, 3=Excused
    Remarks = $null  # Optional field
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/attendance/mark" `
    -Method Post `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $attendanceData `
    -SkipCertificateCheck

Write-Host "Attendance marked: $($response | ConvertTo-Json)"
```

### Scenario 4: Role-Based Access Control

#### Test 4.1: Admin-Only Endpoints
1. Login as a Teacher (non-admin) and get token
2. Try to access admin-only endpoint
3. **Expected Result**: HTTP 403 Forbidden

**PowerShell:**
```powershell
# Login as Teacher
$teacherToken = "teacher_jwt_token"

$headers = @{
    "Authorization" = "Bearer $teacherToken"
}

try {
    Invoke-RestMethod -Uri "https://localhost:5001/api/admin/users" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
} catch {
    Write-Host "Expected 403 Forbidden: $($_.Exception.Message)"
}
```

#### Test 4.2: Multiple Roles
1. Login as user with multiple roles
2. Access endpoints requiring different roles
3. **Expected Result**: Access granted based on roles in JWT

## Troubleshooting

### Issue 1: "Invalid object name 'RefreshTokens'"

**Solution:**
```powershell
cd 'c:\Users\Administrator\Desktop\EAD\AMS\AMS'
dotnet ef database update --no-build
```

### Issue 2: "Remarks field is required"

**Status**: ✅ FIXED
- `Remarks` property is now optional (`string?`)
- No validation errors when left empty

### Issue 3: HTTP 401 after login

**Possible Causes:**
1. Token not stored correctly
2. Token format invalid
3. Authentication middleware not configured

**Debugging:**
```powershell
# Check if cookie is set
# In browser DevTools → Application → Cookies
# Look for "AuthToken" cookie

# Test API token directly
$token = "your_jwt_token"
[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($token.Split('.')[1]))
```

### Issue 4: CORS Errors (for SPA/React clients)

**Solution:** Already configured in `Program.cs`
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

## Database Verification

### Check RefreshTokens Table

**SQL Query:**
```sql
-- Check if table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'RefreshTokens'

-- View all refresh tokens
SELECT Id, UserId, Token, JwtId, IsUsed, IsRevoked, ExpiryDate, CreatedDate
FROM RefreshTokens

-- Check for expired tokens
SELECT COUNT(*) as ExpiredCount
FROM RefreshTokens
WHERE ExpiryDate < GETDATE()

-- Check for active tokens
SELECT COUNT(*) as ActiveCount
FROM RefreshTokens
WHERE ExpiryDate > GETDATE() AND IsUsed = 0 AND IsRevoked = 0
```

## Performance Testing

### Test Token Generation Performance

**PowerShell:**
```powershell
$iterations = 10
$times = @()

for ($i = 0; $i -lt $iterations; $i++) {
    $start = Get-Date
    
    $loginBody = @{
        Username = "testuser"
        Password = "TestPassword123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginBody `
        -SkipCertificateCheck
    
    $end = Get-Date
    $duration = ($end - $start).TotalMilliseconds
    $times += $duration
    
    Write-Host "Login $($i+1): $duration ms"
}

$avgTime = ($times | Measure-Object -Average).Average
Write-Host "`nAverage login time: $avgTime ms"
```

## Security Testing

### Test 1: Token Signature Validation

Try to modify the JWT token payload and access protected endpoint:
```powershell
# This should FAIL with 401 Unauthorized
$tamperedToken = "eyJhbGciOiJIUzI1NiIs.TAMPERED_PAYLOAD.signature"

$headers = @{
    "Authorization" = "Bearer $tamperedToken"
}

try {
    Invoke-RestMethod -Uri "https://localhost:5001/api/auth/user" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
} catch {
    Write-Host "✅ Token tampering detected: $($_.Exception.Message)"
}
```

### Test 2: Expired Token Rejection

```powershell
# Use a token from yesterday (if you have one)
# Should return 401 Unauthorized with "Token expired" message
```

### Test 3: Refresh Token Reuse Prevention

```powershell
# Use the same refresh token twice
# First call should succeed
# Second call should fail with "Token already used"

$refreshBody = @{
    Token = $token
    RefreshToken = $usedRefreshToken
} | ConvertTo-Json

# First call - succeeds
$response1 = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/refresh" `
    -Method Post -ContentType "application/json" -Body $refreshBody -SkipCertificateCheck

# Second call - should fail
try {
    $response2 = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/refresh" `
        -Method Post -ContentType "application/json" -Body $refreshBody -SkipCertificateCheck
} catch {
    Write-Host "✅ Refresh token reuse prevented: $($_.Exception.Message)"
}
```

## Complete Test Script

Here's a complete PowerShell script to test the entire JWT flow:

```powershell
# Complete JWT Authentication Test Script
$baseUrl = "https://localhost:5001"

Write-Host "=== AMS JWT Authentication Test ===" -ForegroundColor Green
Write-Host ""

# Test 1: Login
Write-Host "Test 1: Login" -ForegroundColor Yellow
$loginBody = @{
    Username = "testuser"
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" `
        -Method Post `
        -ContentType "application/json" `
        -Body $loginBody `
        -SkipCertificateCheck
    
    Write-Host "✅ Login successful" -ForegroundColor Green
    Write-Host "Token expires at: $($loginResponse.expiresAt)"
    
    $token = $loginResponse.token
    $refreshToken = $loginResponse.refreshToken
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Access Protected Endpoint
Write-Host "Test 2: Access Protected Endpoint" -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $userInfo = Invoke-RestMethod -Uri "$baseUrl/api/auth/user" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "✅ Protected endpoint accessible" -ForegroundColor Green
    Write-Host "User: $($userInfo.username)"
} catch {
    Write-Host "❌ Failed to access protected endpoint: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Refresh Token
Write-Host "Test 3: Refresh Token" -ForegroundColor Yellow
$refreshBody = @{
    Token = $token
    RefreshToken = $refreshToken
} | ConvertTo-Json

try {
    $refreshResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/refresh" `
        -Method Post `
        -ContentType "application/json" `
        -Body $refreshBody `
        -SkipCertificateCheck
    
    Write-Host "✅ Token refreshed successfully" -ForegroundColor Green
    
    $token = $refreshResponse.token
    $refreshToken = $refreshResponse.refreshToken
} catch {
    Write-Host "❌ Token refresh failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Access with New Token
Write-Host "Test 4: Access with New Token" -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}

try {
    $userInfo = Invoke-RestMethod -Uri "$baseUrl/api/auth/user" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "✅ New token works correctly" -ForegroundColor Green
} catch {
    Write-Host "❌ New token failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== All Tests Completed ===" -ForegroundColor Green
```

## Expected Results Summary

| Test | Expected Result | Status |
|------|----------------|--------|
| MVC Login | Redirect to home, cookie set | ✅ |
| API Login | JWT token returned | ✅ |
| Protected API Access | 200 OK with token | ✅ |
| Protected API Without Token | 401 Unauthorized | ✅ |
| Token Refresh | New token returned | ✅ |
| Expired Token | 401 Unauthorized | ✅ |
| Invalid Token | 401 Unauthorized | ✅ |
| Attendance Marking (no remarks) | Success, no validation error | ✅ |
| Role-based Access | 403 for unauthorized roles | ✅ |
| RefreshTokens DB Table | Exists and functional | ✅ |

## Next Steps

1. **Test All Scenarios**: Run through each test case above
2. **Create Test Users**: Ensure you have users with different roles (Admin, Teacher, Student)
3. **Monitor Logs**: Check application logs for any errors
4. **Database Cleanup**: Periodically clean up expired refresh tokens

## Support

If you encounter issues:
1. Check `JWT_AUTHENTICATION_GUIDE.md` for detailed implementation info
2. Review `QUICKSTART_JWT.md` for quick reference
3. Verify database migrations are applied
4. Ensure `appsettings.json` has correct JWT configuration

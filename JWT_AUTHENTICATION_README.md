# JWT Authentication Implementation for AMS

## Overview
This document describes the JWT (JSON Web Token) authentication implementation in the Attendance Management System (AMS).

## Features Implemented

### 1. JWT Token Generation and Validation
- **JwtService**: Service for generating and validating JWT tokens
- **RefreshToken**: Support for refresh tokens to extend authentication sessions
- **Token Expiry**: Configurable token expiry times

### 2. API Endpoints

#### Authentication Endpoints (`/api/auth`)

##### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "Email": "user@example.com",
  "Password": "password123"
}
```

**Response:**
```json
{
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "RefreshToken": "random_refresh_token_string",
  "Success": true,
  "UserId": "user-id",
  "Email": "user@example.com",
  "Roles": ["Student"]
}
```

##### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "Token": "expired_jwt_token",
  "RefreshToken": "valid_refresh_token"
}
```

##### Get Current User Info
```http
GET /api/auth/me
Authorization: Bearer {token}
```

##### Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "Email": "newuser@example.com",
  "Password": "password123",
  "FullName": "John Doe",
  "Role": "Student"
}
```

### 3. Protected API Controllers

#### Courses API (`/api/courses`)
- `GET /api/courses` - Get all courses (Authenticated)
- `GET /api/courses/{id}` - Get course by ID (Authenticated)
- `POST /api/courses` - Create course (Admin/Teacher only)
- `PUT /api/courses/{id}` - Update course (Admin/Teacher only)
- `DELETE /api/courses/{id}` - Delete course (Admin only)
- `GET /api/courses/my-courses` - Get student's enrolled courses (Student only)

#### Attendance API (`/api/attendance`)
- `GET /api/attendance/student/{studentId}` - Get student attendance
- `GET /api/attendance/course/{courseId}` - Get course attendance (Teacher/Admin)
- `POST /api/attendance` - Mark attendance (Teacher only)
- `POST /api/attendance/bulk` - Bulk mark attendance (Teacher only)
- `PUT /api/attendance/{id}` - Update attendance (Teacher/Admin)
- `GET /api/attendance/statistics/{studentId}` - Get attendance statistics

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Key": "Your-Secret-Key-Must-Be-At-Least-32-Characters-Long",
    "Issuer": "AMSApplication",
    "Audience": "AMSUsers",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Program.cs Setup
The JWT authentication is configured in `Program.cs` with:
- Token validation parameters
- JWT Bearer authentication
- Cookie authentication (for web UI)
- Dual authentication support

## Database Changes

### New Table: RefreshTokens
```sql
CREATE TABLE RefreshTokens (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(450) NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    JwtId NVARCHAR(MAX) NOT NULL,
    IsUsed BIT NOT NULL,
    IsRevoked BIT NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
```

## Usage Examples

### JavaScript/Frontend Example
```javascript
// Login
async function login(email, password) {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ Email: email, Password: password })
  });
  
  const data = await response.json();
  if (data.Success) {
    localStorage.setItem('token', data.Token);
    localStorage.setItem('refreshToken', data.RefreshToken);
    return data;
  }
  throw new Error(data.Errors.join(', '));
}

// Make authenticated request
async function getCourses() {
  const token = localStorage.getItem('token');
  const response = await fetch('/api/courses', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  if (response.status === 401) {
    // Token expired, try refresh
    await refreshToken();
    return getCourses(); // Retry
  }
  
  return await response.json();
}

// Refresh token
async function refreshToken() {
  const token = localStorage.getItem('token');
  const refreshToken = localStorage.getItem('refreshToken');
  
  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ Token: token, RefreshToken: refreshToken })
  });
  
  const data = await response.json();
  if (data.Success) {
    localStorage.setItem('token', data.Token);
    localStorage.setItem('refreshToken', data.RefreshToken);
  } else {
    // Refresh failed, redirect to login
    window.location.href = '/Account/Login';
  }
}
```

### C# Client Example
```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class AmsApiClient
{
    private readonly HttpClient _httpClient;
    private string? _token;

    public AmsApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var request = new { Email = email, Password = password };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/auth/login", content);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResult>(result);
            _token = authResult?.Token;
            return true;
        }
        return false;
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _token);

        var response = await _httpClient.GetAsync("/api/courses");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Course>>(json) ?? new List<Course>();
    }
}
```

## Security Considerations

1. **Token Storage**: Store tokens securely (HttpOnly cookies for web, secure storage for mobile)
2. **HTTPS**: Always use HTTPS in production
3. **Token Expiry**: Configure appropriate token expiry times
4. **Refresh Tokens**: Implement proper refresh token rotation
5. **Role-Based Access**: Use `[Authorize(Roles = "Admin,Teacher")]` attributes
6. **Token Revocation**: Mark refresh tokens as revoked when needed

## Migration Steps

### 1. Restore NuGet Packages
```powershell
dotnet restore
```

### 2. Add Migration for RefreshTokens
```powershell
dotnet ef migrations add AddRefreshTokens
```

### 3. Update Database
```powershell
dotnet ef database update
```

### 4. Build Project
```powershell
dotnet build
```

### 5. Run Project
```powershell
dotnet run
```

## Testing the API

### Using Postman or curl

#### 1. Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"Email":"admin@ams.com","Password":"Admin123"}'
```

#### 2. Get Courses (with token)
```bash
curl -X GET https://localhost:5001/api/courses \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 3. Mark Attendance
```bash
curl -X POST https://localhost:5001/api/attendance \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "StudentId": 1,
    "CourseId": 1,
    "Date": "2024-01-15T10:00:00",
    "IsPresent": true,
    "Remarks": "Present"
  }'
```

## Troubleshooting

### Common Issues

1. **401 Unauthorized**: Check if token is included in Authorization header
2. **Token expired**: Use refresh token endpoint to get new token
3. **Invalid token**: Ensure JWT Key matches in appsettings.json
4. **Role access denied**: Verify user has required role

## Additional Resources

- [JWT.io](https://jwt.io/) - JWT debugger
- [Microsoft JWT Authentication Docs](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)

## Future Enhancements

- [ ] Add email verification
- [ ] Implement password reset via JWT
- [ ] Add two-factor authentication (2FA)
- [ ] Implement token blacklisting
- [ ] Add API rate limiting
- [ ] Add API versioning
- [ ] Add Swagger/OpenAPI documentation

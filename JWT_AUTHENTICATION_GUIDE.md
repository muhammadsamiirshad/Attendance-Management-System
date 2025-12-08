# AMS (Attendance Management System) - JWT Authentication Guide

## Overview
This document provides comprehensive information about JWT (JSON Web Token) authentication implementation in the AMS application.

## Table of Contents
1. [Architecture](#architecture)
2. [Configuration](#configuration)
3. [Authentication Flow](#authentication-flow)
4. [API Endpoints](#api-endpoints)
5. [Usage Examples](#usage-examples)
6. [Security Best Practices](#security-best-practices)
7. [Troubleshooting](#troubleshooting)

---

## Architecture

### Components
- **JwtSettings**: Configuration model for JWT parameters
- **JwtService**: Service for generating and validating JWT tokens
- **AuthController**: API controller for authentication endpoints
- **Token Validation**: Middleware for validating JWT tokens on protected routes

### Authentication Types
The application supports **hybrid authentication**:
- **Cookie-based**: For MVC views (traditional web pages)
- **JWT-based**: For API endpoints

---

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Key": "AMS_SecureKey_2024_MustBe32CharactersOrMore_ForJWT",
    "Issuer": "AMSApplication",
    "Audience": "AMSUsers",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Configuration Parameters
- **Key**: Secret key for signing tokens (minimum 32 characters)
- **Issuer**: Token issuer identifier
- **Audience**: Intended audience for the token
- **ExpiryMinutes**: Access token expiration time (default: 60 minutes)
- **RefreshTokenExpiryDays**: Refresh token expiration time (default: 7 days)

---

## Authentication Flow

### 1. Login Flow
```
Client → POST /api/auth/login → AuthController
  ↓
Validate credentials (UserManager)
  ↓
Generate JWT + Refresh Token (JwtService)
  ↓
Store Refresh Token in database
  ↓
Return tokens to client
```

### 2. Token Structure
**Access Token (JWT) contains:**
- User ID
- Email
- Full Name
- Role(s)
- Expiration time
- Signature

**Refresh Token:**
- Random string stored in database
- Used to obtain new access token
- Longer expiration time

### 3. Token Refresh Flow
```
Client → POST /api/auth/refresh → AuthController
  ↓
Validate old access token format
  ↓
Verify refresh token in database
  ↓
Generate new JWT + Refresh Token
  ↓
Mark old refresh token as used
  ↓
Return new tokens to client
```

---

## API Endpoints

### Authentication Endpoints

#### 1. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "random_base64_string",
  "success": true,
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Student"]
}
```

#### 2. Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "token": "expired_jwt_token",
  "refreshToken": "stored_refresh_token"
}
```

**Response (200 OK):**
```json
{
  "token": "new_jwt_token",
  "refreshToken": "new_refresh_token",
  "success": true,
  "userId": "user-id-guid",
  "email": "user@example.com",
  "roles": ["Student"]
}
```

#### 3. Get Current User
```http
GET /api/auth/me
Authorization: Bearer {jwt_token}
```

**Response (200 OK):**
```json
{
  "userId": "user-id-guid",
  "email": "user@example.com",
  "fullName": "John Doe",
  "roles": ["Student"]
}
```

#### 4. Register (Optional - may be restricted)
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "Password123!",
  "fullName": "New User",
  "role": "Student"
}
```

---

### Resource API Endpoints

All resource endpoints require JWT authentication via `Authorization: Bearer {token}` header.

#### Students API

| Method | Endpoint | Roles | Description |
|--------|----------|-------|-------------|
| GET | `/api/studentsapi` | Admin, Teacher | Get all students |
| GET | `/api/studentsapi/{id}` | All (own data) | Get student by ID |
| GET | `/api/studentsapi/me` | Student | Get own profile |
| GET | `/api/studentsapi/{id}/courses` | All (own data) | Get student courses |
| GET | `/api/studentsapi/available-courses` | Student | Get available courses |
| POST | `/api/studentsapi/{id}/courses/{courseId}` | Student | Register for course |
| PUT | `/api/studentsapi/{id}` | Student (own), Admin | Update student |
| DELETE | `/api/studentsapi/{id}` | Admin | Delete student |

#### Courses API

| Method | Endpoint | Roles | Description |
|--------|----------|-------|-------------|
| GET | `/api/coursesapi` | All | Get all courses |
| GET | `/api/coursesapi/{id}` | All | Get course by ID |
| GET | `/api/coursesapi/{id}/students` | Admin, Teacher | Get course students |
| GET | `/api/coursesapi/search?query={query}` | All | Search courses |
| POST | `/api/coursesapi` | Admin | Create course |
| PUT | `/api/coursesapi/{id}` | Admin | Update course |
| DELETE | `/api/coursesapi/{id}` | Admin | Delete course |

#### Attendance API

| Method | Endpoint | Roles | Description |
|--------|----------|-------|-------------|
| GET | `/api/attendanceapi/session/{sessionId}` | Admin, Teacher | Get session attendance |
| GET | `/api/attendanceapi/student/{studentId}` | All (own data) | Get student attendance |
| GET | `/api/attendanceapi/me` | Student | Get own attendance |
| GET | `/api/attendanceapi/course/{courseId}` | Admin, Teacher | Get course attendance |
| POST | `/api/attendanceapi/mark` | Teacher | Mark attendance |
| PUT | `/api/attendanceapi/{id}` | Teacher, Admin | Update attendance |
| DELETE | `/api/attendanceapi/{id}` | Admin | Delete attendance |

#### Timetable API

| Method | Endpoint | Roles | Description |
|--------|----------|-------|-------------|
| GET | `/api/timetableapi` | Admin | Get all timetables |
| GET | `/api/timetableapi/{id}` | All | Get timetable by ID |
| GET | `/api/timetableapi/teacher/{teacherId}` | All (own data) | Get teacher timetable |
| GET | `/api/timetableapi/teacher/me` | Teacher | Get own timetable |
| GET | `/api/timetableapi/course/{courseId}` | All | Get course timetable |
| GET | `/api/timetableapi/student/{studentId}` | All (own data) | Get student timetable |
| GET | `/api/timetableapi/student/me` | Student | Get own timetable |
| GET | `/api/timetableapi/day/{dayOfWeek}` | All | Get timetable by day |
| POST | `/api/timetableapi` | Admin | Create timetable |
| PUT | `/api/timetableapi/{id}` | Admin | Update timetable |
| DELETE | `/api/timetableapi/{id}` | Admin | Delete timetable |

---

## Usage Examples

### JavaScript/TypeScript (Fetch API)

#### Login and Store Token
```javascript
async function login(email, password) {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });

  if (response.ok) {
    const data = await response.json();
    // Store tokens securely
    localStorage.setItem('accessToken', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else {
    throw new Error('Login failed');
  }
}
```

#### Make Authenticated Request
```javascript
async function getMyProfile() {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch('/api/studentsapi/me', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });

  if (response.ok) {
    return await response.json();
  } else if (response.status === 401) {
    // Token expired, try to refresh
    await refreshToken();
    return getMyProfile(); // Retry
  } else {
    throw new Error('Failed to get profile');
  }
}
```

#### Refresh Token
```javascript
async function refreshToken() {
  const token = localStorage.getItem('accessToken');
  const refreshToken = localStorage.getItem('refreshToken');

  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ token, refreshToken })
  });

  if (response.ok) {
    const data = await response.json();
    localStorage.setItem('accessToken', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else {
    // Refresh failed, logout user
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    window.location.href = '/Account/Login';
    throw new Error('Session expired');
  }
}
```

### C# (HttpClient)

```csharp
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

public class AmsApiClient
{
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private string _refreshToken;

    public AmsApiClient(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var loginRequest = new { Email = email, Password = password };
        var content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("/api/auth/login", content);
        if (response.IsSuccessStatusCode)
        {
            var result = await JsonSerializer.DeserializeAsync<AuthResult>(
                await response.Content.ReadAsStreamAsync()
            );
            _accessToken = result.Token;
            _refreshToken = result.RefreshToken;
            return true;
        }
        return false;
    }

    public async Task<Student> GetMyProfileAsync()
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _httpClient.GetAsync("/api/studentsapi/me");
        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<Student>(
            await response.Content.ReadAsStreamAsync()
        );
    }
}
```

### Python (Requests)

```python
import requests

class AMSClient:
    def __init__(self, base_url):
        self.base_url = base_url
        self.access_token = None
        self.refresh_token = None

    def login(self, email, password):
        response = requests.post(
            f"{self.base_url}/api/auth/login",
            json={"email": email, "password": password}
        )
        if response.status_code == 200:
            data = response.json()
            self.access_token = data["token"]
            self.refresh_token = data["refreshToken"]
            return True
        return False

    def get_my_profile(self):
        headers = {"Authorization": f"Bearer {self.access_token}"}
        response = requests.get(
            f"{self.base_url}/api/studentsapi/me",
            headers=headers
        )
        return response.json() if response.status_code == 200 else None

# Usage
client = AMSClient("https://localhost:5001")
client.login("student@ams.edu", "password123")
profile = client.get_my_profile()
print(profile)
```

---

## Security Best Practices

### 1. Token Storage
- **Web Apps**: Store in memory or httpOnly cookies (not localStorage for high security)
- **Mobile Apps**: Use secure storage (Keychain on iOS, KeyStore on Android)
- **Desktop Apps**: Use OS-provided credential storage

### 2. Token Transmission
- Always use HTTPS in production
- Include token in `Authorization: Bearer {token}` header
- Never pass tokens in URL parameters

### 3. Token Validation
- Validate on every request
- Check expiration time
- Verify signature
- Check issuer and audience

### 4. Key Management
- Use strong, randomly generated keys (minimum 32 characters)
- Store keys securely (environment variables, Azure Key Vault, etc.)
- Rotate keys periodically
- Never commit keys to source control

### 5. Error Handling
- Don't expose sensitive information in error messages
- Log authentication failures for monitoring
- Implement rate limiting to prevent brute force attacks

### 6. Token Expiration
- Use short-lived access tokens (15-60 minutes)
- Use longer-lived refresh tokens (7-30 days)
- Implement token revocation for logout

---

## Troubleshooting

### Common Issues

#### 1. "401 Unauthorized" Response
**Causes:**
- Token expired
- Invalid token signature
- Token not included in request
- User not authenticated

**Solutions:**
- Check if token is present in Authorization header
- Verify token format: `Bearer {token}`
- Try refreshing the token
- Re-authenticate if refresh fails

#### 2. "403 Forbidden" Response
**Causes:**
- User doesn't have required role
- User trying to access another user's resources

**Solutions:**
- Check user roles in token claims
- Verify endpoint role requirements
- Ensure user has proper permissions

#### 3. Token Refresh Fails
**Causes:**
- Refresh token expired
- Refresh token already used
- Refresh token revoked
- Token mismatch

**Solutions:**
- Check refresh token expiration
- Ensure refresh token is stored correctly
- Force user to re-login

#### 4. "Invalid token" Error
**Causes:**
- Malformed token
- Token from different issuer
- Token signature verification failed

**Solutions:**
- Verify token format (should have 3 parts separated by dots)
- Check JWT configuration (Key, Issuer, Audience)
- Ensure client and server use same key

### Debugging Tools

#### 1. JWT Decoder
Visit [jwt.io](https://jwt.io) to decode and inspect JWT tokens

#### 2. Postman/Insomnia
Use API testing tools to test endpoints:
```
1. POST /api/auth/login → Get token
2. Copy token from response
3. Add to Authorization header: Bearer {token}
4. Test protected endpoints
```

#### 3. Browser DevTools
- Network tab: Inspect request/response headers
- Console: Check for JavaScript errors
- Application tab: View localStorage/sessionStorage

---

## Testing

### Unit Tests Example

```csharp
[Test]
public async Task Login_WithValidCredentials_ReturnsToken()
{
    // Arrange
    var loginRequest = new TokenRequest
    {
        Email = "test@example.com",
        Password = "Test123!"
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadFromJsonAsync<AuthResult>();
    result.Success.Should().BeTrue();
    result.Token.Should().NotBeNullOrEmpty();
}
```

### Integration Tests

```csharp
[Test]
public async Task GetMyProfile_WithValidToken_ReturnsProfile()
{
    // Arrange
    var token = await LoginAndGetTokenAsync();
    _client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await _client.GetAsync("/api/studentsapi/me");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var profile = await response.Content.ReadFromJsonAsync<Student>();
    profile.Should().NotBeNull();
}
```

---

## Additional Resources

- [JWT.io - JWT Debugger](https://jwt.io)
- [RFC 7519 - JSON Web Token](https://tools.ietf.org/html/rfc7519)
- [OWASP JWT Security Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
- [Microsoft ASP.NET Core JWT Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/jwt)

---

## Support

For issues or questions:
1. Check this documentation
2. Review error logs
3. Test with Postman/similar tools
4. Contact development team

---

**Last Updated:** 2024
**Version:** 1.0

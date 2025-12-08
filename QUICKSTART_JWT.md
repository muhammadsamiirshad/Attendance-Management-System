# AMS JWT Authentication - Quick Start Guide

## Getting Started with JWT Authentication

### 1. Build and Run the Application

```powershell
# Navigate to project directory
cd c:\Users\Administrator\Desktop\EAD\AMS\AMS

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The application will start at:
- **Web App**: `https://localhost:5001` or `http://localhost:5000`
- **API Swagger**: `https://localhost:5001/api-docs`

---

## 2. Testing JWT Authentication with Swagger

### Access Swagger UI
1. Navigate to `https://localhost:5001/api-docs`
2. You'll see all available API endpoints with documentation

### Authenticate and Get Token
1. Find the **POST /api/auth/login** endpoint
2. Click "Try it out"
3. Enter credentials:
```json
{
  "email": "admin@ams.edu",
  "password": "Admin123!"
}
```
4. Click "Execute"
5. Copy the `token` from the response

### Use Token in Swagger
1. Click the **"Authorize"** button at the top
2. Enter: `Bearer {your_token_here}`
3. Click "Authorize"
4. Now you can test all protected endpoints

---

## 3. Testing with Postman

### Step 1: Login
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@ams.edu",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64_encoded_string",
  "success": true,
  "userId": "user-guid",
  "email": "admin@ams.edu",
  "roles": ["Admin"]
}
```

### Step 2: Use Token in Requests
Add to Headers:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json
```

### Step 3: Test Endpoints

#### Get All Students
```http
GET https://localhost:5001/api/studentsapi
Authorization: Bearer {your_token}
```

#### Get My Profile (Student)
```http
GET https://localhost:5001/api/studentsapi/me
Authorization: Bearer {student_token}
```

#### Get All Courses
```http
GET https://localhost:5001/api/coursesapi
Authorization: Bearer {your_token}
```

#### Register for a Course
```http
POST https://localhost:5001/api/studentsapi/{studentId}/courses/{courseId}
Authorization: Bearer {student_token}
```

---

## 4. Testing with cURL (PowerShell)

### Login
```powershell
$loginData = @{
    email = "admin@ams.edu"
    password = "Admin123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body $loginData `
    -SkipCertificateCheck

$token = $response.token
Write-Host "Token: $token"
```

### Use Token to Get Students
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$students = Invoke-RestMethod -Uri "https://localhost:5001/api/studentsapi" `
    -Method GET `
    -Headers $headers `
    -SkipCertificateCheck

$students | ConvertTo-Json
```

---

## 5. Default Test Accounts

After database seeding, these accounts are available:

### Admin Account
```
Email: admin@ams.edu
Password: Admin123!
Role: Admin
```

### Teacher Account
```
Email: teacher@ams.edu
Password: Teacher123!
Role: Teacher
```

### Student Account
```
Email: student@ams.edu
Password: Student123!
Role: Student
```

---

## 6. API Endpoint Summary

### Authentication
- **POST** `/api/auth/login` - Login and get JWT token
- **POST** `/api/auth/refresh` - Refresh expired token
- **GET** `/api/auth/me` - Get current user info
- **POST** `/api/auth/register` - Register new user

### Students (Requires Authentication)
- **GET** `/api/studentsapi` - Get all students (Admin/Teacher)
- **GET** `/api/studentsapi/me` - Get own profile (Student)
- **GET** `/api/studentsapi/{id}` - Get student by ID
- **GET** `/api/studentsapi/{id}/courses` - Get student courses
- **GET** `/api/studentsapi/available-courses` - Get available courses (Student)
- **POST** `/api/studentsapi/{id}/courses/{courseId}` - Register for course (Student)
- **PUT** `/api/studentsapi/{id}` - Update student
- **DELETE** `/api/studentsapi/{id}` - Delete student (Admin)

### Courses (Requires Authentication)
- **GET** `/api/coursesapi` - Get all courses
- **GET** `/api/coursesapi/{id}` - Get course by ID
- **GET** `/api/coursesapi/{id}/students` - Get course students (Admin/Teacher)
- **GET** `/api/coursesapi/search?query={query}` - Search courses
- **POST** `/api/coursesapi` - Create course (Admin)
- **PUT** `/api/coursesapi/{id}` - Update course (Admin)
- **DELETE** `/api/coursesapi/{id}` - Delete course (Admin)

### Attendance (Requires Authentication)
- **GET** `/api/attendanceapi/me` - Get own attendance (Student)
- **GET** `/api/attendanceapi/student/{id}` - Get student attendance
- **GET** `/api/attendanceapi/session/{sessionId}` - Get session attendance (Teacher/Admin)
- **GET** `/api/attendanceapi/course/{courseId}` - Get course attendance (Teacher/Admin)
- **POST** `/api/attendanceapi/mark` - Mark attendance (Teacher)
- **PUT** `/api/attendanceapi/{id}` - Update attendance (Teacher/Admin)
- **DELETE** `/api/attendanceapi/{id}` - Delete attendance (Admin)

### Timetable (Requires Authentication)
- **GET** `/api/timetableapi` - Get all timetables (Admin)
- **GET** `/api/timetableapi/{id}` - Get timetable by ID
- **GET** `/api/timetableapi/teacher/me` - Get own timetable (Teacher)
- **GET** `/api/timetableapi/student/me` - Get own timetable (Student)
- **GET** `/api/timetableapi/course/{courseId}` - Get course timetable
- **GET** `/api/timetableapi/day/{dayOfWeek}` - Get timetable by day
- **POST** `/api/timetableapi` - Create timetable (Admin)
- **PUT** `/api/timetableapi/{id}` - Update timetable (Admin)
- **DELETE** `/api/timetableapi/{id}` - Delete timetable (Admin)

---

## 7. Common Tasks

### Task 1: Student Registers for a Course
```javascript
// 1. Login as student
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'student@ams.edu',
    password: 'Student123!'
  })
});
const { token } = await loginResponse.json();

// 2. Get available courses
const coursesResponse = await fetch('/api/studentsapi/available-courses', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const courses = await coursesResponse.json();

// 3. Register for a course
const registerResponse = await fetch(`/api/studentsapi/{studentId}/courses/{courseId}`, {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` }
});
```

### Task 2: Teacher Marks Attendance
```javascript
// 1. Login as teacher
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'teacher@ams.edu',
    password: 'Teacher123!'
  })
});
const { token } = await loginResponse.json();

// 2. Mark attendance
const markResponse = await fetch('/api/attendanceapi/mark', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    sessionId: 1,
    attendanceRecords: [
      { studentId: 1, isPresent: true, remarks: null },
      { studentId: 2, isPresent: false, remarks: "Sick leave" }
    ]
  })
});
```

### Task 3: Admin Creates a Course
```javascript
// 1. Login as admin
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'admin@ams.edu',
    password: 'Admin123!'
  })
});
const { token } = await loginResponse.json();

// 2. Create course
const createResponse = await fetch('/api/coursesapi', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    courseName: "Web Development",
    courseCode: "CS301",
    credits: 3,
    description: "Introduction to web development"
  })
});
```

---

## 8. Token Management

### Check Token Expiration
```javascript
function isTokenExpired(token) {
  const payload = JSON.parse(atob(token.split('.')[1]));
  return Date.now() >= payload.exp * 1000;
}
```

### Auto-Refresh Token
```javascript
async function makeAuthenticatedRequest(url, options = {}) {
  let token = localStorage.getItem('accessToken');
  
  // Check if token is expired
  if (isTokenExpired(token)) {
    const refreshToken = localStorage.getItem('refreshToken');
    const refreshResponse = await fetch('/api/auth/refresh', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token, refreshToken })
    });
    
    if (refreshResponse.ok) {
      const data = await refreshResponse.json();
      token = data.token;
      localStorage.setItem('accessToken', data.token);
      localStorage.setItem('refreshToken', data.refreshToken);
    } else {
      // Redirect to login
      window.location.href = '/Account/Login';
      return;
    }
  }
  
  // Make the request with fresh token
  return fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${token}`
    }
  });
}
```

---

## 9. Security Checklist

- [ ] JWT tokens are transmitted over HTTPS only
- [ ] Tokens are stored securely (httpOnly cookies or secure storage)
- [ ] Token expiration is set appropriately (60 minutes recommended)
- [ ] Refresh tokens have longer expiration (7 days)
- [ ] Sensitive endpoints require proper role authorization
- [ ] JWT secret key is strong and stored securely (not in source code)
- [ ] API rate limiting is implemented
- [ ] CORS is properly configured for API access

---

## 10. Troubleshooting

### "401 Unauthorized" Error
- Check if token is included in Authorization header
- Verify token format: `Bearer {token}`
- Check if token is expired
- Try refreshing the token

### "403 Forbidden" Error
- Check user role permissions
- Verify endpoint role requirements
- Ensure user has necessary claims

### Token Not Working in Swagger
- Click "Authorize" button
- Enter: `Bearer {token}` (with space after "Bearer")
- Click "Authorize" to save
- Try the request again

---

## Support

For detailed documentation, see: `JWT_AUTHENTICATION_GUIDE.md`

For issues:
1. Check application logs
2. Test with Postman/Swagger
3. Verify database connection
4. Review error messages in response

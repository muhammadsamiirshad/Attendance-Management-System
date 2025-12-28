# Attendance Management System (AMS)

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-green.svg)](https://asp.net/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-8.0-orange.svg)](https://docs.microsoft.com/en-us/ef/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-purple.svg)](https://getbootstrap.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red.svg)](https://www.microsoft.com/en-us/sql-server)

A comprehensive, professional web-based Attendance Management System built with ASP.NET Core 8.0 MVC. This system provides robust attendance tracking with time-based enforcement, section-wise management, role-based access control, and real-time reporting capabilities.

---

## 🆕 Latest Feature: Auto-Select Teacher in Timetable Management

**NEW**: When creating or editing timetables, the system now automatically selects the appropriate teacher based on the selected course! This feature:
- 🎯 Auto-populates the teacher dropdown when a course is selected
- ✅ Shows real-time feedback with success/warning messages
- 🔄 Updates dynamically when course selection changes
- 💡 Reduces manual work and prevents assignment errors

See [AUTO_SELECT_TEACHER_FEATURE.md](AUTO_SELECT_TEACHER_FEATURE.md) for detailed documentation and [AUTO_SELECT_TEACHER_TEST_GUIDE.md](AUTO_SELECT_TEACHER_TEST_GUIDE.md) for testing instructions.

---

## 📋 Table of Contents

- [Key Features](#-key-features)
- [System Architecture](#-system-architecture)
- [Technology Stack](#-technology-stack)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Database Setup](#-database-setup)
- [Configuration](#-configuration)
- [User Roles](#-user-roles)
- [Key Functionalities](#-key-functionalities)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Security Features](#-security-features)
- [Usage Guide](#-usage-guide)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)

---

## ✨ Key Features

### 🔐 Authentication & Authorization
- **Hybrid Authentication**: ASP.NET Core Identity (Cookie-based) + JWT Bearer tokens for API
- **Role-Based Access Control**: Three distinct roles - Admin, Teacher, and Student
- **Secure Password Management**: 
  - Show/hide password toggle on all password fields (Login, Change Password, Create User, Reset Password)
  - Forced password change on first login
  - Minimum 6 characters with digit requirement
- **JWT Token Management**: Secure API access with refresh token support (7-day expiry)

### 📊 Attendance Management
- **⏰ Time-Window Enforcement**: 
  - Attendance can ONLY be marked from lecture start time to 10 minutes after
  - Cannot mark attendance before lecture starts
  - Clear status messages: "Window opens in X minutes" or "Window closed X minutes ago"
  - Real-time countdown timer showing remaining time
- **👥 Section-Wise Marking**: 
  - Teachers can filter and mark attendance for specific sections
  - Option to mark all sections at once
  - Dynamic student list based on section selection
- **Real-Time Validation**: 
  - Client-side and server-side validation
  - Prevents duplicate attendance records for the same day
  - Automatic status tracking (Present/Absent)
- **Optional Remarks**: Add contextual notes for individual students (completely optional field)
- **Student Names Display**: Shows full student names (not just IDs) in attendance lists

### 📅 Timetable Management
- **Dynamic Scheduling**: Create and manage class schedules with:
  - Day of week (Sunday-Saturday)
  - Start time and end time
  - Classroom assignment
  - Teacher assignment
- **Student-Specific Timetables**: Students see only their enrolled courses in their timetable
- **Conflict Detection**: Prevent scheduling conflicts for teachers and classrooms
- **Flexible Management**: Create, edit, and delete timetable entries

### 📈 Comprehensive Reporting
- **Admin Reports**: System-wide attendance statistics and analytics
- **Teacher Reports**: Course-specific reports with section filtering
- **Student Reports**: Personal attendance records with percentage tracking
- **Date Range Filtering**: Custom date range selection for all reports
- **Real-Time Data**: All reports pull live data from the database

### 👥 User Management
- **Admin Dashboard**: Complete user, course, section, and session management
- **Bulk Operations**: Assign multiple students to sections/courses efficiently
- **Profile Management**: Users can update information and change passwords
- **Password Reset**: Admins can reset any user's password

---

## 🏗 System Architecture

### Architecture Pattern
- **MVC (Model-View-Controller)**: Clean separation of concerns
- **Repository Pattern**: Data access abstraction layer
- **Service Layer**: Business logic encapsulation
- **Dependency Injection**: Loose coupling and improved testability

### Database Design

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│    AppUser      │────────>│     Student     │────────>│   Attendance    │
│   (Identity)    │         │                 │         │                 │
└─────────────────┘         └─────────────────┘         └─────────────────┘
        │                            │                            │
        │                            │                            │
        ▼                            ▼                            ▼
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│     Teacher     │────────>│ CourseAssignment│────────>│     Course      │
│                 │         │                 │         │                 │
└─────────────────┘         └─────────────────┘         └─────────────────┘
                                     │                            │
                                     │                            │
                                     ▼                            ▼
                            ┌─────────────────┐         ┌─────────────────┐
                            │   Timetable     │────────>│     Section     │
                            │                 │         │                 │
                            └─────────────────┘         └─────────────────┘
                                                                 │
                                                                 │
                                                                 ▼
                                                        ┌─────────────────┐
                                                        │ SessionSection  │
                                                        │                 │
                                                        └─────────────────┘
                                                                 │
                                                                 │
                                                                 ▼
                                                        ┌─────────────────┐
                                                        │     Session     │
                                                        │  (Acad. Year)   │
                                                        └─────────────────┘
```

**Key Entities:**

- **AppUser**: Base ASP.NET Core Identity user (extended by Student & Teacher)
- **Student**: Student profile with enrollment data, phone, and email
- **Teacher**: Teacher profile with department, hire date, and contact info
- **Course**: Course details (code, name, credits, department)
- **Section**: Class sections (e.g., Section A, B, C)
- **Session**: Academic year/semester management with start and end dates
- **Attendance**: Individual attendance records with date, status, and optional remarks
- **Timetable**: Class schedule with day, time, classroom, teacher, and section
- **CourseAssignment**: Many-to-many relationship between teachers and courses
- **StudentCourseRegistration**: Course enrollment for students
- **StudentSection**: Student section assignments
- **SessionSection**: Links sections to academic sessions
- **RefreshToken**: JWT refresh token management for API security

---

## 🛠 Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 MVC
- **ORM**: Entity Framework Core 8.0 with Code-First approach
- **Database**: SQL Server 2019+ or SQL Server LocalDB
- **Authentication**: 
  - ASP.NET Core Identity (Cookie-based for web)
  - JWT Bearer tokens (for API endpoints)
- **API**: RESTful Web API with Swagger/OpenAPI documentation
- **Architecture**: Repository Pattern + Service Layer + Dependency Injection

### Frontend
- **Template Engine**: Razor Views (.cshtml)
- **CSS Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons 1.7.2+
- **JavaScript**: Vanilla JavaScript with jQuery 3.x
- **AJAX**: For dynamic content loading and real-time updates
- **Validation**: jQuery Validation + Unobtrusive Validation

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

### Development Tools
- **IDE**: Visual Studio 2022 (recommended) or Visual Studio Code
- **Version Control**: Git
- **Package Manager**: NuGet
- **Migration Tool**: Entity Framework Core CLI
- **API Testing**: Swagger UI (built-in) or Postman

---

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

### Required
- **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** or later
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** (recommended) or [VS Code](https://code.visualstudio.com/)
- **[SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** or SQL Server LocalDB
- **[Git](https://git-scm.com/)** for version control

### Optional
- **[SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** for database management
- **[Postman](https://www.postman.com/)** for API testing

---

## 🚀 Installation

### Step 1: Clone the Repository
```bash
git clone https://github.com/yourusername/AMS.git
cd AMS/AMS
```

### Step 2: Restore NuGet Packages
```bash
dotnet restore
```

### Step 3: Update Connection String
Edit `appsettings.json` and update the connection string:

**For SQL Server LocalDB (default):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AMSDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**For SQL Server with Windows Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AMSDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

**For SQL Server with SQL Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AMSDb;User Id=YOUR_USER;Password=YOUR_PASS;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### Step 4: Configure JWT Settings
JWT settings are pre-configured in `appsettings.json`:

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

**⚠️ Security Important**: 
- Change the JWT Key for production deployments
- Use environment variables or Azure Key Vault for production
- Never commit sensitive keys to version control

---

## 💾 Database Setup

### Step 1: Apply Migrations
```bash
dotnet ef database update
```

This will:
- Create the `AMSDb` database
- Generate all required tables
- Apply relationship configurations

### Step 2: Automatic Data Seeding
The application automatically seeds on first run:

**Default Admin User:**
- **Email**: `admin@ams.com`
- **Password**: `Admin123!`
- **Role**: Admin

**Default Roles:**
- Admin
- Teacher
- Student

**⚠️ Important**: Change the default admin password immediately after first login!

### EF Core Migration Commands

```bash
# List all migrations
dotnet ef migrations list

# Add a new migration
dotnet ef migrations add MigrationName

# Remove the last migration
dotnet ef migrations remove

# Update to a specific migration
dotnet ef database update MigrationName

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

---

## ⚙ Configuration

### Application Settings

#### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AMSDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "AMS_SecureKey_2024_MustBe32CharactersOrMore_ForJWT",
    "Issuer": "AMSApplication",
    "Audience": "AMSUsers",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Identity Configuration

Password requirements (configured in `Program.cs`):
```csharp
options.Password.RequireDigit = true;
options.Password.RequiredLength = 6;
options.Password.RequireNonAlphanumeric = false;
options.Password.RequireUppercase = false;
options.Password.RequireLowercase = false;
options.User.RequireUniqueEmail = true;
options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
options.Lockout.MaxFailedAccessAttempts = 5;
```

---

## 👥 User Roles

### 1. 🔧 Admin Role

**Capabilities:**
- ✅ Create, edit, and delete students and teachers
- ✅ Manage courses, sections, and sessions
- ✅ Create and manage timetables
- ✅ Assign teachers to courses
- ✅ Assign students to sections and courses
- ✅ View all attendance records
- ✅ Generate comprehensive reports
- ✅ Reset user passwords
- ❌ Cannot directly mark attendance

**Default Credentials:**
- Email: `admin@ams.com`
- Password: `Admin123!`

---

### 2. 👨‍🏫 Teacher Role

**Capabilities:**
- ✅ Mark attendance (within time window)
- ✅ View assigned courses and timetable
- ✅ Filter students by section
- ✅ Add optional remarks for students
- ✅ View attendance reports for assigned courses
- ✅ Change password and update profile
- ❌ Cannot access admin functions

---

### 3. 🎓 Student Role

**Capabilities:**
- ✅ View enrolled courses
- ✅ View personal timetable (enrolled courses only)
- ✅ View attendance records and percentages
- ✅ Register for courses
- ✅ Change password and update profile
- ❌ Cannot mark attendance
- ❌ Cannot view other students' data

---

## 🎯 Key Functionalities

### 1. ⏰ Attendance Marking System

#### Time-Based Window Enforcement

```
Example: Monday, 9:00 AM - 10:30 AM Lecture

Timeline:
├─ 8:50 AM  ❌ Too Early
│           "Window opens at 9:00 AM (in 10 minutes)"
│
├─ 9:00 AM  ✅ Window Opens
│           [Countdown: 09:59 remaining]
│
├─ 9:10 AM  ✅ Last Second
│           [Countdown: 00:01 remaining]
│
└─ 9:10:01  ❌ Window Closed
            "Window closed 1 second ago"
```

**Key Rules:**
- ✅ Mark from lecture start to 10 minutes after
- ❌ Cannot mark before lecture starts
- ❌ Cannot mark after 10-minute window
- ⏱ Real-time countdown timer
- 📊 Clear status messages

**Features:**
- Section-wise filtering
- Bulk marking with individual adjustments
- Optional remarks per student
- Student name display
- Duplicate prevention
- Audit trail

### 2. 📅 Timetable Management

**Creating Timetables (Admin):**
- Select day (Sunday-Saturday)
- Set start and end times
- Assign course, teacher, section
- Specify classroom
- Set active status

**Viewing Timetables:**
- Teachers: All assigned lectures
- Students: Only enrolled courses
- Organized by day and time
- Filter by day/course/section

### 3. 📊 Reporting System

**Admin Reports:**
- System-wide statistics
- Course-wise attendance summaries
- Student performance analytics
- Teacher activity tracking
- Custom date ranges

**Teacher Reports:**
- Assigned courses only
- Section filtering
- Student attendance lists
- Percentage calculations
- Trend analysis

**Student Reports:**
- Personal attendance records
- Course-wise breakdown
- Attendance percentages
- Low attendance alerts
- History by date

### 4. 👤 User Management

**Creating Users (Admin Only):**

1. Navigate to Admin → Manage Students/Teachers
2. Click "Create Student" or "Create Teacher"
3. Fill in required details
4. Set initial password
5. User must change password on first login

**Password Management:**
- Show/hide toggle on all password fields
- Forced change on first login
- Admin can reset any password
- Self-service password change

### 5. 🔗 Assignment Management

**Admin can:**
- Assign students to sections
- Assign students to courses
- Assign teachers to courses
- Bulk assignment operations

---

## 🔌 API Documentation

### Accessing Swagger UI

1. Run application in development mode
2. Navigate to: `https://localhost:5001/api-docs`
3. Explore and test endpoints

### API Authentication

**Getting a Token:**

```
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@ams.com",
  "password": "Admin123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "GUID",
  "expiration": "2024-12-08T10:00:00Z"
}
```

**Using the Token:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## 📁 Project Structure

```
AMS/
├── Controllers/              # MVC Controllers
│   ├── AccountController.cs      # Authentication
│   ├── AdminController.cs        # Admin operations
│   ├── AttendanceController.cs   # Attendance marking
│   ├── CourseController.cs       # Course management
│   ├── HomeController.cs         # Home page
│   ├── ReportController.cs       # Reports
│   ├── StudentController.cs      # Student dashboard
│   ├── TeacherController.cs      # Teacher dashboard
│   └── TimetableController.cs    # Timetable management
│
├── Models/                   # Data Models
│   ├── Domain Entities
│   ├── ViewModels
│   ├── Repositories
│   ├── Services
│   └── DbContext
│
├── Views/                    # Razor Views
│   ├── Account/
│   ├── Admin/
│   ├── Attendance/
│   ├── Student/
│   ├── Teacher/
│   └── Shared/
│
├── wwwroot/                  # Static files
│   ├── css/
│   ├── js/
│   └── lib/
│
├── appsettings.json          # Configuration
├── Program.cs                # Entry point
└── AMS.csproj                # Project file
```

---

## 🔒 Security Features

### Authentication & Authorization
- ASP.NET Core Identity with role-based access
- JWT tokens with 60-minute expiry
- Refresh tokens with 7-day validity
- Secure cookies (HttpOnly, Secure, SameSite)

### Password Security
- PBKDF2 hashing (Identity default)
- Minimum 6 characters, one digit
- Forced change on first login
- Account lockout after 5 failed attempts

### Data Protection
- SQL injection prevention (EF Core)
- XSS protection (Razor encoding)
- CSRF protection (anti-forgery tokens)
- HTTPS enforcement
- HSTS enabled

---

## 📖 Usage Guide

### First-Time Setup

1. **Login as Admin**
   - Navigate to `https://localhost:5001`
   - Login: `admin@ams.com` / `Admin123!`
   - Change password

2. **Create Session**
   - Admin → Manage Sessions
   - Create "2024-2025" session
   - Set active

3. **Create Sections**
   - Admin → Manage Sections
   - Create sections A, B, C

4. **Create Courses**
   - Admin → Manage Courses
   - Add courses with details

5. **Create Users**
   - Create teachers and students
   - Assign to sections/courses

6. **Create Timetables**
   - Admin → Manage Timetables
   - Schedule lectures

### Marking Attendance (Teacher)

1. Login as teacher
2. Navigate to "Mark Attendance"
3. Select course
4. Verify window is open
5. Select section
6. Mark attendance
7. Submit

---

## 🛠 Troubleshooting

### Database Connection Errors
- Verify SQL Server is running
- Check connection string
- Run `dotnet ef database update`

### Migration Issues
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### JWT Token Errors
- Ensure JWT Key is 32+ characters
- Check `appsettings.json` configuration

### Attendance Window Issues
- Verify timetable entry exists
- Check day matches (0=Sunday)
- Ensure within 10-minute window

---

## 📄 License

MIT License - See LICENSE file for details

---

## 📞 Support

- **Email**: support@ams.edu
- **Documentation**: This README
- **API Docs**: `https://localhost:5001/api-docs`

---

## 🎓 Acknowledgments

- ASP.NET Core Team
- Bootstrap Team
- Entity Framework Team
- All Contributors

---

**Built with ❤️ using ASP.NET Core 8.0**

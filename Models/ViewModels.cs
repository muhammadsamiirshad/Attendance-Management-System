using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class AttendanceMarkViewModel
    {
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public List<StudentAttendanceItem> Students { get; set; } = new List<StudentAttendanceItem>();
        public AttendanceWindowStatus? WindowStatus { get; set; }
    }

    public class StudentAttendanceItem
    {
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public bool IsPresent { get; set; } = true;
        public string? Remarks { get; set; }
    }

    public class AssignStudentSectionViewModel
    {
        public int StudentId { get; set; }
        public int SectionId { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    public class AssignCourseStudentViewModel
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
    }

    public class AssignTeacherCourseViewModel
    {
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        public List<Course> Courses { get; set; } = new List<Course>();
    }

    // Admin ViewModels
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalSections { get; set; }
    }

    public class AssignStudentToSectionViewModel
    {
        [Required(ErrorMessage = "Please select at least one student")]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        [Required(ErrorMessage = "Please select a section")]
        public int SectionId { get; set; }
        
        public IEnumerable<Student> Students { get; set; } = new List<Student>();
        public IEnumerable<Section> Sections { get; set; } = new List<Section>();
    }

    public class AssignSectionToSessionViewModel
    {
        public int SectionId { get; set; }
        public int SessionId { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
        public List<Session> Sessions { get; set; } = new List<Session>();
    }

    public class AssignCourseToStudentViewModel
    {
        [Required(ErrorMessage = "Please select a course")]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Please select at least one student")]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
        public IEnumerable<Student> Students { get; set; } = new List<Student>();
    }

    public class AssignTeacherToCourseViewModel
    {
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
        public IEnumerable<Teacher> Teachers { get; set; } = new List<Teacher>();
        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
    }

    // Student ViewModels
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; } = null!;
        public List<Course> EnrolledCourses { get; set; } = new List<Course>();
        public List<Timetable> TodaysTimetable { get; set; } = new List<Timetable>();
        public int TotalCourses { get; set; }
    }

    public class CourseRegistrationViewModel
    {
        public Student Student { get; set; } = null!;
        public List<Course> AvailableCourses { get; set; } = new List<Course>();
        public List<Course> EnrolledCourses { get; set; } = new List<Course>();
    }

    public class StudentAttendanceViewModel
    {
        public Student Student { get; set; } = null!;
        public List<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
        public Dictionary<int, double> CourseAttendancePercentages { get; set; } = new Dictionary<int, double>();
    }

    // Teacher ViewModels
    public class TeacherDashboardViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public List<Course> AssignedCourses { get; set; } = new List<Course>();
        public List<Timetable> TodaysTimetable { get; set; } = new List<Timetable>();
        public int TotalCourses { get; set; }
    }

    public class AttendanceManagementViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public List<Course> Courses { get; set; } = new List<Course>();
        public DateTime SelectedDate { get; set; }
    }

    public class TeacherAttendanceViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Course> AssignedCourses { get; set; } = new List<Course>();
        public List<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    }

    // Timetable ViewModels
    public class TimetableViewModel
    {
        public List<Timetable> Timetables { get; set; } = new List<Timetable>();
        public string Title { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public int StudentId { get; set; }
    }

    public class TimetableCreateViewModel
    {
        [Required]
        public int CourseId { get; set; }
        
        [Required]
        public int TeacherId { get; set; }
        
        [Required]
        public int SectionId { get; set; }
        
        [Required]
        public string Day { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Start Time")]
        public string StartTimeString { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "End Time")]
        public string EndTimeString { get; set; } = string.Empty;
        
        public string Classroom { get; set; } = string.Empty;

        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
        public IEnumerable<Teacher> Teachers { get; set; } = new List<Teacher>();
        public IEnumerable<Section> Sections { get; set; } = new List<Section>();

        // Helper properties for conversion
        public TimeSpan StartTime
        {
            get
            {
                if (TimeSpan.TryParse(StartTimeString, out var time))
                    return time;
                return TimeSpan.Zero;
            }
        }

        public TimeSpan EndTime
        {
            get
            {
                if (TimeSpan.TryParse(EndTimeString, out var time))
                    return time;
                return TimeSpan.Zero;
            }
        }
    }

    public class TimetableEditViewModel : TimetableCreateViewModel
    {
        public int Id { get; set; }
    }

    // Attendance ViewModels
    public class AttendanceIndexViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
    }

    public class AttendanceMarkSelectViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Section> Sections { get; set; } = new List<Section>();
        public DateTime SelectedDate { get; set; }
    }
    
    public class CourseSection
    {
        public int CourseId { get; set; }
        public int SectionId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }

    // Report ViewModels
    public class RecentActivity
    {
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ReportIndexViewModel
    {
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
        public int TotalStudents { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public double AverageAttendance { get; set; }
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    }

    public class MonthlyReportViewModel
    {
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public int? SelectedCourseId { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Attendance> AttendanceData { get; set; } = new List<Attendance>();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public double AttendancePercentage { get; set; }
        public int TotalClasses { get; set; }
    }

    public class SemesterReportViewModel
    {
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public DateTime SemesterStartDate { get; set; }
        public DateTime SemesterEndDate { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Attendance> AttendanceData { get; set; } = new List<Attendance>();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public double AttendancePercentage { get; set; }
        public int TotalClasses { get; set; }
    }

    public class YearlyReportViewModel
    {
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int SelectedYear { get; set; }
        public List<Course> Courses { get; set; } = new List<Course>();
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Attendance> AttendanceData { get; set; } = new List<Attendance>();
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public double AttendancePercentage { get; set; }
        public int TotalClasses { get; set; }
    }

    public class AttendanceReportResultViewModel
    {
        public List<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
        public string ReportTitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
    }

    // Student and Teacher Management ViewModels
    public class CreateStudentViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; } = DateTime.Today;
    }

    public class EditStudentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Enrollment Date")]
        [DataType(DataType.Date)]
        public DateTime EnrollmentDate { get; set; }
    }

    public class StudentDetailsViewModel
    {
        public Student Student { get; set; } = null!;
        public List<Course> EnrolledCourses { get; set; } = new List<Course>();
        public List<Section> Sections { get; set; } = new List<Section>();
        public Dictionary<int, double> CourseAttendancePercentages { get; set; } = new Dictionary<int, double>();
    }

    public class CreateTeacherViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; } = DateTime.Today;
    }

    public class EditTeacherViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Teacher Number")]
        public string TeacherNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Department")]
        public string Department { get; set; } = string.Empty;

        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public DateTime HireDate { get; set; }
    }

    public class TeacherDetailsViewModel
    {
        public Teacher Teacher { get; set; } = null!;
        public List<Course> AssignedCourses { get; set; } = new List<Course>();
        public List<Timetable> Timetables { get; set; } = new List<Timetable>();
    }

    // Section Details View Models
    public class SectionDetailsViewModel
    {
        public Section Section { get; set; } = null!;
        public List<StudentInSectionViewModel> Students { get; set; } = new List<StudentInSectionViewModel>();
        public List<Session> AssignedSessions { get; set; } = new List<Session>();
        public int TotalStudents { get; set; }
        public int Capacity { get; set; }
        public int AvailableSpots { get; set; }
    }

    public class StudentInSectionViewModel
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime AssignedToSectionDate { get; set; }
        public List<string> RegisteredCourses { get; set; } = new List<string>();
    }

    // Course Details View Models
    public class CourseDetailsViewModel
    {
        public Course Course { get; set; } = null!;
        public Teacher? AssignedTeacher { get; set; }
        public List<StudentInCourseViewModel> EnrolledStudents { get; set; } = new List<StudentInCourseViewModel>();
        public int TotalStudents { get; set; }
        public List<Timetable> ClassSchedule { get; set; } = new List<Timetable>();
    }

    public class StudentInCourseViewModel
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public int AttendanceCount { get; set; }
        public int TotalClasses { get; set; }
        public double AttendancePercentage { get; set; }
    }

    // Session Details View Models
    public class SessionDetailsViewModel
    {
        public Session Session { get; set; } = null!;
        public List<SectionInSessionViewModel> AssignedSections { get; set; } = new List<SectionInSessionViewModel>();
        public int TotalSections { get; set; }
        public int TotalStudents { get; set; }
    }

    public class SectionInSessionViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int Capacity { get; set; }
        public DateTime AssignedDate { get; set; }
    }

    // Overview View Models
    public class SectionsOverviewViewModel
    {
        public List<SectionOverviewItem> Sections { get; set; } = new List<SectionOverviewItem>();
    }

    public class SectionOverviewItem
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int Capacity { get; set; }
        public int AvailableSpots { get; set; }
        public List<string> AssignedSessions { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }

    public class CoursesOverviewViewModel
    {
        public List<CourseOverviewItem> Courses { get; set; } = new List<CourseOverviewItem>();
    }

    public class CourseOverviewItem
    {
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int CreditHours { get; set; }
        public string? AssignedTeacher { get; set; }
        public int EnrolledStudents { get; set; }
        public bool IsActive { get; set; }
    }
}

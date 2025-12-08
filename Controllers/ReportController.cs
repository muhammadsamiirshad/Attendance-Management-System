using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ICourseService _courseService;
        private readonly IStudentRepository _studentRepository;

        public ReportController(
            IReportService reportService,
            ICourseService courseService,
            IStudentRepository studentRepository)
        {
            _reportService = reportService;
            _courseService = courseService;
            _studentRepository = studentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var students = await _studentRepository.GetAllAsync();

            // Get today's attendance statistics
            var today = DateTime.Today;
            var todayStats = await _reportService.GetAttendanceStatisticsAsync(null, null, today, today);
            
            // Get overall attendance for average calculation (last 30 days)
            var last30Days = today.AddDays(-30);
            var overallStats = await _reportService.GetAttendanceStatisticsAsync(null, null, last30Days, today);
            
            // Get recent attendance activities
            var recentAttendances = await _reportService.GetMonthlyAttendanceReportAsync(null, null, today.Year, today.Month);
            var recentActivities = recentAttendances
                .OrderByDescending(a => a.Date)
                .Take(10)
                .Select(a => new RecentActivity
                {
                    Description = $"Attendance Marked",
                    Details = $"{a.Student?.FirstName} {a.Student?.LastName} - {a.Course?.CourseName} - {a.Status}",
                    Timestamp = a.Date
                })
                .ToList();

            var viewModel = new ReportIndexViewModel
            {
                Courses = courses.ToList(),
                Students = students.ToList(),
                TotalStudents = students.Count(),
                PresentToday = todayStats.ContainsKey("PresentCount") ? Convert.ToInt32(todayStats["PresentCount"]) : 0,
                AbsentToday = todayStats.ContainsKey("AbsentCount") ? Convert.ToInt32(todayStats["AbsentCount"]) : 0,
                AverageAttendance = overallStats.ContainsKey("AttendancePercentage") ? Convert.ToDouble(overallStats["AttendancePercentage"]) : 0.0,
                RecentActivities = recentActivities.Any() ? recentActivities : new List<RecentActivity>
                {
                    new RecentActivity { Description = "No Recent Activity", Details = "No attendance records found", Timestamp = DateTime.Now }
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Monthly()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var students = await _studentRepository.GetAllAsync();

            var viewModel = new MonthlyReportViewModel
            {
                Courses = courses.ToList(),
                Students = students.ToList(),
                SelectedYear = DateTime.Now.Year,
                SelectedMonth = DateTime.Now.Month
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateMonthlyReport(MonthlyReportViewModel model)
        {
            var attendance = await _reportService.GetMonthlyAttendanceReportAsync(
                model.StudentId, model.CourseId, model.SelectedYear, model.SelectedMonth);

            // Update the model with the fetched data
            model.AttendanceData = attendance.ToList();
            model.TotalPresent = attendance.Count(a => a.Status == AttendanceStatus.Present);
            model.TotalAbsent = attendance.Count(a => a.Status == AttendanceStatus.Absent);
            model.TotalClasses = attendance.Count();
            model.AttendancePercentage = model.TotalClasses > 0 ? (double)model.TotalPresent / model.TotalClasses * 100 : 0;
            model.SelectedCourseId = model.CourseId;

            var reportViewModel = new AttendanceReportResultViewModel
            {
                AttendanceRecords = attendance.ToList(),
                ReportTitle = $"Monthly Attendance Report - {DateTime.Now:MMMM yyyy}",
                GeneratedDate = DateTime.Now
            };

            return PartialView("_AttendanceReportResultPartial", reportViewModel);
        }

        public async Task<IActionResult> Semester()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var students = await _studentRepository.GetAllAsync();

            var viewModel = new SemesterReportViewModel
            {
                Courses = courses.ToList(),
                Students = students.ToList(),
                SemesterStartDate = DateTime.Now.AddMonths(-6),
                SemesterEndDate = DateTime.Now
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSemesterReport(SemesterReportViewModel model)
        {
            var attendance = await _reportService.GetSemesterAttendanceReportAsync(
                model.StudentId, model.CourseId, model.SemesterStartDate, model.SemesterEndDate);

            var reportViewModel = new AttendanceReportResultViewModel
            {
                AttendanceRecords = attendance.ToList(),
                ReportTitle = $"Semester Attendance Report - {model.SemesterStartDate:dd/MM/yyyy} to {model.SemesterEndDate:dd/MM/yyyy}",
                GeneratedDate = DateTime.Now
            };

            return PartialView("_AttendanceReportResultPartial", reportViewModel);
        }

        public async Task<IActionResult> Yearly()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var students = await _studentRepository.GetAllAsync();

            var viewModel = new YearlyReportViewModel
            {
                Courses = courses.ToList(),
                Students = students.ToList(),
                SelectedYear = DateTime.Now.Year
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateYearlyReport(YearlyReportViewModel model)
        {
            var attendance = await _reportService.GetYearlyAttendanceReportAsync(
                model.StudentId, model.CourseId, model.SelectedYear);

            var reportViewModel = new AttendanceReportResultViewModel
            {
                AttendanceRecords = attendance.ToList(),
                ReportTitle = $"Yearly Attendance Report - {model.SelectedYear}",
                GeneratedDate = DateTime.Now
            };

            return PartialView("_AttendanceReportResultPartial", reportViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetStatistics(int? studentId, int? courseId, DateTime startDate, DateTime endDate)
        {
            var statistics = await _reportService.GetAttendanceStatisticsAsync(studentId, courseId, startDate, endDate);
            return Json(statistics);
        }
    }
}

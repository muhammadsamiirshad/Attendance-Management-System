using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ITimetableService _timetableService;
        private readonly IAttendanceService _attendanceService;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<AppUser> _userManager;

        public TeacherController(
            ICourseService courseService,
            ITimetableService timetableService,
            IAttendanceService attendanceService,
            ITeacherRepository teacherRepository,
            IStudentRepository studentRepository,
            UserManager<AppUser> userManager)
        {
            _courseService = courseService;
            _timetableService = timetableService;
            _attendanceService = attendanceService;
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            
            if (teacher == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
            var timetable = await _timetableService.GetTimetableByTeacherAsync(teacher.Id);

            var viewModel = new TeacherDashboardViewModel
            {
                Teacher = teacher,
                AssignedCourses = courses.ToList(),
                TodaysTimetable = timetable.Where(t => t.Day == (DayOfWeekEnum)DateTime.Today.DayOfWeek).ToList(),
                TotalCourses = courses.Count()
            };

            return View(viewModel); 
        }

        public IActionResult ManageAttendance()
        {
            // Redirect to the enhanced Attendance/Mark controller
            return RedirectToAction("Mark", "Attendance");
        }

        [HttpPost]
        public async Task<IActionResult> LoadStudentsForAttendance(int courseId, DateTime date)
        {
            try
            {
                if (courseId <= 0)
                {
                    return BadRequest("Invalid course selected.");
                }

                var attendanceModel = await _attendanceService.GetAttendanceMarkViewModelAsync(courseId, date);
                
                if (attendanceModel == null)
                {
                    return BadRequest("Unable to load attendance data.");
                }

                return PartialView("_AttendanceMarkingPartial", attendanceModel);
            }
            catch (Exception ex)
            {
                // Log the error here if you have logging configured
                return BadRequest($"Error loading students: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(AttendanceMarkViewModel model)
        {
            try
            {
                // Check if model is null
                if (model == null)
                {
                    return Json(new { success = false, message = "No data received." });
                }

                // Check if students list is null or empty
                if (model.Students == null || !model.Students.Any())
                {
                    return Json(new { success = false, message = "No students to mark attendance for." });
                }

                // Check if CourseId is valid
                if (model.CourseId <= 0)
                {
                    return Json(new { success = false, message = "Invalid course selected." });
                }

                // Log validation errors if model state is invalid
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                var result = await _attendanceService.MarkAttendanceAsync(model, user.Id);
                
                if (result)
                {
                    TempData["Success"] = "Attendance marked successfully.";
                    return Json(new { success = true, message = "Attendance marked successfully for " + model.Students.Count + " students." });
                }
                
                return Json(new { success = false, message = "Failed to save attendance. Please try again." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> ViewAttendance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            
            if (teacher == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
            
            var viewModel = new TeacherAttendanceViewModel
            {
                Teacher = teacher,
                Courses = courses.ToList(),
                AssignedCourses = courses.ToList(),
                AttendanceRecords = new List<Attendance>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceReport(int courseId, DateTime startDate, DateTime endDate)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }
            
            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            
            if (teacher == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
            var attendance = await _attendanceService.GetAttendanceForReportAsync(null, courseId, startDate, endDate);
            
            var viewModel = new TeacherAttendanceViewModel
            {
                Teacher = teacher,
                Courses = courses.ToList(),
                AssignedCourses = courses.ToList(),
                AttendanceRecords = attendance.ToList()
            };

            return View("ViewAttendance", viewModel);
        }

        public async Task<IActionResult> ViewTimetable()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            
            if (teacher == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var timetable = await _timetableService.GetTimetableByTeacherAsync(teacher.Id);
            
            var viewModel = new TimetableViewModel
            {
                Timetables = timetable.ToList(),
                Title = "My Teaching Schedule"
            };

            return View(viewModel);
        }
    }
}

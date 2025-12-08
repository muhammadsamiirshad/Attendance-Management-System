using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ICourseService _courseService;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly UserManager<AppUser> _userManager;

        public AttendanceController(
            IAttendanceService attendanceService,
            ICourseService courseService,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository,
            UserManager<AppUser> userManager)
        {
            _attendanceService = attendanceService;
            _courseService = courseService;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Course> courses;
            IEnumerable<Student> students;

            // Check if user is a teacher - show only their courses
            if (User.IsInRole("Teacher"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
                    if (teacher != null)
                    {
                        courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
                        // Get students from teacher's courses
                        var courseIds = courses.Select(c => c.Id).ToList();
                        var allStudents = await _studentRepository.GetAllAsync();
                        students = allStudents; // For now, show all students but teacher can filter by their courses
                    }
                    else
                    {
                        courses = new List<Course>();
                        students = new List<Student>();
                    }
                }
                else
                {
                    courses = new List<Course>();
                    students = new List<Student>();
                }
            }
            else
            {
                // Admin sees all courses and students
                courses = await _courseService.GetAllCoursesAsync();
                students = await _studentRepository.GetAllAsync();
            }

            var viewModel = new AttendanceIndexViewModel
            {
                Courses = courses.ToList(),
                Students = students.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetAttendanceData(int? courseId, int? studentId, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddMonths(-1);
            var end = endDate ?? DateTime.Today;

            var attendance = await _attendanceService.GetAttendanceForReportAsync(studentId, courseId, start, end);
            return PartialView("_AttendanceDataPartial", attendance);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Mark()
        {
            // Get the logged-in user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the teacher record for this user
            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            if (teacher == null)
            {
                TempData["Error"] = "Teacher profile not found. Please contact administrator.";
                return RedirectToAction("Index", "Home");
            }

            // Get only the courses assigned to this teacher
            var courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
            
            if (!courses.Any())
            {
                TempData["Warning"] = "You don't have any courses assigned yet. Please contact administrator.";
            }
            
            var viewModel = new AttendanceMarkSelectViewModel
            {
                Courses = courses.ToList(),
                SelectedDate = DateTime.Today
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> LoadStudentsForMarking(int courseId, DateTime date, int? sectionId = null)
        {
            // Validate attendance window
            var windowStatus = await _attendanceService.ValidateAttendanceWindowAsync(courseId, date);
            
            if (!windowStatus.IsAllowed)
            {
                return Json(new { 
                    success = false, 
                    isLocked = windowStatus.IsLocked,
                    message = windowStatus.Message,
                    lectureStartTime = windowStatus.LectureStartTime?.ToString("hh:mm tt"),
                    windowStartTime = windowStatus.WindowStartTime?.ToString("hh:mm tt")
                });
            }
            
            var model = await _attendanceService.GetAttendanceMarkViewModelAsync(courseId, date, sectionId);
            model.WindowStatus = windowStatus;
            return PartialView("_StudentAttendanceListPartial", model);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> MarkAttendance(AttendanceMarkViewModel model)
        {
            // Validate attendance window
            var windowStatus = await _attendanceService.ValidateAttendanceWindowAsync(model.CourseId, model.Date);
            
            if (!windowStatus.IsAllowed)
            {
                return Json(new { 
                    success = false, 
                    message = windowStatus.Message,
                    isLocked = windowStatus.IsLocked
                });
            }
            
            // Remove validation errors for Remarks field (it's optional)
            var remarksKeys = ModelState.Keys.Where(k => k.Contains("Remarks")).ToList();
            foreach (var key in remarksKeys)
            {
                ModelState.Remove(key);
            }
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return Json(new { 
                    success = false, 
                    message = "Validation failed: " + string.Join(", ", errors),
                    errors = errors
                });
            }
            
            var markedBy = User.Identity?.Name ?? "Unknown";
            var result = await _attendanceService.MarkAttendanceAsync(model, markedBy);
            if (result)
            {
                TempData["Success"] = "Attendance marked successfully.";
                return Json(new { success = true, message = "Attendance marked successfully." });
            }

            return Json(new { success = false, message = "Failed to save attendance. Please try again." });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetSectionsForCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var teacher = await _teacherRepository.GetByUserIdAsync(user.Id);
            if (teacher == null)
            {
                return Json(new { success = false, message = "Teacher profile not found" });
            }

            var sections = await _attendanceService.GetSectionsByTeacherAndCourseAsync(teacher.Id, courseId);
            
            return Json(new { 
                success = true, 
                sections = sections.Select(s => new { 
                    id = s.Id, 
                    name = s.SectionName 
                }).ToList()
            });
        }
    }
}

using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ITimetableService _timetableService;
        private readonly IAttendanceService _attendanceService;
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<AppUser> _userManager;

        public StudentController(
            ICourseService courseService,
            ITimetableService timetableService,
            IAttendanceService attendanceService,
            IStudentRepository studentRepository,
            UserManager<AppUser> userManager)
        {
            _courseService = courseService;
            _timetableService = timetableService;
            _attendanceService = attendanceService;
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
            
            var student = await _studentRepository.GetByUserIdAsync(user.Id);
            
            if (student == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var courses = await _courseService.GetCoursesByStudentAsync(student.Id);
            var timetable = await _timetableService.GetTimetableByStudentAsync(student.Id);

            var viewModel = new StudentDashboardViewModel
            {
                Student = student,
                EnrolledCourses = courses.ToList(),
                TodaysTimetable = timetable.Where(t => t.Day == (DayOfWeekEnum)DateTime.Today.DayOfWeek).ToList(),
                TotalCourses = courses.Count()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> RegisterCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var student = await _studentRepository.GetByUserIdAsync(user.Id);
            
            if (student == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var availableCourses = await _courseService.GetActiveCoursesAsync();
            var enrolledCourses = await _courseService.GetCoursesByStudentAsync(student.Id);
            
            var viewModel = new CourseRegistrationViewModel
            {
                Student = student,
                AvailableCourses = availableCourses.Where(c => !enrolledCourses.Any(ec => ec.Id == c.Id)).ToList(),
                EnrolledCourses = enrolledCourses.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCourse(int courseId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var student = await _studentRepository.GetByUserIdAsync(user.Id);
                
                if (student == null)
                {
                    return Json(new { success = false, message = "Student profile not found" });
                }

                // Register the course
                var success = await _courseService.RegisterStudentToCourseAsync(student.Id, courseId);
                
                if (success)
                {
                    TempData["Success"] = "Course registered successfully.";
                    return Json(new { success = true, message = "Course registered successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "You may already be enrolled in this course or the course is not available." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        public async Task<IActionResult> ViewTimetable()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Please log in to view your timetable.";
                return RedirectToAction("Login", "Account");
            }
            
            var student = await _studentRepository.GetByUserIdAsync(user.Id);
            
            if (student == null)
            {
                TempData["Error"] = "Student profile not found. Please contact administration.";
                return RedirectToAction("Index", "Home");
            }

            // Get timetable for student
            var timetable = await _timetableService.GetTimetableByStudentAsync(student.Id);
            
            // Always create a valid viewModel, even if timetable is empty
            var viewModel = new TimetableViewModel
            {
                Timetables = timetable?.ToList() ?? new List<Timetable>(),
                Title = "My Timetable",
                StudentName = user.FullName,
                StudentId = student.Id
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ViewAttendance()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var student = await _studentRepository.GetByUserIdAsync(user.Id);
            
            if (student == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var attendance = await _attendanceService.GetAttendanceByStudentAsync(student.Id);
            var courses = await _courseService.GetCoursesByStudentAsync(student.Id);

            var viewModel = new StudentAttendanceViewModel
            {
                Student = student,
                AttendanceRecords = attendance.ToList(),
                CourseAttendancePercentages = new Dictionary<int, double>()
            };

            // Calculate attendance percentages for each course
            foreach (var course in courses)
            {
                var percentage = await _attendanceService.GetAttendancePercentageAsync(student.Id, course.Id);
                viewModel.CourseAttendancePercentages[course.Id] = percentage;
            }

            return View(viewModel);
        }
    }
}

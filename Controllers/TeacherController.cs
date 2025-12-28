using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly ApplicationDbContext _context;

        public TeacherController(
            ICourseService courseService,
            ITimetableService timetableService,
            IAttendanceService attendanceService,
            ITeacherRepository teacherRepository,
            IStudentRepository studentRepository,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _courseService = courseService;
            _timetableService = timetableService;
            _attendanceService = attendanceService;
            _teacherRepository = teacherRepository;
            _studentRepository = studentRepository;
            _userManager = userManager;
            _context = context;
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

        // View all courses taught by this teacher
        public async Task<IActionResult> MyCourses()
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

            var courses = await _context.Courses
                .Include(c => c.StudentRegistrations)
                .Include(c => c.CourseAssignments)
                .Where(c => c.CourseAssignments.Any(ca => ca.TeacherId == teacher.Id && ca.IsActive))
                .ToListAsync();
            
            return View(courses);
        }

        // View students enrolled in a specific course
        public async Task<IActionResult> ViewCourseStudents(int id)
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

            // Verify teacher is assigned to this course
            var courses = await _courseService.GetCoursesByTeacherAsync(teacher.Id);
            if (!courses.Any(c => c.Id == id))
            {
                TempData["Error"] = "You are not authorized to view this course.";
                return RedirectToAction("MyCourses");
            }

            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            var students = await _studentRepository.GetStudentsByCourseAsync(id);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                AssignedTeacher = teacher,
                TotalStudents = students.Count(),
                EnrolledStudents = students.Select(s => new StudentInCourseViewModel
                {
                    StudentId = s.Id,
                    StudentNumber = s.StudentNumber,
                    FullName = $"{s.FirstName} {s.LastName}",
                    Email = s.Email,
                    Section = s.StudentSections.FirstOrDefault()?.Section?.SectionName ?? "Not Assigned",
                    RegisteredDate = s.CourseRegistrations.FirstOrDefault(cr => cr.CourseId == id)?.RegisteredDate ?? DateTime.MinValue,
                    AttendanceCount = s.Attendances.Count(a => a.CourseId == id && a.IsPresent),
                    TotalClasses = s.Attendances.Count(a => a.CourseId == id),
                    AttendancePercentage = s.Attendances.Any(a => a.CourseId == id) 
                        ? (double)s.Attendances.Count(a => a.CourseId == id && a.IsPresent) / s.Attendances.Count(a => a.CourseId == id) * 100 
                        : 0
                }).ToList(),
                ClassSchedule = course.Timetables.Where(t => t.IsActive).ToList()
            };

            return View(viewModel);
        }
    }
}

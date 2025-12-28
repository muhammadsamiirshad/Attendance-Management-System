using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ITimetableService _timetableService;

        public AdminController(
            IAssignmentService assignmentService,
            ICourseService courseService,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository,
            ISectionRepository sectionRepository,
            ISessionRepository sessionRepository,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            ITimetableService timetableService)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _sectionRepository = sectionRepository;
            _sessionRepository = sessionRepository;
            _userManager = userManager;
            _context = context;
            _timetableService = timetableService;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllAsync();
            var teachers = await _teacherRepository.GetAllAsync();
            var courses = await _courseService.GetAllCoursesAsync();
            var sections = await _sectionRepository.GetAllAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = students.Count(),
                TotalTeachers = teachers.Count(),
                TotalCourses = courses.Count(),
                TotalSections = sections.Count()
            };

            return View(viewModel);
        }

        // Student Management
        public async Task<IActionResult> ManageStudents()
        {
            var students = await _studentRepository.GetAllAsync();
            return View(students);
        }

        // GET: Admin/CreateStudent
        public IActionResult CreateStudent()
        {
            return View();
        }

        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(CreateStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model);
                    }

                    // Generate student number automatically
                    var studentNumber = await _studentRepository.GenerateNextStudentNumberAsync();

                    // Create AppUser
                    var user = new AppUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // Assign Student role
                        await _userManager.AddToRoleAsync(user, "Student");

                        // Create Student
                        var student = new Student
                        {
                            AppUserId = user.Id,
                            StudentNumber = studentNumber,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            Phone = model.Phone,
                            EnrollmentDate = model.EnrollmentDate
                        };

                        await _studentRepository.AddAsync(student);
                        await _studentRepository.SaveChangesAsync();

                        TempData["Success"] = "Student created successfully.";
                        return RedirectToAction(nameof(ManageStudents));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/EditStudent/5
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var viewModel = new EditStudentViewModel
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                EnrollmentDate = student.EnrollmentDate
            };

            return View(viewModel);
        }

        // POST: Admin/EditStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, EditStudentViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var student = await _studentRepository.GetByIdAsync(id);
                    if (student == null)
                    {
                        return NotFound();
                    }

                    // Check if student number is being changed and if it already exists
                    if (student.StudentNumber != model.StudentNumber)
                    {
                        var existingStudent = await _studentRepository.GetByStudentNumberAsync(model.StudentNumber);
                        if (existingStudent != null)
                        {
                            ModelState.AddModelError("StudentNumber", "Student number already exists.");
                            return View(model);
                        }
                    }

                    // Update student properties
                    student.StudentNumber = model.StudentNumber;
                    student.FirstName = model.FirstName;
                    student.LastName = model.LastName;
                    student.Email = model.Email;
                    student.Phone = model.Phone;
                    student.EnrollmentDate = model.EnrollmentDate;

                    await _studentRepository.UpdateAsync(student);
                    await _studentRepository.SaveChangesAsync();

                    TempData["Success"] = "Student updated successfully.";
                    return RedirectToAction(nameof(ManageStudents));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/StudentDetails/5
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var courses = await _courseService.GetCoursesByStudentAsync(id);
            var sections = await _studentRepository.GetStudentsBySectionAsync(id);

            var viewModel = new StudentDetailsViewModel
            {
                Student = student,
                EnrolledCourses = courses.ToList(),
                Sections = new List<Section>()
            };

            return View(viewModel);
        }

        // POST: Admin/DeleteStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Delete associated user
                var user = await _userManager.FindByIdAsync(student.AppUserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                await _studentRepository.DeleteAsync(id);
                await _studentRepository.SaveChangesAsync();

                TempData["Success"] = "Student deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageStudents));
        }

        // Teacher Management
        public async Task<IActionResult> ManageTeachers()
        {
            var teachers = await _teacherRepository.GetAllAsync();
            return View(teachers);
        }

        // GET: Admin/CreateTeacher
        public IActionResult CreateTeacher()
        {
            return View();
        }

        // POST: Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(CreateTeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model);
                    }

                    // Generate teacher number automatically
                    var teacherNumber = await _teacherRepository.GenerateNextTeacherNumberAsync();

                    // Create AppUser
                    var user = new AppUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // Assign Teacher role
                        await _userManager.AddToRoleAsync(user, "Teacher");

                        // Create Teacher
                        var teacher = new Teacher
                        {
                            AppUserId = user.Id,
                            TeacherNumber = teacherNumber,
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            Phone = model.Phone,
                            Department = model.Department,
                            HireDate = model.HireDate
                        };

                        await _teacherRepository.AddAsync(teacher);
                        await _teacherRepository.SaveChangesAsync();

                        TempData["Success"] = "Teacher created successfully.";
                        return RedirectToAction(nameof(ManageTeachers));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/EditTeacher/5
        public async Task<IActionResult> EditTeacher(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            var viewModel = new EditTeacherViewModel
            {
                Id = teacher.Id,
                TeacherNumber = teacher.TeacherNumber,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Department = teacher.Department,
                HireDate = teacher.HireDate
            };

            return View(viewModel);
        }

        // POST: Admin/EditTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, EditTeacherViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var teacher = await _teacherRepository.GetByIdAsync(id);
                    if (teacher == null)
                    {
                        return NotFound();
                    }

                    // Check if teacher number is being changed and if it already exists
                    if (teacher.TeacherNumber != model.TeacherNumber)
                    {
                        var existingTeacher = await _teacherRepository.GetByTeacherNumberAsync(model.TeacherNumber);
                        if (existingTeacher != null)
                        {
                            ModelState.AddModelError("TeacherNumber", "Teacher number already exists.");
                            return View(model);
                        }
                    }

                    // Update teacher properties
                    teacher.TeacherNumber = model.TeacherNumber;
                    teacher.FirstName = model.FirstName;
                    teacher.LastName = model.LastName;
                    teacher.Email = model.Email;
                    teacher.Phone = model.Phone;
                    teacher.Department = model.Department;
                    teacher.HireDate = model.HireDate;

                    await _teacherRepository.UpdateAsync(teacher);
                    await _teacherRepository.SaveChangesAsync();

                    TempData["Success"] = "Teacher updated successfully.";
                    return RedirectToAction(nameof(ManageTeachers));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Admin/TeacherDetails/5
        public async Task<IActionResult> TeacherDetails(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            var courses = await _courseService.GetCoursesByTeacherAsync(id);

            var viewModel = new TeacherDetailsViewModel
            {
                Teacher = teacher,
                AssignedCourses = courses.ToList(),
                Timetables = new List<Timetable>()
            };

            return View(viewModel);
        }

        // POST: Admin/DeleteTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            try
            {
                var teacher = await _teacherRepository.GetByIdAsync(id);
                if (teacher == null)
                {
                    return NotFound();
                }

                // Delete associated user
                var user = await _userManager.FindByIdAsync(teacher.AppUserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                await _teacherRepository.DeleteAsync(id);
                await _teacherRepository.SaveChangesAsync();

                TempData["Success"] = "Teacher deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageTeachers));
        }

        // POST: Admin/ResetStudentPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetStudentPassword(int id, string newPassword)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);
                if (student == null)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction(nameof(ManageStudents));
                }

                var user = await _userManager.FindByIdAsync(student.AppUserId);
                if (user == null)
                {
                    TempData["Error"] = "Associated user not found.";
                    return RedirectToAction(nameof(StudentDetails), new { id });
                }

                // Remove existing password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // Mark as first login so they must change password
                    user.FirstLogin = true;
                    await _userManager.UpdateAsync(user);

                    TempData["Success"] = "Password reset successfully. Student must change password on next login.";
                }
                else
                {
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(StudentDetails), new { id });
        }

        // POST: Admin/ResetTeacherPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetTeacherPassword(int id, string newPassword)
        {
            try
            {
                var teacher = await _teacherRepository.GetByIdAsync(id);
                if (teacher == null)
                {
                    TempData["Error"] = "Teacher not found.";
                    return RedirectToAction(nameof(ManageTeachers));
                }

                var user = await _userManager.FindByIdAsync(teacher.AppUserId);
                if (user == null)
                {
                    TempData["Error"] = "Associated user not found.";
                    return RedirectToAction(nameof(TeacherDetails), new { id });
                }

                // Remove existing password and set new one
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // Mark as first login so they must change password
                    user.FirstLogin = true;
                    await _userManager.UpdateAsync(user);

                    TempData["Success"] = "Password reset successfully. Teacher must change password on next login.";
                }
                else
                {
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(TeacherDetails), new { id });
        }

        // GET: Admin/AssignStudentsToSection
        public async Task<IActionResult> AssignStudentsToSection()
        {
            var viewModel = new AssignStudentToSectionViewModel
            {
                Students = await _assignmentService.GetUnassignedStudentsAsync(),
                Sections = await _sectionRepository.GetAllAsync()
            };
            return View(viewModel);
        }

        // POST: Admin/AssignStudentsToSection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStudentsToSection(AssignStudentToSectionViewModel model)
        {
            if (ModelState.IsValid)
            {
                int successCount = 0;
                int failCount = 0;

                foreach (var studentId in model.StudentIds)
                {
                    var result = await _assignmentService.AssignStudentToSectionAsync(studentId, model.SectionId);
                    if (result)
                        successCount++;
                    else
                        failCount++;
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"{successCount} student(s) assigned to section successfully.";
                }
                
                if (failCount > 0)
                {
                    TempData["Warning"] = $"{failCount} student(s) could not be assigned. Students can only be in one section at a time.";
                }

                return RedirectToAction(nameof(AssignStudentsToSection));
            }

            model.Students = await _assignmentService.GetUnassignedStudentsAsync();
            model.Sections = await _sectionRepository.GetAllAsync();
            return View(model);
        }

        // GET: Admin/AssignSectionsToSessions
        public async Task<IActionResult> AssignSectionsToSessions(int? sectionId)
        {
            var viewModel = new AssignSectionToSessionViewModel
            {
                SectionId = sectionId ?? 0,
                Sections = (await _sectionRepository.GetAllAsync()).ToList(),
                Sessions = (await _sessionRepository.GetAllAsync()).ToList()
            };
            return View(viewModel);
        }

        // POST: Admin/AssignSectionsToSessions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSectionsToSessions(AssignSectionToSessionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _assignmentService.AssignSectionToSessionAsync(model.SectionId, model.SessionId);
                if (result)
                {
                    TempData["Success"] = "Section assigned to session successfully.";
                    return RedirectToAction(nameof(AssignSectionsToSessions));
                }
                else
                {
                    ModelState.AddModelError("", "Failed to assign section to session.");
                }
            }

            model.Sections = (await _sectionRepository.GetAllAsync()).ToList();
            model.Sessions = (await _sessionRepository.GetAllAsync()).ToList();
            return View(model);
        }

        // GET: Admin/AssignCoursesToStudents
        public async Task<IActionResult> AssignCoursesToStudents()
        {
            var viewModel = new AssignCourseToStudentViewModel
            {
                Students = await _studentRepository.GetAllAsync(),
                Courses = await _courseService.GetAllCoursesAsync()
            };
            return View(viewModel);
        }

        // POST: Admin/AssignCoursesToStudents
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCoursesToStudents(AssignCourseToStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                int successCount = 0;
                int failCount = 0;

                foreach (var studentId in model.StudentIds)
                {
                    var result = await _assignmentService.AssignCourseToStudentAsync(model.CourseId, studentId);
                    if (result)
                        successCount++;
                    else
                        failCount++;
                }

                if (successCount > 0)
                {
                    TempData["Success"] = $"Course assigned to {successCount} student(s) successfully.";
                }
                
                if (failCount > 0)
                {
                    TempData["Warning"] = $"{failCount} student(s) could not be assigned (may already be enrolled).";
                }

                return RedirectToAction(nameof(AssignCoursesToStudents));
            }

            model.Students = await _studentRepository.GetAllAsync();
            model.Courses = await _courseService.GetAllCoursesAsync();
            return View(model);
        }

        // GET: Admin/AssignTeachersToCourses
        public async Task<IActionResult> AssignTeachersToCourses()
        {
            var viewModel = new AssignTeacherToCourseViewModel
            {
                Teachers = await _teacherRepository.GetAllAsync(),
                Courses = await _courseService.GetAllCoursesAsync()
            };
            return View(viewModel);
        }

        // POST: Admin/AssignTeachersToCourses
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeachersToCourses(AssignTeacherToCourseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _assignmentService.AssignTeacherToCourseAsync(model.TeacherId, model.CourseId);
                if (result)
                {
                    TempData["Success"] = "Teacher assigned to course successfully.";
                    return RedirectToAction(nameof(AssignTeachersToCourses));
                }
                else
                {
                    TempData["Error"] = "Failed to assign teacher. This course may already have another teacher assigned. Each course can only have one teacher.";
                }
            }

            model.Teachers = await _teacherRepository.GetAllAsync();
            model.Courses = await _courseService.GetAllCoursesAsync();
            return View(model);
        }

        // View Section Details
        public async Task<IActionResult> ViewSectionDetails(int id)
        {
            var section = await _context.Sections
                .Include(s => s.SessionSections)
                    .ThenInclude(ss => ss.Session)
                .Include(s => s.StudentSections)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (section == null)
                return NotFound();

            var students = await _studentRepository.GetStudentsBySectionAsync(id);

            var viewModel = new SectionDetailsViewModel
            {
                Section = section,
                TotalStudents = students.Count(),
                Capacity = section.Capacity,
                AvailableSpots = section.Capacity - students.Count(),
                Students = students.Select(s => new StudentInSectionViewModel
                {
                    StudentId = s.Id,
                    StudentNumber = s.StudentNumber,
                    FullName = $"{s.FirstName} {s.LastName}",
                    Email = s.Email,
                    EnrollmentDate = s.EnrollmentDate,
                    AssignedToSectionDate = s.StudentSections.FirstOrDefault(ss => ss.SectionId == id)?.AssignedDate ?? DateTime.MinValue,
                    RegisteredCourses = s.CourseRegistrations.Where(cr => cr.IsActive).Select(cr => cr.Course.CourseName).ToList()
                }).ToList(),
                AssignedSessions = section.SessionSections
                    .Where(ss => ss.IsActive)
                    .Select(ss => ss.Session)
                    .ToList()
            };

            return View(viewModel);
        }

        // View Course Details
        public async Task<IActionResult> ViewCourseDetails(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            var teacher = await _teacherRepository.GetTeachersByCourseAsync(id);
            var students = await _studentRepository.GetStudentsByCourseAsync(id);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,
                AssignedTeacher = teacher.FirstOrDefault(),
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

        // View Session Details
        public async Task<IActionResult> ViewSessionDetails(int id)
        {
            var session = await _sessionRepository.GetByIdAsync(id);
            if (session == null)
                return NotFound();

            var sections = await _sectionRepository.GetSectionsBySessionAsync(id);

            var viewModel = new SessionDetailsViewModel
            {
                Session = session,
                TotalSections = sections.Count(),
                AssignedSections = sections.Select(sec => new SectionInSessionViewModel
                {
                    SectionId = sec.Id,
                    SectionName = sec.SectionName,
                    Description = sec.Description,
                    StudentCount = sec.StudentSections.Count(ss => ss.IsActive),
                    Capacity = sec.Capacity,
                    AssignedDate = sec.SessionSections.FirstOrDefault(ss => ss.SessionId == id)?.AssignedDate ?? DateTime.MinValue
                }).ToList(),
                TotalStudents = sections.Sum(sec => sec.StudentSections.Count(ss => ss.IsActive))
            };

            return View(viewModel);
        }

        // View All Sections Overview
        public async Task<IActionResult> ViewAllSections()
        {
            var sections = await _context.Sections
                .Include(s => s.SessionSections)
                    .ThenInclude(ss => ss.Session)
                .Include(s => s.StudentSections)
                .ToListAsync();

            var viewModel = new SectionsOverviewViewModel
            {
                Sections = sections.Select(sec => new SectionOverviewItem
                {
                    SectionId = sec.Id,
                    SectionName = sec.SectionName,
                    Description = sec.Description,
                    StudentCount = sec.StudentSections.Count(ss => ss.IsActive),
                    Capacity = sec.Capacity,
                    AvailableSpots = sec.Capacity - sec.StudentSections.Count(ss => ss.IsActive),
                    AssignedSessions = sec.SessionSections.Where(ss => ss.IsActive).Select(ss => ss.Session.SessionName).ToList(),
                    IsActive = sec.IsActive
                }).ToList()
            };

            return View(viewModel);
        }

        // View All Courses Overview
        public async Task<IActionResult> ViewAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            
            // Load course details with navigation properties
            var coursesList = await _context.Courses
                .Include(c => c.CourseAssignments)
                    .ThenInclude(ca => ca.Teacher)
                .Include(c => c.StudentRegistrations)
                .ToListAsync();

            var viewModel = new CoursesOverviewViewModel
            {
                Courses = coursesList.Select(c => new CourseOverviewItem
                {
                    CourseId = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Department = c.Department,
                    CreditHours = c.CreditHours,
                    AssignedTeacher = c.CourseAssignments.Any(ca => ca.IsActive) 
                        ? $"{c.CourseAssignments.FirstOrDefault(ca => ca.IsActive)?.Teacher?.FirstName} {c.CourseAssignments.FirstOrDefault(ca => ca.IsActive)?.Teacher?.LastName}"
                        : "Not Assigned",
                    EnrolledStudents = c.StudentRegistrations.Count(sr => sr.IsActive),
                    IsActive = c.IsActive
                }).ToList()
            };

            return View(viewModel);
        }

        // Unassign Teacher from Course
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignTeacherFromCourse(int courseId, int teacherId)
        {
            try
            {
                var assignment = await _context.CourseAssignments
                    .FirstOrDefaultAsync(ca => ca.CourseId == courseId && ca.TeacherId == teacherId && ca.IsActive);

                if (assignment != null)
                {
                    assignment.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Teacher successfully unassigned from the course.";
                }
                else
                {
                    TempData["Error"] = "Assignment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unassigning teacher: {ex.Message}";
            }

            return RedirectToAction("ViewCourseDetails", new { id = courseId });
        }

        // Unassign Student from Section
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignStudentFromSection(int studentId, int sectionId)
        {
            try
            {
                var assignment = await _context.StudentSections
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SectionId == sectionId && ss.IsActive);

                if (assignment != null)
                {
                    assignment.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Student successfully unassigned from the section.";
                }
                else
                {
                    TempData["Error"] = "Assignment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unassigning student: {ex.Message}";
            }

            return RedirectToAction("ViewSectionDetails", new { id = sectionId });
        }

        // Unassign Section from Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignSectionFromSession(int sectionId, int sessionId)
        {
            try
            {
                var assignment = await _context.SessionSections
                    .FirstOrDefaultAsync(ss => ss.SectionId == sectionId && ss.SessionId == sessionId && ss.IsActive);

                if (assignment != null)
                {
                    assignment.IsActive = false;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Section successfully unassigned from the session.";
                }
                else
                {
                    TempData["Error"] = "Assignment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error unassigning section: {ex.Message}";
            }

            return RedirectToAction("ViewSessionDetails", new { id = sessionId });
        }

        // Timetable Management
        public async Task<IActionResult> ManageTimetables()
        {
            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .Include(t => t.Section)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
            
            return View(timetables);
        }

        // GET: Admin/CreateTimetable
        public async Task<IActionResult> CreateTimetable()
        {
            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Teachers = await _teacherRepository.GetAllAsync();
            ViewBag.Sections = await _sectionRepository.GetAllAsync();
            return View();
        }

        // POST: Admin/CreateTimetable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTimetable(Timetable model)
        {
            // Debug logging - log all received values
            System.Diagnostics.Debug.WriteLine($"===== Timetable Creation Attempt =====");
            System.Diagnostics.Debug.WriteLine($"CourseId: {model.CourseId}");
            System.Diagnostics.Debug.WriteLine($"TeacherId: {model.TeacherId}");
            System.Diagnostics.Debug.WriteLine($"SectionId: {model.SectionId}");
            System.Diagnostics.Debug.WriteLine($"Day: {model.Day}");
            System.Diagnostics.Debug.WriteLine($"StartTime: {model.StartTime}");
            System.Diagnostics.Debug.WriteLine($"EndTime: {model.EndTime}");
            System.Diagnostics.Debug.WriteLine($"Classroom: {model.Classroom}");
            System.Diagnostics.Debug.WriteLine($"IsActive: {model.IsActive}");
            
            // Additional validation for required fields (belt and suspenders approach)
            if (model.CourseId <= 0)
            {
                ModelState.AddModelError(nameof(model.CourseId), "Please select a valid course.");
            }
            if (model.TeacherId <= 0)
            {
                ModelState.AddModelError(nameof(model.TeacherId), "Please select a valid teacher.");
            }
            if (model.SectionId <= 0)
            {
                ModelState.AddModelError(nameof(model.SectionId), "Please select a valid section.");
            }
            
            // Log validation errors for debugging
            if (!ModelState.IsValid)
            {
                var errorList = new List<string>();
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        var fieldErrors = entry.Value.Errors.Select(e => 
                            $"{entry.Key}: {(string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage)}");
                        errorList.AddRange(fieldErrors);
                        System.Diagnostics.Debug.WriteLine($"Validation Error - {entry.Key}: {string.Join(", ", entry.Value.Errors.Select(e => e.ErrorMessage ?? e.Exception?.Message))}");
                    }
                }
                
                ViewBag.Courses = await _courseService.GetAllCoursesAsync();
                ViewBag.Teachers = await _teacherRepository.GetAllAsync();
                ViewBag.Sections = await _sectionRepository.GetAllAsync();
                return View(model);
            }

            try
            {
                // Validate times
                if (model.StartTime >= model.EndTime)
                {
                    ModelState.AddModelError(string.Empty, "End time must be after start time.");
                    ViewBag.Courses = await _courseService.GetAllCoursesAsync();
                    ViewBag.Teachers = await _teacherRepository.GetAllAsync();
                    ViewBag.Sections = await _sectionRepository.GetAllAsync();
                    return View(model);
                }

                // Check for conflicts
                var hasConflict = await _timetableService.HasConflictAsync(model);
                if (hasConflict)
                {
                    ModelState.AddModelError(string.Empty, "This timetable conflicts with an existing schedule for the same teacher, section, or classroom.");
                    ViewBag.Courses = await _courseService.GetAllCoursesAsync();
                    ViewBag.Teachers = await _teacherRepository.GetAllAsync();
                    ViewBag.Sections = await _sectionRepository.GetAllAsync();
                    return View(model);
                }

                System.Diagnostics.Debug.WriteLine("Creating timetable...");
                await _timetableService.CreateTimetableAsync(model);
                System.Diagnostics.Debug.WriteLine($"Timetable created with ID: {model.Id}");
                
                TempData["Success"] = "Timetable created successfully.";
                return RedirectToAction(nameof(ManageTimetables));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating timetable: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the timetable: {ex.Message}");
                
                ViewBag.Courses = await _courseService.GetAllCoursesAsync();
                ViewBag.Teachers = await _teacherRepository.GetAllAsync();
                ViewBag.Sections = await _sectionRepository.GetAllAsync();
                return View(model);
            }
        }

        // GET: Admin/EditTimetable/5
        public async Task<IActionResult> EditTimetable(int id)
        {
            var timetable = await _timetableService.GetTimetableByIdAsync(id);
            if (timetable == null)
            {
                return NotFound();
            }

            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Teachers = await _teacherRepository.GetAllAsync();
            ViewBag.Sections = await _sectionRepository.GetAllAsync();
            return View(timetable);
        }

        // POST: Admin/EditTimetable/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTimetable(int id, Timetable model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var hasConflict = await _timetableService.HasConflictAsync(model);
                    if (hasConflict)
                    {
                        ModelState.AddModelError(string.Empty, "This timetable conflicts with an existing schedule.");
                        ViewBag.Courses = await _courseService.GetAllCoursesAsync();
                        ViewBag.Teachers = await _teacherRepository.GetAllAsync();
                        ViewBag.Sections = await _sectionRepository.GetAllAsync();
                        return View(model);
                    }

                    await _timetableService.UpdateTimetableAsync(model);
                    TempData["Success"] = "Timetable updated successfully.";
                    return RedirectToAction(nameof(ManageTimetables));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            ViewBag.Courses = await _courseService.GetAllCoursesAsync();
            ViewBag.Teachers = await _teacherRepository.GetAllAsync();
            ViewBag.Sections = await _sectionRepository.GetAllAsync();
            return View(model);
        }

        // POST: Admin/DeleteTimetable/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimetable(int id)
        {
            try
            {
                await _timetableService.DeleteTimetableAsync(id);
                TempData["Success"] = "Timetable deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(ManageTimetables));
        }

        // POST: Admin/ActivateAllStudentRegistrations
        // This fixes the "No Students Found" issue in attendance marking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAllStudentRegistrations()
        {
            try
            {
                var inactiveRegistrations = await _context.StudentCourseRegistrations
                    .Where(scr => !scr.IsActive)
                    .ToListAsync();

                if (inactiveRegistrations.Any())
                {
                    foreach (var registration in inactiveRegistrations)
                    {
                        registration.IsActive = true;
                    }
                    
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Successfully activated {inactiveRegistrations.Count} student course registration(s). Students should now appear in attendance marking.";
                }
                else
                {
                    TempData["Info"] = "All student course registrations are already active.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while activating registrations: {ex.Message}";
            }

            return RedirectToAction(nameof(AssignCoursesToStudents));
        }

        // GET: Admin/ViewStudentRegistrations
        // View all student course registrations with their status
        public async Task<IActionResult> ViewStudentRegistrations()
        {
            var registrations = await _context.StudentCourseRegistrations
                .Include(scr => scr.Student)
                    .ThenInclude(s => s.AppUser)
                .Include(scr => scr.Course)
                .OrderBy(scr => scr.Course.CourseCode)
                .ThenBy(scr => scr.Student.StudentNumber)
                .ToListAsync();

            var viewModel = registrations.GroupBy(scr => scr.Course)
                .Select(g => new
                {
                    Course = g.Key,
                    Registrations = g.ToList()
                })
                .ToList();

            return View(viewModel);
        }
    }
}

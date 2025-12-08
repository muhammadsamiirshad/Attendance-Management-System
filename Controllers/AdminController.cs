using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

        public AdminController(
            IAssignmentService assignmentService,
            ICourseService courseService,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository,
            ISectionRepository sectionRepository,
            ISessionRepository sessionRepository,
            UserManager<AppUser> userManager)
        {
            _assignmentService = assignmentService;
            _courseService = courseService;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
            _sectionRepository = sectionRepository;
            _sessionRepository = sessionRepository;
            _userManager = userManager;
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
                    // Check if student number already exists
                    var existingStudent = await _studentRepository.GetByStudentNumberAsync(model.StudentNumber);
                    if (existingStudent != null)
                    {
                        ModelState.AddModelError("StudentNumber", "Student number already exists.");
                        return View(model);
                    }

                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model);
                    }

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
                            StudentNumber = model.StudentNumber,
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
                    // Check if teacher number already exists
                    var existingTeacher = await _teacherRepository.GetByTeacherNumberAsync(model.TeacherNumber);
                    if (existingTeacher != null)
                    {
                        ModelState.AddModelError("TeacherNumber", "Teacher number already exists.");
                        return View(model);
                    }

                    // Check if email already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                        return View(model);
                    }

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
                            TeacherNumber = model.TeacherNumber,
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
                    TempData["Warning"] = $"{failCount} student(s) could not be assigned (may already be in a section).";
                }

                return RedirectToAction(nameof(AssignStudentsToSection));
            }

            model.Students = await _assignmentService.GetUnassignedStudentsAsync();
            model.Sections = await _sectionRepository.GetAllAsync();
            return View(model);
        }

        // GET: Admin/AssignSectionsToSessions
        public async Task<IActionResult> AssignSectionsToSessions()
        {
            var viewModel = new AssignSectionToSessionViewModel
            {
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
                    ModelState.AddModelError("", "Failed to assign teacher to course.");
                }
            }

            model.Teachers = await _teacherRepository.GetAllAsync();
            model.Courses = await _courseService.GetAllCoursesAsync();
            return View(model);
        }
    }
}

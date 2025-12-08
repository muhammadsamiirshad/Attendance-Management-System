using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AMS.Models
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ICourseRepository _courseRepo;
        private readonly ITimetableRepository _timetableRepo;

        public AttendanceService(IAttendanceRepository attendanceRepo, IStudentRepository studentRepo, ICourseRepository courseRepo, ITimetableRepository timetableRepo)
        {
            _attendanceRepo = attendanceRepo;
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
            _timetableRepo = timetableRepo;
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId)
        {
            return await _attendanceRepo.GetAttendanceByStudentAsync(studentId);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId)
        {
            return await _attendanceRepo.GetAttendanceByCourseAsync(courseId);
        }

        public async Task<bool> MarkAttendanceAsync(AttendanceMarkViewModel model, string markedBy)
        {
            try
            {
                // Validate attendance window
                var windowStatus = await ValidateAttendanceWindowAsync(model.CourseId, model.Date);
                if (!windowStatus.IsAllowed)
                {
                    return false;
                }

                foreach (var student in model.Students)
                {
                    var existingAttendance = await _attendanceRepo.GetAttendanceAsync(
                        student.StudentId, model.CourseId, model.Date);

                    if (existingAttendance != null)
                    {
                        existingAttendance.Status = student.IsPresent ? AttendanceStatus.Present : AttendanceStatus.Absent;
                        existingAttendance.Remarks = string.IsNullOrWhiteSpace(student.Remarks) ? null : student.Remarks;
                        await _attendanceRepo.UpdateAsync(existingAttendance);
                    }
                    else
                    {
                        var attendance = new Attendance
                        {
                            StudentId = student.StudentId,
                            CourseId = model.CourseId,
                            Date = model.Date,
                            Status = student.IsPresent ? AttendanceStatus.Present : AttendanceStatus.Absent,
                            Remarks = string.IsNullOrWhiteSpace(student.Remarks) ? null : student.Remarks,
                            CreatedBy = markedBy,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _attendanceRepo.AddAsync(attendance);
                    }
                }
                await _attendanceRepo.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<AttendanceWindowStatus> ValidateAttendanceWindowAsync(int courseId, DateTime date)
        {
            var status = new AttendanceWindowStatus();
            
            // Get the lecture for this course on this date
            var lecture = await GetUpcomingLectureAsync(courseId, date);
            
            if (lecture == null)
            {
                status.IsAllowed = false;
                var dayName = date.DayOfWeek.ToString();
                status.Message = $"No lecture scheduled for this course on {dayName} ({date:MMM dd, yyyy}). Please check the timetable or select a different date.";
                status.IsLocked = true;
                return status;
            }

            // Combine date with lecture times - use the date part only and add the time
            var lectureDate = date.Date;
            var lectureStartTime = lectureDate.Add(lecture.StartTime);
            var lectureEndTime = lectureDate.Add(lecture.EndTime);
            
            // Allow attendance ONLY from lecture start time until 10 minutes after lecture starts
            var windowStartTime = lectureStartTime; // Starts AT lecture time (not before)
            var windowEndTime = lectureStartTime.AddMinutes(10); // Ends 10 minutes after lecture STARTS
            
            var now = DateTime.Now;
            
            status.LectureStartTime = lectureStartTime;
            status.WindowStartTime = windowStartTime;
            status.LectureEndTime = lectureEndTime;
            status.WindowEndTime = windowEndTime;

            // Check if we're within the allowed window (lecture start time until 10 minutes after lecture start)
            if (now < windowStartTime)
            {
                status.IsAllowed = false;
                status.IsLocked = false;
                status.Message = $"Attendance marking will be available from {windowStartTime:hh:mm tt} (when the lecture starts). Currently it's {now:hh:mm tt}.";
                return status;
            }
            
            if (now > windowEndTime)
            {
                status.IsAllowed = false;
                status.IsLocked = true;
                status.Message = $"Attendance marking is locked. The window closed at {windowEndTime:hh:mm tt} (10 minutes after lecture started at {lectureStartTime:hh:mm tt}).";
                return status;
            }

            // We're in the valid window
            status.IsAllowed = true;
            status.IsLocked = false;
            
            var remainingTime = (windowEndTime - now).TotalMinutes;
            if (remainingTime > 5)
            {
                status.Message = $"Attendance window is open. You can mark attendance for the next {Math.Round(remainingTime)} minutes (until {windowEndTime:hh:mm tt}).";
            }
            else
            {
                status.Message = $"⚠️ Attendance window closing soon! Only {Math.Round(remainingTime)} minutes remaining.";
            }
            
            return status;
        }

        public async Task<Timetable?> GetUpcomingLectureAsync(int courseId, DateTime date)
        {
            var dayOfWeek = (DayOfWeekEnum)date.DayOfWeek;
            var timetables = await _timetableRepo.GetTimetableByDayAsync(dayOfWeek);
            
            return timetables
                .Where(t => t.CourseId == courseId && t.IsActive)
                .OrderBy(t => t.StartTime)
                .FirstOrDefault();
        }

        public async Task<AttendanceMarkViewModel> GetAttendanceMarkViewModelAsync(int courseId, DateTime date, int? sectionId = null)
        {
            var course = await _courseRepo.GetByIdAsync(courseId);
            
            // Get students - filter by section if provided
            IEnumerable<Student> students;
            if (sectionId.HasValue)
            {
                students = await _studentRepo.GetStudentsBySectionAsync(sectionId.Value);
                // Further filter to only students registered in this course
                var courseStudentIds = (await _studentRepo.GetStudentsByCourseAsync(courseId)).Select(s => s.Id);
                students = students.Where(s => courseStudentIds.Contains(s.Id));
            }
            else
            {
                students = await _studentRepo.GetStudentsByCourseAsync(courseId);
            }

            var model = new AttendanceMarkViewModel
            {
                CourseId = courseId,
                Course = course,
                Date = date,
                Students = new List<StudentAttendanceItem>()
            };

            foreach (var student in students)
            {
                var existingAttendance = await _attendanceRepo.GetAttendanceAsync(
                    student.Id, courseId, date);

                model.Students.Add(new StudentAttendanceItem
                {
                    StudentId = student.Id,
                    Student = student,
                    IsPresent = existingAttendance?.Status == AttendanceStatus.Present,
                    Remarks = existingAttendance?.Remarks ?? ""
                });
            }

            return model;
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceForReportAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate)
        {
            return await _attendanceRepo.GetAttendanceForReportAsync(studentId, courseId, startDate, endDate);
        }

        public async Task<double> GetAttendancePercentageAsync(int studentId, int courseId)
        {
            var attendances = await _attendanceRepo.GetAttendanceByCourseAsync(courseId);
            var studentAttendances = attendances.Where(a => a.StudentId == studentId).ToList();

            if (!studentAttendances.Any())
                return 0;

            var presentCount = studentAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
            return Math.Round((double)presentCount / studentAttendances.Count * 100, 2);
        }
        
        public async Task<IEnumerable<Section>> GetSectionsByTeacherAndCourseAsync(int teacherId, int courseId)
        {
            return await _timetableRepo.GetSectionsByTeacherAndCourseAsync(teacherId, courseId);
        }
    }

    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ApplicationDbContext _context;

        public CourseService(ICourseRepository courseRepo, ApplicationDbContext context)
        {
            _courseRepo = courseRepo;
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepo.GetAllAsync();
        }

        public async Task<IEnumerable<Course>> GetActiveCoursesAsync()
        {
            return await _courseRepo.GetActiveCourses();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId)
        {
            return await _courseRepo.GetCoursesByTeacherAsync(teacherId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(int studentId)
        {
            return await _courseRepo.GetCoursesByStudentAsync(studentId);
        }

        public async Task<Course?> GetCourseByIdAsync(int id)
        {
            return await _courseRepo.GetByIdAsync(id);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            await _courseRepo.AddAsync(course);
            await _courseRepo.SaveChangesAsync();
            return course;
        }

        public async Task UpdateCourseAsync(Course course)
        {
            await _courseRepo.UpdateAsync(course);
            await _courseRepo.SaveChangesAsync();
        }

        public async Task DeleteCourseAsync(int id)
        {
            await _courseRepo.DeleteAsync(id);
            await _courseRepo.SaveChangesAsync();
        }

        public async Task<bool> RegisterStudentToCourseAsync(int studentId, int courseId)
        {
            try
            {
                // Check if student is already registered
                var existingRegistration = await _context.StudentCourseRegistrations
                    .FirstOrDefaultAsync(scr => scr.StudentId == studentId && scr.CourseId == courseId);

                if (existingRegistration != null)
                {
                    // If inactive, reactivate it
                    if (!existingRegistration.IsActive)
                    {
                        existingRegistration.IsActive = true;
                        existingRegistration.RegisteredDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    return false; // Already registered
                }

                // Create new registration
                var registration = new StudentCourseRegistration
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    RegisteredDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.StudentCourseRegistrations.AddAsync(registration);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TimetableService : ITimetableService
    {
        private readonly ITimetableRepository _timetableRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly ApplicationDbContext _context;

        public TimetableService(ITimetableRepository timetableRepo, IStudentRepository studentRepo, ApplicationDbContext context)
        {
            _timetableRepo = timetableRepo;
            _studentRepo = studentRepo;
            _context = context;
        }

        public async Task<IEnumerable<Timetable>> GetTimetableByTeacherAsync(int teacherId)
        {
            return await _timetableRepo.GetTimetableByTeacherAsync(teacherId);
        }

        public async Task<IEnumerable<Timetable>> GetTimetableBySectionAsync(int sectionId)
        {
            return await _timetableRepo.GetTimetableBySectionAsync(sectionId);
        }

        public async Task<IEnumerable<Timetable>> GetTimetableByStudentAsync(int studentId)
        {
            // Get student with their course registrations
            var student = await _context.Students
                .Include(s => s.CourseRegistrations)
                    .ThenInclude(cr => cr.Course)
                .Include(s => s.StudentSections)
                    .ThenInclude(ss => ss.Section)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return new List<Timetable>();

            // Get IDs of courses the student is enrolled in
            var enrolledCourseIds = student.CourseRegistrations
                .Where(cr => cr.IsActive)
                .Select(cr => cr.CourseId)
                .ToList();

            if (!enrolledCourseIds.Any())
                return new List<Timetable>();

            // Get timetables for enrolled courses only
            var timetables = await _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .Include(t => t.Section)
                .Where(t => t.IsActive && enrolledCourseIds.Contains(t.CourseId))
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            // If student has section assignments, further filter by sections
            if (student.StudentSections?.Any() == true)
            {
                var sectionIds = student.StudentSections
                    .Where(ss => ss.IsActive)
                    .Select(ss => ss.SectionId)
                    .ToList();

                if (sectionIds.Any())
                {
                    timetables = timetables
                        .Where(t => sectionIds.Contains(t.SectionId))
                        .ToList();
                }
            }

            return timetables;
        }

        public async Task<IEnumerable<Timetable>> GetAllTimetablesAsync()
        {
            return await _timetableRepo.GetAllAsync();
        }

        public async Task<Timetable?> GetTimetableByIdAsync(int id)
        {
            return await _timetableRepo.GetByIdAsync(id);
        }

        public async Task<Timetable> CreateTimetableAsync(Timetable timetable)
        {
            await _timetableRepo.AddAsync(timetable);
            await _timetableRepo.SaveChangesAsync();
            return timetable;
        }

        public async Task UpdateTimetableAsync(Timetable timetable)
        {
            var existing = await _timetableRepo.GetByIdAsync(timetable.Id);
            if (existing != null)
            {
                // Detach the existing entity to avoid tracking conflict
                _context.Entry(existing).State = EntityState.Detached;
                
                // Now update with the new entity
                _context.Entry(timetable).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTimetableAsync(int id)
        {
            await _timetableRepo.DeleteAsync(id);
            await _timetableRepo.SaveChangesAsync();
        }

        public async Task<bool> HasConflictAsync(Timetable timetable)
        {
            var existingTimetables = await _timetableRepo.GetTimetableByDayAsync(timetable.Day);
            
            return existingTimetables.Any(t => 
                t.Id != timetable.Id &&
                t.TeacherId == timetable.TeacherId &&
                t.StartTime < timetable.EndTime &&
                t.EndTime > timetable.StartTime);
        }

        public async Task<IEnumerable<Section>> GetSectionsByTeacherAndCourseAsync(int teacherId, int courseId)
        {
            return await _timetableRepo.GetSectionsByTeacherAndCourseAsync(teacherId, courseId);
        }
    }
}

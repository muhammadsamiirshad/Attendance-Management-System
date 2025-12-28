using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AMS.Models
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _context;

        public AssignmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AssignStudentToSectionAsync(int studentId, int sectionId)
        {
            try
            {
                // Check if student is already in ANY active section
                var studentInOtherSection = await _context.StudentSections
                    .AnyAsync(ss => ss.StudentId == studentId && ss.IsActive && ss.SectionId != sectionId);

                if (studentInOtherSection)
                {
                    // Student is already in another section, cannot assign to multiple sections
                    return false;
                }

                var existing = await _context.StudentSections
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SectionId == sectionId);

                if (existing != null)
                {
                    existing.IsActive = true;
                    _context.Update(existing);
                }
                else
                {
                    var assignment = new StudentSection
                    {
                        StudentId = studentId,
                        SectionId = sectionId,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _context.StudentSections.AddAsync(assignment);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AssignSectionToSessionAsync(int sectionId, int sessionId)
        {
            try
            {
                var existing = await _context.SessionSections
                    .FirstOrDefaultAsync(ss => ss.SectionId == sectionId && ss.SessionId == sessionId);

                if (existing != null)
                {
                    existing.IsActive = true;
                    _context.Update(existing);
                }
                else
                {
                    var assignment = new SessionSection
                    {
                        SectionId = sectionId,
                        SessionId = sessionId,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _context.SessionSections.AddAsync(assignment);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AssignCourseToStudentAsync(int courseId, int studentId)
        {
            try
            {
                var existing = await _context.StudentCourseRegistrations
                    .FirstOrDefaultAsync(scr => scr.StudentId == studentId && scr.CourseId == courseId);

                if (existing != null)
                {
                    existing.IsActive = true;
                    _context.Update(existing);
                }
                else
                {
                    var registration = new StudentCourseRegistration
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        RegisteredDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _context.StudentCourseRegistrations.AddAsync(registration);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AssignTeacherToCourseAsync(int teacherId, int courseId)
        {
            try
            {
                // Check if course is already assigned to ANY active teacher
                var courseHasTeacher = await _context.CourseAssignments
                    .AnyAsync(ca => ca.CourseId == courseId && ca.IsActive && ca.TeacherId != teacherId);

                if (courseHasTeacher)
                {
                    // Course already has a teacher, one course can only have one teacher
                    return false;
                }

                var existing = await _context.CourseAssignments
                    .FirstOrDefaultAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId);

                if (existing != null)
                {
                    // Deactivate all other teachers for this course first
                    var otherAssignments = await _context.CourseAssignments
                        .Where(ca => ca.CourseId == courseId && ca.Id != existing.Id)
                        .ToListAsync();
                    
                    foreach (var otherAssignment in otherAssignments)
                    {
                        otherAssignment.IsActive = false;
                        _context.Update(otherAssignment);
                    }

                    existing.IsActive = true;
                    _context.Update(existing);
                }
                else
                {
                    // Deactivate all other teachers for this course first
                    var otherAssignments = await _context.CourseAssignments
                        .Where(ca => ca.CourseId == courseId)
                        .ToListAsync();
                    
                    foreach (var otherAssignment in otherAssignments)
                    {
                        otherAssignment.IsActive = false;
                        _context.Update(otherAssignment);
                    }

                    var newAssignment = new CourseAssignment
                    {
                        TeacherId = teacherId,
                        CourseId = courseId,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _context.CourseAssignments.AddAsync(newAssignment);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveStudentFromSectionAsync(int studentId, int sectionId)
        {
            try
            {
                var assignment = await _context.StudentSections
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SectionId == sectionId);

                if (assignment != null)
                {
                    assignment.IsActive = false;
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveTeacherFromCourseAsync(int teacherId, int courseId)
        {
            try
            {
                var assignment = await _context.CourseAssignments
                    .FirstOrDefaultAsync(ca => ca.TeacherId == teacherId && ca.CourseId == courseId);

                if (assignment != null)
                {
                    assignment.IsActive = false;
                    _context.Update(assignment);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Student>> GetUnassignedStudentsAsync()
        {
            var assignedStudentIds = await _context.StudentSections
                .Where(ss => ss.IsActive)
                .Select(ss => ss.StudentId)
                .ToListAsync();

            return await _context.Students
                .Include(s => s.AppUser)
                .Where(s => !assignedStudentIds.Contains(s.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetUnassignedCoursesForStudentAsync(int studentId)
        {
            var assignedCourseIds = await _context.StudentCourseRegistrations
                .Where(scr => scr.StudentId == studentId && scr.IsActive)
                .Select(scr => scr.CourseId)
                .ToListAsync();

            return await _context.Courses
                .Where(c => c.IsActive && !assignedCourseIds.Contains(c.Id))
                .ToListAsync();
        }
    }

    public class ReportService : IReportService
    {
        private readonly IAttendanceRepository _attendanceRepo;

        public ReportService(IAttendanceRepository attendanceRepo)
        {
            _attendanceRepo = attendanceRepo;
        }

        public async Task<IEnumerable<Attendance>> GetMonthlyAttendanceReportAsync(int? studentId, int? courseId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _attendanceRepo.GetAttendanceForReportAsync(studentId, courseId, startDate, endDate);
        }

        public async Task<IEnumerable<Attendance>> GetSemesterAttendanceReportAsync(int? studentId, int? courseId, DateTime semesterStart, DateTime semesterEnd)
        {
            return await _attendanceRepo.GetAttendanceForReportAsync(studentId, courseId, semesterStart, semesterEnd);
        }

        public async Task<IEnumerable<Attendance>> GetYearlyAttendanceReportAsync(int? studentId, int? courseId, int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            return await _attendanceRepo.GetAttendanceForReportAsync(studentId, courseId, startDate, endDate);
        }

        public async Task<Dictionary<string, object>> GetAttendanceStatisticsAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate)
        {
            var attendances = await _attendanceRepo.GetAttendanceForReportAsync(studentId, courseId, startDate, endDate);

            var stats = new Dictionary<string, object>
            {
                ["TotalRecords"] = attendances.Count(),
                ["PresentCount"] = attendances.Count(a => a.Status == AttendanceStatus.Present),
                ["AbsentCount"] = attendances.Count(a => a.Status == AttendanceStatus.Absent),
                ["LateCount"] = attendances.Count(a => a.Status == AttendanceStatus.Late),
                ["ExcusedCount"] = attendances.Count(a => a.Status == AttendanceStatus.Excused)
            };

            if (stats["TotalRecords"].ToString() != "0")
            {
                var totalRecords = (int)stats["TotalRecords"];
                var presentCount = (int)stats["PresentCount"];
                var lateCount = (int)stats["LateCount"];
                
                stats["AttendancePercentage"] = Math.Round((double)(presentCount + lateCount) / totalRecords * 100, 2);
            }
            else
            {
                stats["AttendancePercentage"] = 0.0;
            }

            return stats;
        }
    }
}

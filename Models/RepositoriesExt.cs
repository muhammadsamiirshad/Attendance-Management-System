using Microsoft.EntityFrameworkCore;

namespace AMS.Models
{
    public class SectionRepository : Repository<Section>, ISectionRepository
    {
        public SectionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Section>> GetActiveSections()
        {
            return await _dbSet.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetSectionsBySessionAsync(int sessionId)
        {
            return await _context.SessionSections
                .Where(ss => ss.SessionId == sessionId && ss.IsActive)
                .Include(ss => ss.Section)
                .Select(ss => ss.Section)
                .ToListAsync();
        }
    }

    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Session>> GetActiveSessions()
        {
            return await _dbSet.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<Session?> GetCurrentSessionAsync()
        {
            var today = DateTime.Today;
            return await _dbSet
                .Where(s => s.IsActive && s.StartDate <= today && s.EndDate >= today)
                .FirstOrDefaultAsync();
        }
    }

    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId)
                .Include(a => a.Course)
                .Include(a => a.Student)
                .ThenInclude(s => s.AppUser)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId)
        {
            return await _dbSet
                .Where(a => a.CourseId == courseId)
                .Include(a => a.Student)
                .ThenInclude(s => s.AppUser)
                .Include(a => a.Course)
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Student.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .Include(a => a.Student)
                .ThenInclude(s => s.AppUser)
                .Include(a => a.Course)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Attendance?> GetAttendanceAsync(int studentId, int courseId, DateTime date)
        {
            return await _dbSet
                .AsNoTracking() // Disable tracking to always get fresh data from database
                .FirstOrDefaultAsync(a => a.StudentId == studentId && 
                                         a.CourseId == courseId && 
                                         a.Date.Date == date.Date);
        }

        public async Task<IEnumerable<Attendance>> GetAttendanceForReportAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate)
        {
            var query = _dbSet
                .Include(a => a.Student)
                .ThenInclude(s => s.AppUser)
                .Include(a => a.Course)
                .Where(a => a.Date >= startDate && a.Date <= endDate);

            if (studentId.HasValue)
                query = query.Where(a => a.StudentId == studentId.Value);

            if (courseId.HasValue)
                query = query.Where(a => a.CourseId == courseId.Value);

            return await query
                .OrderBy(a => a.Student.FirstName)
                .ThenBy(a => a.Course.CourseName)
                .ThenByDescending(a => a.Date)
                .ToListAsync();
        }
    }

    public class TimetableRepository : Repository<Timetable>, ITimetableRepository
    {
        public TimetableRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Timetable>> GetTimetableByTeacherAsync(int teacherId)
        {
            return await _dbSet
                .Where(t => t.TeacherId == teacherId && t.IsActive)
                .Include(t => t.Course)
                .Include(t => t.Section)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Timetable>> GetTimetableBySectionAsync(int sectionId)
        {
            return await _dbSet
                .Where(t => t.SectionId == sectionId && t.IsActive)
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .ThenInclude(te => te.AppUser)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Timetable>> GetTimetableByDayAsync(DayOfWeekEnum day)
        {
            return await _dbSet
                .Where(t => t.Day == day && t.IsActive)
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .ThenInclude(te => te.AppUser)
                .Include(t => t.Section)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Timetable>> GetActiveTimetables()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .ThenInclude(te => te.AppUser)
                .Include(t => t.Section)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.StartTime)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Timetable>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Course)
                .Include(t => t.Teacher)
                .ThenInclude(te => te.AppUser)
                .Include(t => t.Section)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Section>> GetSectionsByTeacherAndCourseAsync(int teacherId, int courseId)
        {
            return await _dbSet
                .Where(t => t.TeacherId == teacherId && t.CourseId == courseId && t.IsActive)
                .Select(t => t.Section)
                .Distinct()
                .OrderBy(s => s.SectionName)
                .ToListAsync();
        }
    }
}

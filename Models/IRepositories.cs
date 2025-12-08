namespace AMS.Models
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }

    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByUserIdAsync(string userId);
        Task<Student?> GetByStudentNumberAsync(string studentNumber);
        Task<IEnumerable<Student>> GetStudentsBySectionAsync(int sectionId);
        Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId);
    }

    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<Teacher?> GetByUserIdAsync(string userId);
        Task<Teacher?> GetByTeacherNumberAsync(string teacherNumber);
        Task<IEnumerable<Teacher>> GetTeachersByCourseAsync(int courseId);
    }

    public interface ICourseRepository : IRepository<Course>
    {
        Task<Course?> GetByCourseCodeAsync(string courseCode);
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId);
        Task<IEnumerable<Course>> GetCoursesByStudentAsync(int studentId);
        Task<IEnumerable<Course>> GetActiveCourses();
    }

    public interface ISectionRepository : IRepository<Section>
    {
        Task<IEnumerable<Section>> GetActiveSections();
        Task<IEnumerable<Section>> GetSectionsBySessionAsync(int sessionId);
    }

    public interface ISessionRepository : IRepository<Session>
    {
        Task<IEnumerable<Session>> GetActiveSessions();
        Task<Session?> GetCurrentSessionAsync();
    }

    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId);
        Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId);
        Task<IEnumerable<Attendance>> GetAttendanceByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Attendance?> GetAttendanceAsync(int studentId, int courseId, DateTime date);
        Task<IEnumerable<Attendance>> GetAttendanceForReportAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate);
    }

    public interface ITimetableRepository : IRepository<Timetable>
    {
        Task<IEnumerable<Timetable>> GetTimetableByTeacherAsync(int teacherId);
        Task<IEnumerable<Timetable>> GetTimetableBySectionAsync(int sectionId);
        Task<IEnumerable<Timetable>> GetTimetableByDayAsync(DayOfWeekEnum day);
        Task<IEnumerable<Timetable>> GetActiveTimetables();
        Task<IEnumerable<Section>> GetSectionsByTeacherAndCourseAsync(int teacherId, int courseId);
    }
}

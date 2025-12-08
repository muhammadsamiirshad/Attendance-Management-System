namespace AMS.Models
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAttendanceByStudentAsync(int studentId);
        Task<IEnumerable<Attendance>> GetAttendanceByCourseAsync(int courseId);
        Task<bool> MarkAttendanceAsync(AttendanceMarkViewModel model, string markedBy);
        Task<AttendanceMarkViewModel> GetAttendanceMarkViewModelAsync(int courseId, DateTime date, int? sectionId = null);
        Task<IEnumerable<Attendance>> GetAttendanceForReportAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate);
        Task<double> GetAttendancePercentageAsync(int studentId, int courseId);
        Task<AttendanceWindowStatus> ValidateAttendanceWindowAsync(int courseId, DateTime date);
        Task<Timetable?> GetUpcomingLectureAsync(int courseId, DateTime date);
        Task<IEnumerable<Section>> GetSectionsByTeacherAndCourseAsync(int teacherId, int courseId);
    }

    public class AttendanceWindowStatus
    {
        public bool IsAllowed { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? LectureStartTime { get; set; }
        public DateTime? WindowStartTime { get; set; }
        public DateTime? LectureEndTime { get; set; }
        public DateTime? WindowEndTime { get; set; }
        public bool IsLocked { get; set; }
    }

    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<IEnumerable<Course>> GetActiveCoursesAsync();
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId);
        Task<IEnumerable<Course>> GetCoursesByStudentAsync(int studentId);
        Task<Course?> GetCourseByIdAsync(int id);
        Task<Course> CreateCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(int id);
        Task<bool> RegisterStudentToCourseAsync(int studentId, int courseId);
    }

    public interface ITimetableService
    {
        Task<IEnumerable<Timetable>> GetTimetableByTeacherAsync(int teacherId);
        Task<IEnumerable<Timetable>> GetTimetableBySectionAsync(int sectionId);
        Task<IEnumerable<Timetable>> GetTimetableByStudentAsync(int studentId);
        Task<IEnumerable<Timetable>> GetAllTimetablesAsync();
        Task<Timetable?> GetTimetableByIdAsync(int id);
        Task<Timetable> CreateTimetableAsync(Timetable timetable);
        Task UpdateTimetableAsync(Timetable timetable);
        Task DeleteTimetableAsync(int id);
        Task<bool> HasConflictAsync(Timetable timetable);
    }

    public interface IAssignmentService
    {
        Task<bool> AssignStudentToSectionAsync(int studentId, int sectionId);
        Task<bool> AssignSectionToSessionAsync(int sectionId, int sessionId);
        Task<bool> AssignCourseToStudentAsync(int courseId, int studentId);
        Task<bool> AssignTeacherToCourseAsync(int teacherId, int courseId);
        Task<bool> RemoveStudentFromSectionAsync(int studentId, int sectionId);
        Task<bool> RemoveTeacherFromCourseAsync(int teacherId, int courseId);
        Task<IEnumerable<Student>> GetUnassignedStudentsAsync();
        Task<IEnumerable<Course>> GetUnassignedCoursesForStudentAsync(int studentId);
    }

    public interface IReportService
    {
        Task<IEnumerable<Attendance>> GetMonthlyAttendanceReportAsync(int? studentId, int? courseId, int year, int month);
        Task<IEnumerable<Attendance>> GetSemesterAttendanceReportAsync(int? studentId, int? courseId, DateTime semesterStart, DateTime semesterEnd);
        Task<IEnumerable<Attendance>> GetYearlyAttendanceReportAsync(int? studentId, int? courseId, int year);
        Task<Dictionary<string, object>> GetAttendanceStatisticsAsync(int? studentId, int? courseId, DateTime startDate, DateTime endDate);
    }
}

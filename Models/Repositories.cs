using Microsoft.EntityFrameworkCore;

namespace AMS.Models
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(s => s.AppUser)
                .FirstOrDefaultAsync(s => s.AppUserId == userId);
        }

        public async Task<Student?> GetByStudentNumberAsync(string studentNumber)
        {
            return await _dbSet
                .Include(s => s.AppUser)
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);
        }

        public async Task<IEnumerable<Student>> GetStudentsBySectionAsync(int sectionId)
        {
            return await _context.StudentSections
                .Where(ss => ss.SectionId == sectionId && ss.IsActive)
                .Include(ss => ss.Student)
                .ThenInclude(s => s.AppUser)
                .Select(ss => ss.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByCourseAsync(int courseId)
        {
            System.Diagnostics.Debug.WriteLine($"===== GetStudentsByCourseAsync =====");
            System.Diagnostics.Debug.WriteLine($"Looking for students in courseId: {courseId}");
            
            // Get all registrations for this course
            var allRegistrations = await _context.StudentCourseRegistrations
                .Where(scr => scr.CourseId == courseId)
                .ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"Total registrations for course {courseId}: {allRegistrations.Count}");
            System.Diagnostics.Debug.WriteLine($"Active registrations: {allRegistrations.Count(r => r.IsActive)}");
            System.Diagnostics.Debug.WriteLine($"Inactive registrations: {allRegistrations.Count(r => !r.IsActive)}");
            
            // ⚠️ IMPORTANT: Get students with active registrations
            // Make sure to properly include Student and AppUser navigation properties
            var students = await _context.StudentCourseRegistrations
                .Where(scr => scr.CourseId == courseId && scr.IsActive)
                .Include(scr => scr.Student)
                    .ThenInclude(s => s.AppUser)
                .Include(scr => scr.Student.CourseRegistrations)
                .Include(scr => scr.Student.StudentSections)
                .Select(scr => scr.Student)
                .Distinct()
                .ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"Students returned: {students.Count}");
            
            if (students.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ ============================================");
                System.Diagnostics.Debug.WriteLine("⚠️ WARNING: No active students found for this course!");
                System.Diagnostics.Debug.WriteLine("⚠️ ============================================");
                
                if (allRegistrations.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ FOUND {allRegistrations.Count} INACTIVE REGISTRATION(S)!");
                    System.Diagnostics.Debug.WriteLine("⚠️ ");
                    System.Diagnostics.Debug.WriteLine("⚠️ SOLUTION: You need to activate these registrations!");
                    System.Diagnostics.Debug.WriteLine("⚠️ ");
                    System.Diagnostics.Debug.WriteLine("⚠️ OPTION 1: Use Admin Panel");
                    System.Diagnostics.Debug.WriteLine("⚠️   → Go to: Admin → Assign Courses to Students");
                    System.Diagnostics.Debug.WriteLine("⚠️   → Click 'Fix No Students Found Issue' button");
                    System.Diagnostics.Debug.WriteLine("⚠️ ");
                    System.Diagnostics.Debug.WriteLine("⚠️ OPTION 2: Run SQL Script");
                    System.Diagnostics.Debug.WriteLine("⚠️   → Execute: FIX_ATTENDANCE_STUDENTS_NOW.sql");
                    System.Diagnostics.Debug.WriteLine("⚠️ ");
                    System.Diagnostics.Debug.WriteLine("⚠️ OPTION 3: Quick SQL Fix");
                    System.Diagnostics.Debug.WriteLine("⚠️   → UPDATE StudentCourseRegistrations SET IsActive = 1 WHERE IsActive = 0");
                    System.Diagnostics.Debug.WriteLine("⚠️ ============================================");
                    System.Diagnostics.Debug.WriteLine("⚠️ Inactive registrations:");
                    
                    foreach (var reg in allRegistrations)
                    {
                        System.Diagnostics.Debug.WriteLine($"⚠️   - StudentId={reg.StudentId}, CourseId={reg.CourseId}, IsActive={reg.IsActive}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No registrations found at all!");
                    System.Diagnostics.Debug.WriteLine("⚠️ Students need to be enrolled in this course first.");
                }
                System.Diagnostics.Debug.WriteLine("⚠️ ============================================");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("✅ Students found:");
                foreach (var student in students)
                {
                    System.Diagnostics.Debug.WriteLine($"  ✓ Student: {student.StudentNumber} - {student.FirstName} {student.LastName}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"===== End GetStudentsByCourseAsync =====");
            return students;
        }

        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.AppUser)
                .ToListAsync();
        }

        public async Task<string> GenerateNextStudentNumberAsync()
        {
            var lastStudent = await _dbSet
                .OrderByDescending(s => s.StudentNumber)
                .FirstOrDefaultAsync();

            if (lastStudent == null || string.IsNullOrEmpty(lastStudent.StudentNumber))
            {
                return "STU-00001";
            }

            // Extract the numeric part from the last student number (e.g., "STU-00001" -> "00001")
            var lastNumberStr = lastStudent.StudentNumber.Replace("STU-", "");
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                var nextNumber = lastNumber + 1;
                return $"STU-{nextNumber:D5}"; // Format as STU-00001, STU-00002, etc.
            }

            // If parsing fails, start from 1
            return "STU-00001";
        }
    }

    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Teacher?> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(t => t.AppUserId == userId);
        }

        public async Task<Teacher?> GetByTeacherNumberAsync(string teacherNumber)
        {
            return await _dbSet
                .Include(t => t.AppUser)
                .FirstOrDefaultAsync(t => t.TeacherNumber == teacherNumber);
        }

        public async Task<IEnumerable<Teacher>> GetTeachersByCourseAsync(int courseId)
        {
            return await _context.CourseAssignments
                .Where(ca => ca.CourseId == courseId && ca.IsActive)
                .Include(ca => ca.Teacher)
                .ThenInclude(t => t.AppUser)
                .Select(ca => ca.Teacher)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.AppUser)
                .ToListAsync();
        }

        public async Task<string> GenerateNextTeacherNumberAsync()
        {
            var lastTeacher = await _dbSet
                .OrderByDescending(t => t.TeacherNumber)
                .FirstOrDefaultAsync();

            if (lastTeacher == null || string.IsNullOrEmpty(lastTeacher.TeacherNumber))
            {
                return "TCH-00001";
            }

            // Extract the numeric part from the last teacher number (e.g., "TCH-00001" -> "00001")
            var lastNumberStr = lastTeacher.TeacherNumber.Replace("TCH-", "");
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                var nextNumber = lastNumber + 1;
                return $"TCH-{nextNumber:D5}"; // Format as TCH-00001, TCH-00002, etc.
            }

            // If parsing fails, start from 1
            return "TCH-00001";
        }
    }

    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.StudentRegistrations)
                .Include(c => c.CourseAssignments)
                    .ThenInclude(ca => ca.Teacher)
                .ToListAsync();
        }

        public override async Task<Course?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.StudentRegistrations)
                .Include(c => c.CourseAssignments)
                    .ThenInclude(ca => ca.Teacher)
                .Include(c => c.Timetables)
                .Include(c => c.Attendances)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course?> GetByCourseCodeAsync(string courseCode)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId)
        {
            return await _context.CourseAssignments
                .Where(ca => ca.TeacherId == teacherId && ca.IsActive)
                .Include(ca => ca.Course)
                    .ThenInclude(c => c.StudentRegistrations)
                .Include(ca => ca.Course)
                    .ThenInclude(c => c.CourseAssignments)
                        .ThenInclude(ca2 => ca2.Teacher)
                .Select(ca => ca.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(int studentId)
        {
            return await _context.StudentCourseRegistrations
                .Where(scr => scr.StudentId == studentId && scr.IsActive)
                .Include(scr => scr.Course)
                    .ThenInclude(c => c.CourseAssignments)
                        .ThenInclude(ca => ca.Teacher)
                .Select(scr => scr.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetActiveCourses()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }
    }
}

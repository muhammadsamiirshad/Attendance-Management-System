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
            return await _context.StudentCourseRegistrations
                .Where(scr => scr.CourseId == courseId && scr.IsActive)
                .Include(scr => scr.Student)
                .ThenInclude(s => s.AppUser)
                .Select(scr => scr.Student)
                .ToListAsync();
        }

        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.AppUser)
                .ToListAsync();
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
    }

    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Course?> GetByCourseCodeAsync(string courseCode)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.CourseCode == courseCode);
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherAsync(int teacherId)
        {
            return await _context.CourseAssignments
                .Where(ca => ca.TeacherId == teacherId && ca.IsActive)
                .Include(ca => ca.Course)
                .Select(ca => ca.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByStudentAsync(int studentId)
        {
            return await _context.StudentCourseRegistrations
                .Where(scr => scr.StudentId == studentId && scr.IsActive)
                .Include(scr => scr.Course)
                .Select(scr => scr.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetActiveCourses()
        {
            return await _dbSet.Where(c => c.IsActive).ToListAsync();
        }
    }
}

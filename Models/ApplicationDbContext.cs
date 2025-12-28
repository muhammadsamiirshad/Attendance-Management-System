using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AMS.Models
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Teacher> Teachers { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<StudentSection> StudentSections { get; set; } = null!;
        public DbSet<SessionSection> SessionSections { get; set; } = null!;
        public DbSet<CourseAssignment> CourseAssignments { get; set; } = null!;
        public DbSet<StudentCourseRegistration> StudentCourseRegistrations { get; set; } = null!;
        public DbSet<Timetable> Timetables { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student relationships
            modelBuilder.Entity<Student>()
                .HasOne(s => s.AppUser)
                .WithMany()
                .HasForeignKey(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Teacher relationships
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.AppUser)
                .WithMany()
                .HasForeignKey(t => t.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentSection relationships
            modelBuilder.Entity<StudentSection>()
                .HasOne(ss => ss.Student)
                .WithMany(s => s.StudentSections)
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentSection>()
                .HasOne(ss => ss.Section)
                .WithMany(s => s.StudentSections)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // SessionSection relationships
            modelBuilder.Entity<SessionSection>()
                .HasOne(ss => ss.Session)
                .WithMany(s => s.SessionSections)
                .HasForeignKey(ss => ss.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionSection>()
                .HasOne(ss => ss.Section)
                .WithMany(s => s.SessionSections)
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // CourseAssignment relationships
            modelBuilder.Entity<CourseAssignment>()
                .HasOne(ca => ca.Teacher)
                .WithMany(t => t.CourseAssignments)
                .HasForeignKey(ca => ca.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseAssignment>()
                .HasOne(ca => ca.Course)
                .WithMany(c => c.CourseAssignments)
                .HasForeignKey(ca => ca.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // StudentCourseRegistration relationships
            modelBuilder.Entity<StudentCourseRegistration>()
                .HasOne(scr => scr.Student)
                .WithMany(s => s.CourseRegistrations)
                .HasForeignKey(scr => scr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentCourseRegistration>()
                .HasOne(scr => scr.Course)
                .WithMany(c => c.StudentRegistrations)
                .HasForeignKey(scr => scr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Ignore the CourseRegistrations property as it's just an alias
            modelBuilder.Entity<Course>()
                .Ignore(c => c.CourseRegistrations);

            // Timetable relationships
            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Timetables)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Teacher)
                .WithMany(te => te.Timetables)
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Section)
                .WithMany(s => s.Timetables)
                .HasForeignKey(t => t.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Attendance relationships
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Attendances)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentNumber)
                .IsUnique();

            modelBuilder.Entity<Teacher>()
                .HasIndex(t => t.TeacherNumber)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.CourseCode)
                .IsUnique();

            // Composite unique constraints
            modelBuilder.Entity<StudentSection>()
                .HasIndex(ss => new { ss.StudentId, ss.SectionId })
                .IsUnique();

            modelBuilder.Entity<SessionSection>()
                .HasIndex(ss => new { ss.SessionId, ss.SectionId })
                .IsUnique();

            modelBuilder.Entity<CourseAssignment>()
                .HasIndex(ca => new { ca.TeacherId, ca.CourseId })
                .IsUnique();

            modelBuilder.Entity<StudentCourseRegistration>()
                .HasIndex(scr => new { scr.StudentId, scr.CourseId })
                .IsUnique();

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.StudentId, a.CourseId, a.Date })
                .IsUnique();
        }
    }
}

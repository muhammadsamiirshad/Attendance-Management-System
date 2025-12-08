using Microsoft.AspNetCore.Identity;

namespace AMS.Models
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles
            string[] roles = { "Admin", "Teacher", "Student" };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create admin user
            var adminEmail = "admin@ams.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    FirstLogin = false,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed sample data
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedSampleData(context);
        }

        private static async Task SeedSampleData(ApplicationDbContext context)
        {
            // Seed Sections
            if (!context.Sections.Any())
            {
                var sections = new List<Section>
                {
                    new Section { SectionName = "Section A", Description = "Morning Section", Capacity = 50 },
                    new Section { SectionName = "Section B", Description = "Afternoon Section", Capacity = 45 },
                    new Section { SectionName = "Section C", Description = "Evening Section", Capacity = 40 }
                };

                await context.Sections.AddRangeAsync(sections);
                await context.SaveChangesAsync();
            }

            // Seed Sessions
            if (!context.Sessions.Any())
            {
                var sessions = new List<Session>
                {
                    new Session 
                    { 
                        SessionName = "Fall 2025", 
                        StartDate = new DateTime(2025, 9, 1), 
                        EndDate = new DateTime(2025, 12, 31),
                        IsActive = true
                    },
                    new Session 
                    { 
                        SessionName = "Spring 2026", 
                        StartDate = new DateTime(2026, 1, 1), 
                        EndDate = new DateTime(2026, 5, 31),
                        IsActive = false
                    }
                };

                await context.Sessions.AddRangeAsync(sessions);
                await context.SaveChangesAsync();
            }

            // Seed Courses
            if (!context.Courses.Any())
            {
                var courses = new List<Course>
                {
                    new Course 
                    { 
                        CourseCode = "CS101", 
                        CourseName = "Introduction to Computer Science", 
                        Description = "Basic concepts of computer science",
                        CreditHours = 3,
                        Department = "Computer Science"
                    },
                    new Course 
                    { 
                        CourseCode = "MATH201", 
                        CourseName = "Calculus I", 
                        Description = "Differential and integral calculus",
                        CreditHours = 4,
                        Department = "Mathematics"
                    },
                    new Course 
                    { 
                        CourseCode = "ENG101", 
                        CourseName = "English Composition", 
                        Description = "Academic writing and communication",
                        CreditHours = 3,
                        Department = "English"
                    }
                };

                await context.Courses.AddRangeAsync(courses);
                await context.SaveChangesAsync();
            }
        }
    }
}

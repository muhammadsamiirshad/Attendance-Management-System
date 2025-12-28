using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AMS.Models;

namespace AMS.Controllers.API
{
    [Route("api/timetable")]
    [ApiController]
    public class TimetableApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimetableApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/timetable/get-teacher?courseId=1&sectionId=2
        [HttpGet("get-teacher")]
        public async Task<IActionResult> GetTeacherForCourseAndSection([FromQuery] int courseId, [FromQuery] int? sectionId)
        {
            if (courseId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid course ID" });
            }

            try
            {
                // Find the teacher assigned to this course
                var courseAssignment = await _context.CourseAssignments
                    .Include(ca => ca.Teacher)
                    .Include(ca => ca.Course)
                    .Where(ca => ca.CourseId == courseId && ca.IsActive)
                    .FirstOrDefaultAsync();

                if (courseAssignment == null || courseAssignment.Teacher == null)
                {
                    return Ok(new 
                    { 
                        success = false, 
                        message = "No teacher is assigned to this course",
                        teacherId = (int?)null,
                        teacherName = (string?)null
                    });
                }

                return Ok(new 
                { 
                    success = true, 
                    teacherId = courseAssignment.TeacherId,
                    teacherName = $"{courseAssignment.Teacher.FirstName} {courseAssignment.Teacher.LastName} ({courseAssignment.Teacher.TeacherNumber})",
                    message = "Teacher found successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}

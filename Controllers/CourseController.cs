using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ITeacherRepository _teacherRepository;

        public CourseController(ICourseService courseService, ITeacherRepository teacherRepository)
        {
            _courseService = courseService;
            _teacherRepository = teacherRepository;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Course());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                await _courseService.CreateCourseAsync(course);
                TempData["Success"] = "Course created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                await _courseService.UpdateCourseAsync(course);
                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            TempData["Success"] = "Course deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

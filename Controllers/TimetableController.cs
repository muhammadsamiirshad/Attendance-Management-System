using AMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    [Authorize(Roles = "Admin,Teacher,Student")]
    public class TimetableController : Controller
    {
        private readonly ITimetableService _timetableService;
        private readonly ICourseService _courseService;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ISectionRepository _sectionRepository;

        public TimetableController(
            ITimetableService timetableService,
            ICourseService courseService,
            ITeacherRepository teacherRepository,
            ISectionRepository sectionRepository)
        {
            _timetableService = timetableService;
            _courseService = courseService;
            _teacherRepository = teacherRepository;
            _sectionRepository = sectionRepository;
        }

        public async Task<IActionResult> Index()
        {
            var timetables = await _timetableService.GetAllTimetablesAsync();
            
            var viewModel = new TimetableViewModel
            {
                Timetables = timetables.ToList(),
                Title = "All Timetables"
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new TimetableCreateViewModel
            {
                Courses = await _courseService.GetAllCoursesAsync(),
                Teachers = await _teacherRepository.GetAllAsync(),
                Sections = await _sectionRepository.GetAllAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(TimetableCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var timetable = new Timetable
                {
                    CourseId = model.CourseId,
                    TeacherId = model.TeacherId,
                    SectionId = model.SectionId,
                    Day = Enum.Parse<DayOfWeekEnum>(model.Day),
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Classroom = model.Classroom
                };

                var hasConflict = await _timetableService.HasConflictAsync(timetable);
                if (hasConflict)
                {
                    ModelState.AddModelError("", "Time conflict detected. Please choose a different time slot.");
                }
                else
                {
                    await _timetableService.CreateTimetableAsync(timetable);
                    TempData["Success"] = "Timetable entry created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            model.Courses = await _courseService.GetAllCoursesAsync();
            model.Teachers = await _teacherRepository.GetAllAsync();
            model.Sections = await _sectionRepository.GetAllAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var timetables = await _timetableService.GetAllTimetablesAsync();
            var timetable = timetables.FirstOrDefault(t => t.Id == id);
            
            if (timetable == null)
            {
                return NotFound();
            }

            var viewModel = new TimetableEditViewModel
            {
                Id = timetable.Id,
                CourseId = timetable.CourseId,
                TeacherId = timetable.TeacherId,
                SectionId = timetable.SectionId,
                Day = timetable.Day.ToString(),
                StartTimeString = timetable.StartTime.ToString(@"hh\:mm"),
                EndTimeString = timetable.EndTime.ToString(@"hh\:mm"),
                Classroom = timetable.Classroom,
                Courses = await _courseService.GetAllCoursesAsync(),
                Teachers = await _teacherRepository.GetAllAsync(),
                Sections = await _sectionRepository.GetAllAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(TimetableEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new timetable object with updated values
                var timetable = new Timetable
                {
                    Id = model.Id,
                    CourseId = model.CourseId,
                    TeacherId = model.TeacherId,
                    SectionId = model.SectionId,
                    Day = Enum.Parse<DayOfWeekEnum>(model.Day),
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Classroom = model.Classroom
                };

                var hasConflict = await _timetableService.HasConflictAsync(timetable);
                if (hasConflict)
                {
                    ModelState.AddModelError("", "Time conflict detected. Please choose a different time slot.");
                }
                else
                {
                    await _timetableService.UpdateTimetableAsync(timetable);
                    TempData["Success"] = "Timetable entry updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }

            model.Courses = await _courseService.GetAllCoursesAsync();
            model.Teachers = await _teacherRepository.GetAllAsync();
            model.Sections = await _sectionRepository.GetAllAsync();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _timetableService.DeleteTimetableAsync(id);
            TempData["Success"] = "Timetable entry deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

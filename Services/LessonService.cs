using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Web.Models;

namespace LMS.Web.Services;

public class LessonService
{
    private readonly ICourseRepository _courseRepository;
    private readonly NotificationService _notificationService;
    private readonly ILogger<LessonService> _logger;

    public LessonService(
        ICourseRepository courseRepository,
        NotificationService notificationService,
        ILogger<LessonService> logger)
    {
        _courseRepository = courseRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Lesson?> GetLessonAsync(int moduleId, int lessonId)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(moduleId);
            return course?.Modules
                .FirstOrDefault(m => m.Id == moduleId)?
                .Lessons
                .FirstOrDefault(l => l.Id == lessonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
            _notificationService.ShowError("Failed to load lesson");
            throw;
        }
    }

    public async Task AddLessonAsync(int moduleId, LessonModel model)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(moduleId);
            var module = course?.Modules.FirstOrDefault(m => m.Id == moduleId);

            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return;
            }

            var lesson = new Lesson
            {
                Title = model.Title,
                Content = model.Content,
                Order = model.Order,
                ModuleId = moduleId
            };

            module.Lessons.Add(lesson);
            await _courseRepository.UpdateAsync(course!);
            _notificationService.ShowSuccess("Lesson added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding lesson to module {ModuleId}", moduleId);
            _notificationService.ShowError("Failed to add lesson");
            throw;
        }
    }

    public async Task UpdateLessonAsync(LessonModel model)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(model.ModuleId);
            if (course == null)
            {
                _notificationService.ShowError("Course not found");
                return;
            }

            var module = course.Modules.FirstOrDefault(m => m.Id == model.ModuleId);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return;
            }

            var lesson = module.Lessons.FirstOrDefault(l => l.Id == model.Id);
            if (lesson == null)
            {
                _notificationService.ShowError("Lesson not found");
                return;
            }

            var oldOrder = lesson.Order;
            var newOrder = model.Order;

            // Only update orders if they've changed
            if (oldOrder != newOrder)
            {
                var lessons = module.Lessons.OrderBy(l => l.Order).ToList();
                
                // Update orders for affected lessons only
                if (oldOrder < newOrder)
                {
                    foreach (var l in lessons.Where(l => l.Order > oldOrder && l.Order <= newOrder))
                    {
                        l.Order--;
                    }
                }
                else
                {
                    foreach (var l in lessons.Where(l => l.Order >= newOrder && l.Order < oldOrder))
                    {
                        l.Order++;
                    }
                }
            }

            lesson.Title = model.Title;
            lesson.Content = model.Content;
            lesson.Order = model.Order;

            await _courseRepository.UpdateAsync(course);
            _notificationService.ShowSuccess("Lesson updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson {LessonId}", model.Id);
            _notificationService.ShowError("Failed to update lesson");
            throw;
        }
    }

    public async Task DeleteLessonAsync(int moduleId, int lessonId)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(moduleId);
            var module = course?.Modules.FirstOrDefault(m => m.Id == moduleId);
            var lesson = module?.Lessons.FirstOrDefault(l => l.Id == lessonId);

            if (lesson == null)
            {
                _notificationService.ShowError("Lesson not found");
                return;
            }

            module!.Lessons.Remove(lesson);
            await _courseRepository.UpdateAsync(course!);
            _notificationService.ShowSuccess("Lesson deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
            _notificationService.ShowError("Failed to delete lesson");
            throw;
        }
    }
} 
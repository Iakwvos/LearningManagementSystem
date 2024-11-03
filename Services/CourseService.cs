using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Web.Models;

namespace LMS.Web.Services;

public class CourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly NotificationService _notificationService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository, 
        NotificationService notificationService,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        try
        {
            return await _courseRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all courses");
            _notificationService.ShowError("Failed to load courses");
            throw;
        }
    }

    public async Task<Course?> GetCourseAsync(int id)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(id);
            if (course == null)
            {
                _notificationService.ShowWarning($"Course with ID {id} not found");
            }
            return course;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting course {CourseId}", id);
            _notificationService.ShowError("Failed to load course details");
            throw;
        }
    }

    public async Task<Course?> CreateCourseAsync(CourseModel model)
    {
        try
        {
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = model.ImageUrl,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _courseRepository.AddAsync(course);
            _notificationService.ShowSuccess("Course created successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            _notificationService.ShowError("Failed to create course");
            throw;
        }
    }

    public async Task UpdateCourseAsync(int id, CourseModel model)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id);
            if (course == null)
            {
                _notificationService.ShowError("Course not found");
                return;
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.ImageUrl = model.ImageUrl;
            course.UpdatedDate = DateTime.UtcNow;

            await _courseRepository.UpdateAsync(course);
            _notificationService.ShowSuccess("Course updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", id);
            _notificationService.ShowError("Failed to update course");
            throw;
        }
    }

    public async Task AddModuleAsync(int courseId, ModuleModel model)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
            {
                _notificationService.ShowError("Course not found");
                return;
            }

            var module = new Module
            {
                Title = model.Title,
                Description = model.Description,
                Order = model.Order,
                CourseId = courseId
            };

            course.Modules.Add(module);
            await _courseRepository.UpdateAsync(course);
            _notificationService.ShowSuccess("Module added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding module to course {CourseId}", courseId);
            _notificationService.ShowError("Failed to add module");
            throw;
        }
    }

    public async Task UpdateModuleAsync(ModuleModel model)
    {
        try
        {
            _logger.LogInformation("Updating module {ModuleId} for course {CourseId}", model.Id, model.CourseId);
            
            var course = await _courseRepository.GetCourseWithModulesAsync(model.CourseId);
            if (course == null)
            {
                _notificationService.ShowError("Course not found");
                return;
            }

            var module = course.Modules.FirstOrDefault(m => m.Id == model.Id);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return;
            }

            // Update module properties
            module.Title = model.Title;
            module.Description = model.Description;

            // Handle reordering if the order has changed
            if (module.Order != model.Order)
            {
                var modules = course.Modules.OrderBy(m => m.Order).ToList();
                
                // Remove from old position
                modules.Remove(module);
                
                // Insert at new position
                var newIndex = Math.Min(Math.Max(0, model.Order - 1), modules.Count);
                modules.Insert(newIndex, module);
                
                // Update all module orders
                for (int i = 0; i < modules.Count; i++)
                {
                    modules[i].Order = i + 1;
                }
            }

            await _courseRepository.UpdateAsync(course);
            _notificationService.ShowSuccess("Module updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module {ModuleId}", model.Id);
            _notificationService.ShowError("Failed to update module");
            throw;
        }
    }

    public async Task DeleteModuleAsync(int courseId, int moduleId)
    {
        try
        {
            var course = await _courseRepository.GetCourseWithModulesAsync(courseId);
            if (course == null)
            {
                _notificationService.ShowError("Course not found");
                return;
            }

            var module = course.Modules.FirstOrDefault(m => m.Id == moduleId);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return;
            }

            course.Modules.Remove(module);
            await _courseRepository.UpdateAsync(course);
            _notificationService.ShowSuccess("Module deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module {ModuleId} from course {CourseId}", moduleId, courseId);
            _notificationService.ShowError("Failed to delete module");
            throw;
        }
    }
} 
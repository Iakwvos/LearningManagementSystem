using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Web.Models;
using Microsoft.Extensions.Logging;

namespace LMS.Web.Services;

public class LessonService
{
    private readonly IModuleRepository _moduleRepository;
    private readonly ILogger<LessonService> _logger;
    private readonly NotificationService _notificationService;
    private readonly IFileService _fileService;

    public LessonService(
        IModuleRepository moduleRepository,
        ILogger<LessonService> logger,
        NotificationService notificationService,
        IFileService fileService)
    {
        _moduleRepository = moduleRepository;
        _logger = logger;
        _notificationService = notificationService;
        _fileService = fileService;
    }

    public async Task<bool> CreateLessonAsync(LessonModel model)
    {
        try
        {
            var module = await _moduleRepository.GetModuleWithLessonsAsync(model.ModuleId);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return false;
            }

            var lesson = new Lesson
            {
                Title = model.Title,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Order = module.Lessons.Any() ? module.Lessons.Max(l => l.Order) + 1 : 1
            };

            if (model.Attachments != null)
            {
                lesson.Attachments = model.Attachments.Select(a => new Attachment
                {
                    Title = a.Title,
                    FileUrl = a.FileUrl,
                    FileType = a.FileType,
                    FileSize = a.FileSize
                }).ToList();
            }

            module.Lessons.Add(lesson);
            await _moduleRepository.UpdateAsync(module);

            _notificationService.ShowSuccess("Lesson created successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson");
            _notificationService.ShowError($"Error creating lesson: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateLessonAsync(LessonModel model)
    {
        try
        {
            var module = await _moduleRepository.GetModuleWithLessonsAsync(model.ModuleId);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return false;
            }

            var lesson = module.Lessons.FirstOrDefault(l => l.Id == model.Id);
            if (lesson == null)
            {
                _notificationService.ShowError("Lesson not found");
                return false;
            }

            // Update basic properties
            lesson.Title = model.Title;
            lesson.Content = model.Content;

            // Handle attachments
            var existingAttachments = lesson.Attachments.ToList();
            var newAttachments = model.Attachments?.ToList() ?? new List<AttachmentModel>();

            // Remove deleted attachments
            foreach (var attachment in existingAttachments)
            {
                if (!newAttachments.Any(a => a.Id == attachment.Id))
                {
                    await _fileService.DeleteFileAsync(attachment.FileUrl);
                    lesson.Attachments.Remove(attachment);
                }
            }

            // Add new attachments
            foreach (var attachmentModel in newAttachments.Where(a => a.Id == 0))
            {
                lesson.Attachments.Add(new Attachment
                {
                    Title = attachmentModel.Title,
                    FileUrl = attachmentModel.FileUrl,
                    FileType = attachmentModel.FileType,
                    FileSize = attachmentModel.FileSize
                });
            }

            await _moduleRepository.UpdateAsync(module);
            _notificationService.ShowSuccess("Lesson updated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson");
            _notificationService.ShowError($"Error updating lesson: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteLessonAsync(int lessonId, int moduleId)
    {
        try
        {
            var module = await _moduleRepository.GetModuleWithLessonsAsync(moduleId);
            if (module == null)
            {
                _notificationService.ShowError("Module not found");
                return false;
            }

            var lesson = module.Lessons.FirstOrDefault(l => l.Id == lessonId);
            if (lesson == null)
            {
                _notificationService.ShowError("Lesson not found");
                return false;
            }

            // Delete attachments
            foreach (var attachment in lesson.Attachments)
            {
                await _fileService.DeleteFileAsync(attachment.FileUrl);
            }

            module.Lessons.Remove(lesson);
            await _moduleRepository.UpdateAsync(module);

            _notificationService.ShowSuccess("Lesson deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lesson");
            _notificationService.ShowError($"Error deleting lesson: {ex.Message}");
            return false;
        }
    }
} 
using System.ComponentModel.DataAnnotations;

namespace LMS.Web.Models;

public class LessonModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters")]
    public string Content { get; set; } = string.Empty;

    public int Order { get; set; }

    [Required(ErrorMessage = "ModuleId is required")]
    public int ModuleId { get; set; }

    public List<AttachmentModel> Attachments { get; set; } = new();
} 
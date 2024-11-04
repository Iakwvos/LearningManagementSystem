using System.ComponentModel.DataAnnotations;

namespace LMS.Web.Models;

public class LessonModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required")]
    public string Content { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0")]
    public int Order { get; set; }

    public int ModuleId { get; set; }
    public List<AttachmentModel> Attachments { get; set; } = new();
} 
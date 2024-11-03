using System.ComponentModel.DataAnnotations;

namespace LMS.Web.Models;

public class ModuleModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Order must be greater than 0")]
    public int Order { get; set; }

    public int CourseId { get; set; }
} 
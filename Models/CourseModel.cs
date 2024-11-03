using System.ComponentModel.DataAnnotations;

namespace LMS.Web.Models;

public class CourseModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please enter a valid URL")]
    public string ImageUrl { get; set; } = string.Empty;
} 
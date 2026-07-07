namespace CampusTrade.Backend.Models.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public int SortOrder { get; set; }

    public List<CategoryDto> Children { get; set; } = new();
}
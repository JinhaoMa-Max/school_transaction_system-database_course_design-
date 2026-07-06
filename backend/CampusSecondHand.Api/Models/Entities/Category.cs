namespace CampusSecondHand.Api.Models.Entities;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

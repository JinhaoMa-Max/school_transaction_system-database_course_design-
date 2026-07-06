namespace CampusSecondHand.Api.Models.Requests;

public class UpdateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
}

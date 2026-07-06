namespace CampusSecondHand.Api.Models.Requests;

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
}

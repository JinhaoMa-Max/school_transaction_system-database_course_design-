namespace CampusTrade.Backend.Models.DTOs;

public class GoodsImageDto
{
    public int ImageId { get; set; }
    public int GoodsId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
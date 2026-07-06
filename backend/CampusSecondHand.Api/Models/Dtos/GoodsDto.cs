namespace CampusSecondHand.Api.Models.Dtos;

public class GoodsDto
{
    public int GoodsId { get; set; }
    public int SellerId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public string PublishTime { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? SellerNickname { get; set; }
    public string? CategoryName { get; set; }
}

public class GoodsListResult
{
    public List<GoodsDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

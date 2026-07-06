namespace CampusSecondHand.Api.Models.Entities;

public class Goods
{
    public int GoodsId { get; set; }
    public int SellerId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string GoodsCondition { get; set; } = string.Empty;
    public string GoodsStatus { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    // from v_goods_list join
    public string? SellerName { get; set; }
    public string? CategoryName { get; set; }
    public string? CoverImage { get; set; }
}

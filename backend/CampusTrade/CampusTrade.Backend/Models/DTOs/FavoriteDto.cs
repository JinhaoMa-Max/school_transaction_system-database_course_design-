namespace CampusTrade.Backend.Models.DTOs;

/// <summary>收藏 DTO — 对应 favorite 表 + v_goods_list 视图</summary>
public class FavoriteDto
{
    public int FavoriteId { get; set; }
    public int UserId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }     // 商品名（JOIN v_goods_list）
    public decimal? GoodsPrice { get; set; }    // 当前价格（JOIN v_goods_list）
    public string? CoverImage { get; set; }     // 商品首图（JOIN v_goods_list）
    public string? GoodsStatus { get; set; }     // 商品状态（已售/在售）
    public DateTime CreateTime { get; set; }    // favorite.created_at
}

public class FavoriteCheckResult { public bool Favorited { get; set; } }

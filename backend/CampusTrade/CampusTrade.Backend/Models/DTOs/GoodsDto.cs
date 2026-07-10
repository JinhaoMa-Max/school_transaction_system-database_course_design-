namespace CampusTrade.Backend.Models.DTOs;

public class GoodsDto
{
    public int GoodsId { get; set; }
    public int SellerId { get; set; }
    public string SellerNickname { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty; // 全新/几乎全新/轻微使用/明显痕迹
    public string Status { get; set; } = string.Empty;    // pending/approved/rejected/locked/sold/offline
    public int ViewCount { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("publishTime")]
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; } // 封面图（第一张）
}

public class GoodsListResult
{
    public List<GoodsDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class CreateGoodsRequest
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Condition { get; set; } = string.Empty; // 取值需校验
}

public class UpdateGoodsRequest
{
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Condition { get; set; }
    // 注意：Status 不由前端更新，由审核/下架/锁定等操作控制
}

public class AuditGoodsRequest
{
    public string Status { get; set; } = string.Empty; // "approved" 或 "rejected"
    public string? Remark { get; set; } // 驳回原因等
}

public class UploadImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
}
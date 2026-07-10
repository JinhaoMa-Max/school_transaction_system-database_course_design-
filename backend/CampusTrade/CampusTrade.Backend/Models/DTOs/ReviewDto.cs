namespace CampusTrade.Backend.Models.DTOs;

/// <summary>评价 DTO — 对应 review 表 + v_review_detail 视图</summary>
public class ReviewDto
{
    public int ReviewId { get; set; }
    public int OrderId { get; set; }
    public int ReviewerId { get; set; }
    public string? ReviewerName { get; set; }       // 评价者昵称（v_review_detail JOIN）
    public int ReviewedUserId { get; set; }
    public string? ReviewedUserName { get; set; }   // 被评价者昵称
    public int Rating { get; set; }                 // 1-5 星
    public string? Content { get; set; }            // 评价内容（CLOB）
    public DateTime CreateTime { get; set; }        // created_at
    public int? GoodsId { get; set; }               // 对应商品ID（v_review_detail JOIN）
    public string? GoodsTitle { get; set; }         // 对应商品名
    public bool IsFollowUp { get; set; }            // 是否为追评（同一订单+评价者的第二条评价）
}

public class ReviewListResult { public List<ReviewDto> List { get; set; } = new(); public int Total { get; set; } public int Page { get; set; } public int Size { get; set; } }

/// <summary>创建评价请求</summary>
public class CreateReviewRequest { public int OrderId { get; set; } public int ReviewedUserId { get; set; } public int Rating { get; set; } public string? Content { get; set; } }

/// <summary>更新评价请求</summary>
public class UpdateReviewRequest { public int Rating { get; set; } public string? Content { get; set; } }

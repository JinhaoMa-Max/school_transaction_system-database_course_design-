namespace CampusTrade.Backend.Models.DTOs;

// ============================================================
// 订单 + 面交 DTO — 对应 trade_order / appointment 表 + v_order_list 视图
// ============================================================

/// <summary>订单 DTO — 前端 TradeOrder 类型</summary>
public class OrderDto
{
    public int OrderId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }         // 商品名（v_order_list JOIN）
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }           // 买家昵称
    public int SellerId { get; set; }
    public string? SellerName { get; set; }          // 卖家昵称
    public decimal FinalPrice { get; set; }          // final_price → 前端 dealPrice
    public string Status { get; set; } = string.Empty; // order_status
    public DateTime CreateTime { get; set; }
    public DateTime? MeetTime { get; set; }          // 面交时间（v_order_list LEFT JOIN）
    public string? MeetPlace { get; set; }           // 面交地点 → 前端 meetLocation
    public string? ConfirmCode { get; set; }         // 确认码
    public bool BuyerReviewed { get; set; }           // 买家是否已评价
    public bool SellerReviewed { get; set; }          // 卖家是否已评价
}

public class OrderListResult { public List<OrderDto> List { get; set; } = new(); public int Total { get; set; } public int Page { get; set; } public int Size { get; set; } }

/// <summary>下单请求</summary>
public class CreateOrderRequest { public int GoodsId { get; set; } public decimal Price { get; set; } }

/// <summary>完成面交请求</summary>
public class CompleteOrderRequest { public string ConfirmCode { get; set; } = string.Empty; }

/// <summary>面交预约 DTO</summary>
public class AppointmentDto
{
    public int AppointmentId { get; set; }
    public int OrderId { get; set; }
    public DateTime MeetTime { get; set; }
    public string MeetPlace { get; set; } = string.Empty;   // → 前端 meetLocation
    public string ConfirmCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;      // appointment_status
    public DateTime CreateTime { get; set; }
}

/// <summary>创建面交请求</summary>
public class CreateAppointmentRequest { public int OrderId { get; set; } public DateTime MeetTime { get; set; } public string MeetPlace { get; set; } = string.Empty; }

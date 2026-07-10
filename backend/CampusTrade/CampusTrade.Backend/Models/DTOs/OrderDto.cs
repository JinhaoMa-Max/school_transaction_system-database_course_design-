namespace CampusTrade.Backend.Models.DTOs;

public class OrderDto
{
    public int OrderId { get; set; }
    public int GoodsId { get; set; }
    public string? GoodsTitle { get; set; }
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public int SellerId { get; set; }
    public string? SellerName { get; set; }
    public decimal DealPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public DateTime? MeetTime { get; set; }
    public string? MeetLocation { get; set; }
    public string? ConfirmCode { get; set; }
    public int BuyerReviewed { get; set; }          // 0=未评, 1=首评, 2=首评+追评
    public int SellerReviewed { get; set; }
}

public class OrderListResult
{
    public List<OrderDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

public class CreateOrderRequest
{
    public int GoodsId { get; set; }
    public decimal DealPrice { get; set; }
}

public class UpdateOrderRequest
{
    public decimal? DealPrice { get; set; }
    public string? Status { get; set; }
}

public class CompleteOrderRequest
{
    public string ConfirmCode { get; set; } = string.Empty;
}

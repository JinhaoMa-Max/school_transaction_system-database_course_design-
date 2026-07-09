namespace CampusTrade.Backend.Models.DTOs;

public class TradeOrderDto
{
    public int OrderId { get; set; }
    public int GoodsId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public decimal DealPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

public class OrderListResult
{
    public List<TradeOrderDto> List { get; set; } = new();
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

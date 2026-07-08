namespace CampusTrade.Backend.Models.DTOs;

// 议价记录（对齐 docs/backend-rebuild-contract.md 中 BargainOffer 定义）
public class BargainOfferDto
{
    public int BargainId { get; set; }
    public int GoodsId { get; set; }
    public int BuyerId { get; set; }
    public decimal OfferPrice { get; set; }
    public string SellerResult { get; set; } = string.Empty;
    public decimal? CounterPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
}

// 分页返回结构（对齐前端 PageResult<T>：list/total/page/size）
public class BargainListResult
{
    public List<BargainOfferDto> List { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}

// 创建议价入参：POST /api/bargains
public class CreateBargainRequest
{
    public int GoodsId { get; set; }
    public decimal OfferPrice { get; set; }
}

// 卖家处理议价入参：PUT /api/bargains/{bargainId}/handle
public class HandleBargainRequest
{
    public string SellerResult { get; set; } = string.Empty;
    public decimal? CounterPrice { get; set; }
}

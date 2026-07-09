using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IBargainService
{
    // 议价列表分页查询
    Task<BargainListResult> GetPagedAsync(
        int page,
        int size,
        int? goodsId,
        int? buyerId,
        int? sellerId,
        string? status);

    // 议价详情
    Task<BargainOfferDto?> GetByIdAsync(int bargainId);

    // 买家创建议价（currentBuyerId 来自 token）
    Task<BargainOfferDto> CreateAsync(CreateBargainRequest request, int? currentBuyerId);

    // 卖家处理议价（accepted/rejected/countered）
    Task<BargainOfferDto> HandleAsync(int bargainId, HandleBargainRequest request, int? currentUserId);

    // 买家响应卖家还价（accepted/rejected/countered）
    Task<BargainOfferDto> BuyerHandleAsync(int bargainId, BuyerHandleBargainRequest request, int? currentUserId);

    // 关闭议价（买家/卖家均可触发，具体权限在实现中校验）
    Task<bool> CloseAsync(int bargainId, int? currentUserId);
}

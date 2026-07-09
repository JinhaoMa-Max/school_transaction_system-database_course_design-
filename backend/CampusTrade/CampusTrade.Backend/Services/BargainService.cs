using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class BargainService : IBargainService
{
    private readonly IBargainRepository _bargainRepository;
    private readonly IGoodsRepository _goodsRepository;

    public BargainService(IBargainRepository bargainRepository, IGoodsRepository goodsRepository)
    {
        _bargainRepository = bargainRepository;
        _goodsRepository = goodsRepository;
    }

    public async Task<BargainListResult> GetPagedAsync(
        int page,
        int size,
        int? goodsId,
        int? buyerId,
        string? status)
    {
        // 参数兜底校验（避免无意义分页与潜在数据库压力）
        if (page < 1) throw new ArgumentException("page 必须从 1 开始");
        if (size < 1) throw new ArgumentException("size 必须大于 0");

        var (items, total) = await _bargainRepository.GetPagedAsync(page, size, goodsId, buyerId, status);
        return new BargainListResult
        {
            List = items,
            Total = total,
            Page = page,
            Size = size
        };
    }

    public Task<BargainOfferDto?> GetByIdAsync(int bargainId)
    {
        return _bargainRepository.GetByIdAsync(bargainId);
    }

    public async Task<BargainOfferDto> CreateAsync(CreateBargainRequest request, int? currentBuyerId)
    {
        // 未登录不允许创建议价
        if (currentBuyerId == null)
        {
            throw new UnauthorizedAccessException("未登录");
        }

        if (request.GoodsId <= 0) throw new ArgumentException("goodsId 不合法");
        if (request.OfferPrice <= 0) throw new ArgumentException("offerPrice 必须大于 0");

        // 校验商品存在，并限制不能对自己商品议价
        var goods = await _goodsRepository.GetByIdAsync(request.GoodsId);
        if (goods == null) throw new ArgumentException("商品不存在");
        if (goods.SellerId == currentBuyerId.Value) throw new InvalidOperationException("不能对自己发布的商品发起议价");

        return await _bargainRepository.CreateAsync(request.GoodsId, currentBuyerId.Value, request.OfferPrice);
    }

    public async Task<BargainOfferDto> HandleAsync(int bargainId, HandleBargainRequest request, int? currentUserId)
    {
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("未登录");
        }

        if (bargainId <= 0) throw new ArgumentException("bargainId 不合法");

        // sellerResult 枚举校验（对齐前端 accepted/rejected/countered）
        var sellerResult = request.SellerResult?.Trim();
        if (sellerResult != "accepted" && sellerResult != "rejected" && sellerResult != "countered")
        {
            throw new ArgumentException("sellerResult 必须为 accepted / rejected / countered");
        }

        // 还价时 counterPrice 必填
        if (sellerResult == "countered")
        {
            if (request.CounterPrice == null || request.CounterPrice <= 0)
                throw new ArgumentException("counterPrice 在 countered 时必填且必须大于 0");
        }

        // 校验议价存在，并校验当前用户是卖家
        var bargain = await _bargainRepository.GetByIdAsync(bargainId);
        if (bargain == null) throw new ArgumentException("议价不存在");

        var goods = await _goodsRepository.GetByIdAsync(bargain.GoodsId);
        if (goods == null) throw new ArgumentException("关联商品不存在");

        if (goods.SellerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("只有卖家本人可以处理议价");
        }

        return await _bargainRepository.RespondAsync(bargainId, sellerResult, request.CounterPrice);
    }

    public async Task<BargainOfferDto> BuyerHandleAsync(int bargainId, BuyerHandleBargainRequest request, int? currentUserId)
    {
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("未登录");
        }

        if (bargainId <= 0) throw new ArgumentException("bargainId 不合法");

        // 买家对卖家还价进行确认：接受/拒绝/再次出价
        var buyerResult = request.BuyerResult?.Trim();
        if (buyerResult != "accepted" && buyerResult != "rejected" && buyerResult != "countered")
        {
            throw new ArgumentException("buyerResult 必须为 accepted / rejected / countered");
        }

        if (buyerResult == "countered")
        {
            if (request.OfferPrice == null || request.OfferPrice <= 0)
                throw new ArgumentException("offerPrice 在 countered 时必填且必须大于 0");
        }

        var bargain = await _bargainRepository.GetByIdAsync(bargainId);
        if (bargain == null) throw new ArgumentException("议价不存在");

        if (bargain.BuyerId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException("只有买家本人可以响应卖家还价");
        }

        // 当前设计下，买家响应接口主要用于处理卖家已给出 counterPrice 的场景
        if (bargain.SellerResult != "countered")
        {
            throw new InvalidOperationException("当前议价未处于卖家还价状态，无法进行买家响应");
        }

        return await _bargainRepository.BuyerHandleAsync(bargainId, buyerResult, request.OfferPrice);
    }

    public async Task<bool> CloseAsync(int bargainId, int? currentUserId)
    {
        if (currentUserId == null)
        {
            throw new UnauthorizedAccessException("未登录");
        }

        if (bargainId <= 0) throw new ArgumentException("bargainId 不合法");

        var bargain = await _bargainRepository.GetByIdAsync(bargainId);
        if (bargain == null) return false;

        var goods = await _goodsRepository.GetByIdAsync(bargain.GoodsId);
        if (goods == null) throw new ArgumentException("关联商品不存在");

        // 仅买家/卖家可关闭议价
        var uid = currentUserId.Value;
        if (uid != bargain.BuyerId && uid != goods.SellerId)
        {
            throw new UnauthorizedAccessException("只有买家或卖家可以关闭议价");
        }

        return await _bargainRepository.CloseAsync(bargainId);
    }
}

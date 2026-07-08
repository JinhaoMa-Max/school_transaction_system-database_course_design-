using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 收藏模块 Repository 接口（F10）
/// 数据库: favorite 表 / fn_favorite_goods_ids / v_goods_list
/// </summary>
public interface IFavoriteRepository
{
    /// <summary>收藏列表（含商品名、价格、首图）</summary>
    Task<(List<FavoriteDto> Items, int Total)> GetPagedAsync(int userId, int page, int size);

    /// <summary>添加收藏</summary>
    Task<int> AddAsync(int userId, int goodsId);

    /// <summary>取消收藏（按收藏ID）</summary>
    Task<bool> RemoveAsync(int favoriteId);

    /// <summary>取消收藏（按用户+商品）</summary>
    Task<bool> RemoveByGoodsAsync(int userId, int goodsId);

    /// <summary>是否已收藏</summary>
    Task<bool> IsFavoritedAsync(int userId, int goodsId);

    /// <summary>获取用户收藏的商品ID列表（逗号分隔）— 调用 fn_favorite_goods_ids</summary>
    Task<string> GetFavoriteGoodsIdsAsync(int userId);
}

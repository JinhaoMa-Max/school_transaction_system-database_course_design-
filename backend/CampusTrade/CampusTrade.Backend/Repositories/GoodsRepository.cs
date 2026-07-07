using System.Collections.Generic;
using System.Threading.Tasks;
using CampusTrade.Backend.Models.Entities;

namespace CampusTrade.Backend.Repositories.Interfaces;

/// <summary>
/// 商品数据仓储接口
/// 负责所有商品相关表的数据库访问操作（goods + goods_image）
/// 实现类应使用 Dapper + Oracle
/// </summary>
public interface IGoodsRepository
{
    // ============================================================
    //  商品主表（goods）操作
    // ============================================================

    /// <summary>
    /// 根据商品 ID 获取商品完整信息（不含图片列表）
    /// </summary>
    /// <param name="id">商品 ID（主键）</param>
    /// <returns>
    /// 商品实体 <see cref="Goods"/>，如果不存在则返回 null
    /// </returns>
    /// <remarks>
    /// 仅查询 goods 表，不关联 goods_image。
    /// 图片列表请调用 <see cref="GetImagesByGoodsIdAsync"/> 单独获取。
    /// </remarks>
    Task<Goods?> GetByIdAsync(int id);

    /// <summary>
    /// 多条件搜索商品列表（仅返回已审核通过的商品）
    /// </summary>
    /// <param name="keyword">搜索关键词，匹配标题和描述（模糊匹配），可为 null 或空</param>
    /// <param name="categoryId">分类 ID，精确匹配，可为 null</param>
    /// <param name="minPrice">最低价格，可为 null</param>
    /// <param name="maxPrice">最高价格，可为 null</param>
    /// <param name="condition">成色（如 全新、几乎全新、轻微使用、明显痕迹），精确匹配，可为 null</param>
    /// <param name="sortBy">排序字段：price / view_count / created_at，默认 created_at</param>
    /// <param name="ascending">true 为升序，false 为降序（默认降序，即最新优先）</param>
    /// <param name="page">页码，从 1 开始</param>
    /// <param name="pageSize">每页记录数</param>
    /// <returns>符合条件的商品列表（不含图片）</returns>
    /// <remarks>
    /// 业务约束：
    /// - 只查询 goods_status = 'approved' 的商品（前台可见）
    /// - 关键词匹配 LOWER(title) 或 LOWER(description) 包含 LOWER(keyword)
    /// - 价格区间包含边界值（price BETWEEN minPrice AND maxPrice）
    /// - 排序字段非法时回退到 created_at
    /// - 使用 OFFSET / FETCH 实现分页
    /// </remarks>
    Task<List<Goods>> SearchAsync(
        string? keyword,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? condition,
        string? sortBy,
        bool ascending,
        int page,
        int pageSize
    );

    /// <summary>
    /// 统计满足搜索条件的商品总数（用于分页）
    /// </summary>
    /// <param name="keyword">同 <see cref="SearchAsync"/></param>
    /// <param name="categoryId">同 <see cref="SearchAsync"/></param>
    /// <param name="minPrice">同 <see cref="SearchAsync"/></param>
    /// <param name="maxPrice">同 <see cref="SearchAsync"/></param>
    /// <param name="condition">同 <see cref="SearchAsync"/></param>
    /// <returns>符合条件的记录总数</returns>
    /// <remarks>
    /// 查询条件必须与 <see cref="SearchAsync"/> 保持一致，
    /// 保证前端分页数据准确。
    /// </remarks>
    Task<int> CountSearchAsync(
        string? keyword,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? condition
    );

    /// <summary>
    /// 创建新商品（插入 goods 表）
    /// </summary>
    /// <param name="goods">商品实体对象（应包含除 goods_id 外的所有字段）</param>
    /// <returns>新生成的自增商品 ID（goods_id）</returns>
    /// <remarks>
    /// - 使用 RETURNING goods_id INTO :new_id 获取插入后的 ID
    /// - created_at 和 updated_at 应由数据库自动填充 SYSDATE
    /// - 调用前需确保 <see cref="Goods.SellerId"/> 和 <see cref="Goods.CategoryId"/> 有效
    /// - 状态默认由 Service 层设置（通常为 'pending'）
    /// </remarks>
    Task<int> CreateAsync(Goods goods);

    /// <summary>
    /// 更新商品基本信息（不包含状态和浏览量）
    /// </summary>
    /// <param name="goods">包含更新数据的商品实体（必须设置 GoodsId）</param>
    /// <returns>true 表示更新成功（影响行数 > 0），false 表示商品不存在或未发生变更</returns>
    /// <remarks>
    /// 更新字段：
    /// - category_id, title, description, price, condition
    /// - updated_at 自动更新为 SYSDATE
    /// 不更新字段：
    /// - seller_id（卖家不可变更）
    /// - goods_status（由专门方法 <see cref="UpdateStatusAsync"/> 管理）
    /// - view_count（由 <see cref="IncrementViewCountAsync"/> 管理）
    /// - created_at（不变）
    /// </remarks>
    Task<bool> UpdateAsync(Goods goods);

    /// <summary>
    /// 物理删除商品（硬删除，从 goods 表中移除记录）
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>true 表示删除成功（影响行数 > 0）</returns>
    /// <remarks>
    /// ⚠️ 警告：此操作不可恢复！
    /// 业务层应确保：
    /// - 商品状态为 'offline' 或 'rejected' 时才允许删除
    /// - 确认无关联的有效订单
    /// - 删除前应先调用 <see cref="DeleteImagesByGoodsIdAsync"/> 清理图片
    /// </remarks>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// 更新商品状态（状态流转管理）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="status">新状态，取值范围见 <see cref="Goods.GoodsStatus"/></param>
    /// <returns>true 表示更新成功（影响行数 > 0）</returns>
    /// <remarks>
    /// 状态流转规则（由 Service 层校验）：
    /// - pending → approved / rejected
    /// - approved → locked / offline
    /// - locked → approved / sold
    /// - offline / rejected → pending（重新提交审核）
    /// - sold 为终态，不可变更
    /// 方法本身不校验合法性，仅执行 UPDATE。
    /// </remarks>
    Task<bool> UpdateStatusAsync(int goodsId, string status);

    /// <summary>
    /// 检查商品是否存在
    /// </summary>
    /// <param name="id">商品 ID</param>
    /// <returns>true 表示存在，false 表示不存在</returns>
    /// <remarks>
    /// 用于 Service 层的快速校验，不区分商品状态。
    /// </remarks>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// 增加商品浏览次数（原子操作）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>true 表示更新成功（影响行数 > 0）</returns>
    /// <remarks>
    /// UPDATE goods SET view_count = view_count + 1 WHERE goods_id = :id
    /// 无需检查商品是否存在，由调用方保证。
    /// </remarks>
    Task<bool> IncrementViewCountAsync(int goodsId);

    // ============================================================
    //  商品图片表（goods_image）操作
    // ============================================================

    /// <summary>
    /// 获取商品的所有图片列表
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>图片列表（按 sort_order 升序排列），若无图片则返回空集合</returns>
    /// <remarks>
    /// 仅查询 goods_image 表，不校验商品是否存在。
    /// </remarks>
    Task<List<GoodsImage>> GetImagesByGoodsIdAsync(int goodsId);

    /// <summary>
    /// 添加一张商品图片
    /// </summary>
    /// <param name="image">图片实体（必须设置 GoodsId、ImageUrl、SortOrder）</param>
    /// <returns>无返回值，插入失败直接抛出异常</returns>
    /// <remarks>
    /// - image_id 使用自增列，无需手动赋值
    /// - created_at 自动填充 SYSDATE
    /// - 若 GoodsId 无效，会触发外键约束错误（业务层应提前校验商品存在）
    /// </remarks>
    Task AddImageAsync(GoodsImage image);

    /// <summary>
    /// 删除商品的所有图片（用于编辑时批量替换）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <remarks>
    /// 执行 DELETE FROM goods_image WHERE goods_id = :goodsId
    /// 若商品无图片，执行成功但影响行数为 0。
    /// 通常配合 <see cref="AddImageAsync"/> 批量更新使用。
    /// </remarks>
    Task DeleteImagesByGoodsIdAsync(int goodsId);

    /// <summary>
    /// 根据图片 ID 获取单张图片信息
    /// </summary>
    /// <param name="imageId">图片 ID（主键）</param>
    /// <returns>图片实体，不存在则返回 null</returns>
    /// <remarks>
    /// 用于删除单张图片前校验图片是否存在。
    /// </remarks>
    Task<GoodsImage?> GetImageByIdAsync(int imageId);

    /// <summary>
    /// 删除单张图片（通过图片 ID）
    /// </summary>
    /// <param name="imageId">图片 ID</param>
    /// <returns>true 表示删除成功（影响行数 > 0）</returns>
    /// <remarks>
    /// 业务层调用前应通过 <see cref="GetImageByIdAsync"/> 校验图片存在，
    /// 并确认操作者有权删除（商品卖家）。
    /// </remarks>
    Task<bool> DeleteImageAsync(int imageId);
}
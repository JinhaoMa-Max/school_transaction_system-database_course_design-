using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 商品模块数据访问层接口（Repository 契约）
/// 注意：所有方法参数中的时间字段在 Oracle 中对应 DATE 类型，金额对应 NUMBER(10,2)
/// </summary>
public interface IGoodsRepository
{
    // ==================== 商品核心 CRUD ====================

    /// <summary>
    /// 分页查询商品列表（供前台商品广场展示）
    /// </summary>
    /// <param name="page">页码（从 1 开始）</param>
    /// <param name="size">每页记录数（建议 10~20）</param>
    /// <param name="categoryId">商品分类 ID（精确匹配，可空）</param>
    /// <param name="keyword">搜索关键词（模糊匹配 title 和 description，可空）</param>
    /// <param name="sortBy">排序字段，可选值："price"、"createdAt"、"viewCount"，默认按 createdAt 降序</param>
    /// <param name="ascending">是否升序排列（false 表示降序）</param>
    /// <returns>元组：(商品列表 List&lt;GoodsDto&gt;, 总记录数 int)</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 默认过滤掉状态为 'offline'（已下架）和 'rejected'（已驳回）的商品，普通用户看不到这些。
    /// 2. 需要 LEFT JOIN user 表获取卖家昵称，LEFT JOIN category 获取分类名称。
    /// 3. 封面图取 goods_image 表中 sort_order 最小的一张（若无图片则 ImageUrl 为 null）。
    /// 4. Oracle 分页建议使用 OFFSET FETCH（12c+）或 ROWNUM 嵌套查询。
    /// 5. 总记录数需要单独 COUNT(*) 查询。
    /// </remarks>
    Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
        int page,
        int size,
        int? categoryId,
        string? keyword,
        string? sortBy,
        bool ascending);

    /// <summary>
    /// 根据主键获取商品详情
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>商品详情 DTO，若不存在则返回 null</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 必须关联查询卖家昵称、分类名称。
    /// 2. 封面图取第一张图（sort_order 最小）。
    /// 3. 不需要过滤状态，后台管理员可能需要查看任何状态的商品。
    /// </remarks>
    Task<GoodsDto?> GetByIdAsync(int goodsId);

    /// <summary>
    /// 新增商品（卖家发布）
    /// </summary>
    /// <param name="request">商品信息（分类、标题、描述、价格、成色）</param>
    /// <param name="sellerId">卖家用户 ID（从登录态获取）</param>
    /// <returns>新生成的自增商品 ID (goods_id)</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. goods_status 字段固定写入 'pending'（待审核）。
    /// 2. view_count 默认为 0。
    /// 3. created_at 使用 Oracle 的 SYSDATE。
    /// 4. 需使用 RETURNING goods_id INTO :变量 获取自增主键。
    /// </remarks>
    Task<int> CreateAsync(CreateGoodsRequest request, int sellerId);

    /// <summary>
    /// 更新商品信息（卖家编辑）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="request">待更新的字段（只传非空值）</param>
    /// <returns>是否更新成功（影响行数 > 0）</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 动态拼接 SET 子句，只更新 request 中非 null 的字段。
    /// 2. 必须同步更新 updated_at = SYSDATE。
    /// 3. 注意：不允许通过此接口直接修改 goods_status，状态变更由专用方法处理。
    /// </remarks>
    Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request);

    /// <summary>
    /// 物理删除商品（极少使用，通常为管理员强制删除）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>是否删除成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 注意外键约束：如果 goods_image、bargain_offer、trade_order 等表有级联删除设置，需确认。
    /// 2. 建议业务层先做软下架（改为 offline）再物理删除，此处只负责执行 DELETE。
    /// </remarks>
    Task<bool> DeleteAsync(int goodsId);

    /// <summary>
    /// 更新商品状态（审核、锁定、售出、下架等）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="newStatus">新状态，可选值：pending/approved/rejected/locked/sold/offline</param>
    /// <returns>是否更新成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 只更新 goods_status 和 updated_at。
    /// 2. 业务层会校验状态流转合法性（例如：locked 不能直接跳转 pending）。
    /// </remarks>
    Task<bool> UpdateStatusAsync(int goodsId, string newStatus);

    /// <summary>
    /// 增加商品浏览次数（原子操作 +1）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>是否更新成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. SQL: UPDATE goods SET view_count = view_count + 1 WHERE goods_id = :GoodsId
    /// 2. 无需事务，Oracle 行锁自动保证原子性。
    /// </remarks>
    Task<bool> IncrementViewCountAsync(int goodsId);

    // ==================== 商品图片管理 ====================

    /// <summary>
    /// 获取指定商品的所有图片列表
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>图片列表，按 sort_order 升序排列</returns>
    Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId);

    /// <summary>
    /// 为商品添加一张新图片
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="imageUrl">图片 URL（必填）</param>
    /// <param name="sortOrder">排序序号（数字越小越靠前）</param>
    /// <returns>新生成的图片自增 ID (image_id)</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 使用 RETURNING image_id INTO 获取主键。
    /// 2. created_at 使用 SYSDATE。
    /// </remarks>
    Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder);

    /// <summary>
    /// 根据图片主键删除图片
    /// </summary>
    /// <param name="imageId">图片 ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteImageAsync(int imageId);

    // ==================== 额外业务辅助查询（如需要） ====================

    /// <summary>
    /// 检查商品是否存在且属于某个卖家（用于权限校验）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <param name="sellerId">卖家 ID</param>
    /// <returns>若存在且匹配则返回 true</returns>
    /// <remarks>SQL: SELECT COUNT(1) FROM goods WHERE goods_id=:goodsId AND seller_id=:sellerId</remarks>
    Task<bool> IsOwnedByUserAsync(int goodsId, int sellerId);

    /// <summary>
    /// 检查商品当前是否处于可购买状态（未锁定、未售出、未下架）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>状态为 'approved' 返回 true</returns>
    /// <remarks>下单前调用，用于 F16 锁定前置校验</remarks>
    Task<bool> IsAvailableForPurchaseAsync(int goodsId);

    /// <summary>
    /// 获取商品的当前状态（用于业务层状态机判断）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>状态字符串，如 'approved'，若不存在返回 null</returns>
    Task<string?> GetStatusAsync(int goodsId);

    /// <summary>
    /// 锁定商品（下单专用，原子操作防止超卖）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>锁定成功返回 true，失败（已被他人锁定或状态不可用）返回 false</returns>
    /// <remarks>
    /// 数据库实现注意事项（关键业务逻辑）：
    /// 1. 必须使用条件更新：UPDATE goods SET goods_status = 'locked' 
    ///    WHERE goods_id = :goodsId AND goods_status = 'approved'
    /// 2. 检查影响行数：1 表示成功，0 表示商品已被他人锁定或状态异常。
    /// 3. 该操作需要事务配合（与创建订单在同一事务中）。
    /// </remarks>
    Task<bool> LockForPurchaseAsync(int goodsId);

    /// <summary>
    /// 释放商品锁定（取消订单或支付超时）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>是否释放成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 只有当状态为 'locked' 时才能恢复为 'approved'。
    /// 2. 同样使用条件更新避免误操作。
    /// </remarks>
    Task<bool> UnlockGoodsAsync(int goodsId);

    /// <summary>
    /// 标记商品为已售出（交易最终确认完成时触发）
    /// </summary>
    /// <param name="goodsId">商品 ID</param>
    /// <returns>是否更新成功</returns>
    /// <remarks>
    /// 数据库实现注意事项：
    /// 1. 状态变更为 'sold'。
    /// 2. 只有状态为 'locked' 或 'approved' 时可转为 'sold'（视业务流程而定）。
    /// </remarks>
    Task<bool> MarkAsSoldAsync(int goodsId);
}
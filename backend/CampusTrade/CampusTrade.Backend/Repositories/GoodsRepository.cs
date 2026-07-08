using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using System.Data;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 商品数据访问层 — 唯一可写 SQL 的层
/// 查询优先使用数据库视图 (v_goods_list, v_goods_detail)
/// 原子操作使用函数 (fn_can_purchase, fn_increment_view)
/// </summary>
public class GoodsRepository : IGoodsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GoodsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== 商品核心 CRUD ====================

    /// <summary>
    /// 分页查询商品列表
    /// 数据来源: v_goods_list 视图（自动 JOIN 卖家名、分类名、首图）
    /// </summary>
    public async Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
        int page, int size, int? categoryId, string? keyword, string? sortBy, bool ascending)
    {
        using var connection = _connectionFactory.CreateConnection();

        // 构建动态 WHERE 条件
        var where = new List<string>();
        var parameters = new DynamicParameters();

        if (categoryId.HasValue)
        {
            where.Add("category_id = :CategoryId");
            parameters.Add(":CategoryId", categoryId.Value);
        }
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            where.Add("title LIKE :Keyword");
            parameters.Add(":Keyword", $"%{keyword}%");
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        // 排序字段映射（防 SQL 注入：只用白名单，不拼接用户输入）
        var orderColumn = sortBy switch
        {
            "price" => "price",
            "viewCount" => "view_count",
            _ => "created_at"
        };
        var direction = ascending ? "ASC" : "DESC";

        // 查总数
        var countSql = $"SELECT COUNT(*) FROM v_goods_list {whereClause}";
        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // 查分页数据（Oracle OFFSET FETCH 不支持绑定变量，用字符串插值——值来自白名单，安全）
        var offset = (page - 1) * size;
        var listSql = $"""
            SELECT goods_id       AS GoodsId,
                   seller_id      AS SellerId,
                   seller_name    AS SellerNickname,
                   category_id    AS CategoryId,
                   category_name  AS CategoryName,
                   title          AS Title,
                   price          AS Price,
                   goods_condition AS Condition,
                   goods_status   AS Status,
                   view_count     AS ViewCount,
                   created_at     AS CreatedAt,
                   cover_image    AS ImageUrl
            FROM v_goods_list
            {whereClause}
            ORDER BY {orderColumn} {direction}
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;

        var items = await connection.QueryAsync<GoodsDto>(listSql, parameters);

        return (items.ToList(), total);
    }

    /// <summary>
    /// 商品详情 — 查 v_goods_detail 视图（含描述、卖家信用、收藏数）
    /// </summary>
    public async Task<GoodsDto?> GetByIdAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT goods_id       AS GoodsId,
                   seller_id      AS SellerId,
                   seller_name    AS SellerNickname,
                   category_id    AS CategoryId,
                   category_name  AS CategoryName,
                   title          AS Title,
                   description    AS Description,
                   price          AS Price,
                   goods_condition AS Condition,
                   goods_status   AS Status,
                   view_count     AS ViewCount,
                   created_at     AS CreatedAt,
                   cover_image    AS ImageUrl
            FROM v_goods_detail
            WHERE goods_id = :GoodsId
            """;

        return await connection.QueryFirstOrDefaultAsync<GoodsDto>(sql, new { GoodsId = goodsId });
    }

    /// <summary>
    /// 发布商品 — 直接 INSERT，状态自动设为 'pending'
    /// </summary>
    public async Task<int> CreateAsync(CreateGoodsRequest request, int sellerId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
            INSERT INTO goods (seller_id, category_id, title, description, price, goods_condition, goods_status, view_count, created_at)
            VALUES (:SellerId, :CategoryId, :Title, :Description, :Price, :Condition, 'pending', 0, SYSDATE)
            RETURNING goods_id INTO :NewId
            """;

        var parameters = new DynamicParameters();
        parameters.Add(":SellerId", sellerId);
        parameters.Add(":CategoryId", request.CategoryId);
        parameters.Add(":Title", request.Title);
        parameters.Add(":Description", request.Description ?? (object)DBNull.Value);
        parameters.Add(":Price", request.Price);
        parameters.Add(":Condition", request.Condition);
        parameters.Add(":NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":NewId");
    }

    /// <summary>
    /// 编辑商品 — 只更新传入的非空字段
    /// </summary>
    public async Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        // 动态拼接 SET 子句
        var sets = new List<string> { "updated_at = SYSDATE" };
        var parameters = new DynamicParameters();
        parameters.Add(":GoodsId", goodsId);

        if (request.CategoryId.HasValue) { sets.Add("category_id = :CategoryId"); parameters.Add(":CategoryId", request.CategoryId.Value); }
        if (request.Title != null) { sets.Add("title = :Title"); parameters.Add(":Title", request.Title); }
        if (request.Description != null) { sets.Add("description = :Description"); parameters.Add(":Description", request.Description); }
        if (request.Price.HasValue) { sets.Add("price = :Price"); parameters.Add(":Price", request.Price.Value); }
        if (request.Condition != null) { sets.Add("goods_condition = :Condition"); parameters.Add(":Condition", request.Condition); }

        if (sets.Count == 1) return true; // 没东西更新

        var sql = $"UPDATE goods SET {string.Join(", ", sets)} WHERE goods_id = :GoodsId";
        var affected = await connection.ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    /// <summary>
    /// 物理删除商品（管理员操作，极少使用）
    /// </summary>
    public async Task<bool> DeleteAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM goods WHERE goods_id = :GoodsId";
        var affected = await connection.ExecuteAsync(sql, new { GoodsId = goodsId });
        return affected > 0;
    }

    /// <summary>
    /// 更新商品状态 — 审核/锁定/售出/下架
    /// Service 层负责校验状态流转合法性
    /// </summary>
    public async Task<bool> UpdateStatusAsync(int goodsId, string newStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE goods SET goods_status = :Status, updated_at = SYSDATE
            WHERE goods_id = :GoodsId
            """;
        var affected = await connection.ExecuteAsync(sql, new { GoodsId = goodsId, Status = newStatus });
        return affected > 0;
    }

    /// <summary>
    /// 浏览量 +1 — 调用数据库函数 fn_increment_view
    /// </summary>
    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT fn_increment_view(:GoodsId) FROM DUAL";
        await connection.ExecuteAsync(sql, new { GoodsId = goodsId });
        return true; // 函数内部 UPDATE，不会失败
    }

    // ==================== 商品图片管理 ====================

    /// <summary>
    /// 商品图片列表 — 按 sort_order 升序
    /// </summary>
    public async Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT image_id AS ImageId, goods_id AS GoodsId,
                   image_url AS ImageUrl, sort_order AS SortOrder
            FROM goods_image
            WHERE goods_id = :GoodsId
            ORDER BY sort_order
            """;
        return await connection.QueryAsync<GoodsImageDto>(sql, new { GoodsId = goodsId });
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    public async Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO goods_image (goods_id, image_url, sort_order, created_at)
            VALUES (:GoodsId, :ImageUrl, :SortOrder, SYSDATE)
            RETURNING image_id INTO :NewId
            """;
        var parameters = new DynamicParameters();
        parameters.Add(":GoodsId", goodsId);
        parameters.Add(":ImageUrl", imageUrl);
        parameters.Add(":SortOrder", sortOrder);
        parameters.Add(":NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        await connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":NewId");
    }

    /// <summary>
    /// 删除图片
    /// </summary>
    public async Task<bool> DeleteImageAsync(int imageId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM goods_image WHERE image_id = :ImageId";
        var affected = await connection.ExecuteAsync(sql, new { ImageId = imageId });
        return affected > 0;
    }

    // ==================== 辅助业务方法 ====================

    /// <summary>
    /// 校验商品归属 — 卖家只能操作自己的商品
    /// </summary>
    public async Task<bool> IsOwnedByUserAsync(int goodsId, int sellerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM goods WHERE goods_id = :GoodsId AND seller_id = :SellerId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { GoodsId = goodsId, SellerId = sellerId });
        return count > 0;
    }

    /// <summary>
    /// 检查是否可购买 — 调用数据库函数 fn_can_purchase
    /// 校验：商品 approved + 非自买 + 用户未封禁 + 信用分≥0
    /// </summary>
    public async Task<bool> IsAvailableForPurchaseAsync(int goodsId)
    {
        // fn_can_purchase 需要 buyer_id，这里只检查商品状态
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM goods WHERE goods_id = :GoodsId AND goods_status = 'approved'";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { GoodsId = goodsId });
        return count > 0;
    }

    /// <summary>
    /// 查询商品当前状态
    /// </summary>
    public async Task<string?> GetStatusAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT goods_status FROM goods WHERE goods_id = :GoodsId";
        return await connection.QueryFirstOrDefaultAsync<string>(sql, new { GoodsId = goodsId });
    }

    /// <summary>
    /// 原子锁定商品 — F16 核心实现
    /// 条件更新：只有 'approved' 状态才能变为 'locked'，防止超卖
    /// SQL: UPDATE goods SET goods_status='locked' WHERE goods_id=:id AND goods_status='approved'
    /// 影响行数 1=成功, 0=已被他人锁定
    /// </summary>
    public async Task<bool> LockForPurchaseAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE goods SET goods_status = 'locked', updated_at = SYSDATE
            WHERE goods_id = :GoodsId AND goods_status = 'approved'
            """;
        var affected = await connection.ExecuteAsync(sql, new { GoodsId = goodsId });
        return affected > 0;
    }

    /// <summary>
    /// 释放商品锁定 — 取消订单时恢复
    /// 条件更新：只有 'locked' 状态才能恢复为 'approved'
    /// </summary>
    public async Task<bool> UnlockGoodsAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE goods SET goods_status = 'approved', updated_at = SYSDATE
            WHERE goods_id = :GoodsId AND goods_status = 'locked'
            """;
        var affected = await connection.ExecuteAsync(sql, new { GoodsId = goodsId });
        return affected > 0;
    }

    /// <summary>
    /// 标记售出 — 交易完成时调用
    /// 状态变为 'sold'
    /// </summary>
    public async Task<bool> MarkAsSoldAsync(int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE goods SET goods_status = 'sold', updated_at = SYSDATE
            WHERE goods_id = :GoodsId AND goods_status IN ('locked', 'approved')
            """;
        var affected = await connection.ExecuteAsync(sql, new { GoodsId = goodsId });
        return affected > 0;
    }
}

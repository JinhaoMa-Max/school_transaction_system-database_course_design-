using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using System.Data;

namespace CampusTrade.Backend.Repositories;

public class GoodsRepository : IGoodsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GoodsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== 商品核心 CRUD ====================

    public async Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
    int page, int size, int? categoryId, string? keyword, string? sortBy, bool ascending)
{
    using var conn = _connectionFactory.CreateConnection();

    // 总记录数（无绑定变量）
    const string totalSql = "SELECT COUNT(*) FROM goods WHERE goods_status IN ('approved', 'sold')";
    var total = await conn.ExecuteScalarAsync<int>(totalSql);

    if (total == 0)
        return (new List<GoodsDto>(), 0);

    // 计算分页行号（直接拼接，避免绑定变量）
    int startRow = (page - 1) * size + 1;
    int endRow = page * size;

    var dataSql = $@"
        SELECT * FROM (
            SELECT
                g.goods_id AS GoodsId,
                g.seller_id AS SellerId,
                u.nickname AS SellerNickname,
                g.category_id AS CategoryId,
                c.category_name AS CategoryName,
                g.title AS Title,
                g.description AS Description,
                g.price AS Price,
                g.goods_condition AS Condition,
                g.goods_status AS Status,
                g.view_count AS ViewCount,
                g.created_at AS CreatedAt,
                (SELECT image_url FROM goods_image WHERE goods_id = g.goods_id ORDER BY sort_order ASC FETCH FIRST 1 ROW ONLY) AS ImageUrl,
                ROWNUM AS rn
            FROM goods g
            JOIN app_user u ON g.seller_id = u.user_id
            JOIN category c ON g.category_id = c.category_id
            WHERE g.goods_status IN ('approved', 'sold')
            ORDER BY g.created_at DESC
        )
        WHERE rn BETWEEN {startRow} AND {endRow}
    ";

    // ★★★ 注意：这里没有传入任何参数，完全避免了绑定变量问题
    var items = await conn.QueryAsync<GoodsDto>(dataSql);
    return (items.ToList(), total);
}

    public async Task<GoodsDto?> GetByIdAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                goods_id AS GoodsId,
                seller_id AS SellerId,
                seller_name AS SellerNickname,
                category_id AS CategoryId,
                category_name AS CategoryName,
                title AS Title,
                description AS Description,
                price AS Price,
                goods_condition AS Condition,
                goods_status AS Status,
                view_count AS ViewCount,
                created_at AS CreatedAt,
                (
                    SELECT image_url
                    FROM goods_image
                    WHERE goods_id = v.goods_id
                    ORDER BY sort_order ASC
                    FETCH FIRST 1 ROW ONLY
                ) AS ImageUrl
            FROM v_goods_detail v
            WHERE goods_id = :GoodsId
        ";
        return await conn.QueryFirstOrDefaultAsync<GoodsDto>(sql, new { GoodsId = goodsId });
    }

    public async Task<int> CreateAsync(CreateGoodsRequest request, int sellerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
        INSERT INTO goods
            (seller_id, category_id, title, description, price, goods_condition, goods_status, view_count, created_at)
        VALUES
            (:SellerId, :CategoryId, :Title, :Description, :Price, :Condition, 'pending', 0, SYSTIMESTAMP)
        RETURNING goods_id INTO :GoodsId ";
    
        var parameters = new DynamicParameters(request);
        parameters.Add("SellerId", sellerId);
        // 声明一个输出参数来接收 RETURNING 子句返回的新ID
        parameters.Add("GoodsId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync(sql, parameters);
        // 从输出参数中获取新生成的ID
        return parameters.Get<int>("GoodsId");
    }

    public async Task<bool> UpdateAsync(int goodsId, UpdateGoodsRequest request)
    {
        using var conn = _connectionFactory.CreateConnection();
        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("GoodsId", goodsId);

        if (request.CategoryId.HasValue)
        {
            updates.Add("category_id = :CategoryId");
            parameters.Add("CategoryId", request.CategoryId.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            updates.Add("title = :Title");
            parameters.Add("Title", request.Title);
        }
        if (request.Description != null)
        {
            updates.Add("description = :Description");
            parameters.Add("Description", request.Description);
        }
        if (request.Price.HasValue)
        {
            updates.Add("price = :Price");
            parameters.Add("Price", request.Price.Value);
        }
        if (!string.IsNullOrWhiteSpace(request.Condition))
        {
            updates.Add("goods_condition = :Condition");
            parameters.Add("Condition", request.Condition);
        }

        if (updates.Count == 0) return true;

        var sql = $@"
            UPDATE goods
            SET {string.Join(", ", updates)}, updated_at = SYSTIMESTAMP
            WHERE goods_id = :GoodsId
        ";
        var rows = await conn.ExecuteAsync(sql, parameters);
        return rows > 0;
    }

    // ==================== 【重要修复】处理外键级联删除 ====================
    public async Task<bool> DeleteAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var transaction = conn.BeginTransaction();
        try
        {
            // 1. 先删除关联的商品图片（解决外键约束）
            await conn.ExecuteAsync(
                "DELETE FROM goods_image WHERE goods_id = :GoodsId",
                new { GoodsId = goodsId },
                transaction
            );

            // 2. 再删除商品主记录
            var rows = await conn.ExecuteAsync(
                "DELETE FROM goods WHERE goods_id = :GoodsId",
                new { GoodsId = goodsId },
                transaction
            );

            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int goodsId, string newStatus)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET goods_status = :NewStatus, updated_at = SYSTIMESTAMP
            WHERE goods_id = :GoodsId
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId, NewStatus = newStatus });
        return rows > 0;
    }

    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT fn_increment_view(:GoodsId) FROM dual";
        var newCount = await conn.ExecuteScalarAsync<int?>(sql, new { GoodsId = goodsId });
        return newCount.HasValue && newCount >= 0;
    }

    // ==================== 商品图片管理 ====================

    public async Task<IEnumerable<GoodsImageDto>> GetImagesAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                image_id AS ImageId,
                goods_id AS GoodsId,
                image_url AS ImageUrl,
                sort_order AS SortOrder,
                created_at AS CreatedAt
            FROM goods_image
            WHERE goods_id = :GoodsId
            ORDER BY sort_order ASC
        ";
        return await conn.QueryAsync<GoodsImageDto>(sql, new { GoodsId = goodsId });
    }

    public async Task<int> AddImageAsync(int goodsId, string imageUrl, int sortOrder)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO goods_image (goods_id, image_url, sort_order, created_at)
            VALUES (:GoodsId, :ImageUrl, :SortOrder, SYSTIMESTAMP)
            RETURNING image_id INTO :ImageId
        ";
        var parameters = new DynamicParameters();
        parameters.Add("GoodsId", goodsId);
        parameters.Add("ImageUrl", imageUrl);
        parameters.Add("SortOrder", sortOrder);
        parameters.Add("ImageId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await conn.ExecuteAsync(sql, parameters);
        return parameters.Get<int>("ImageId");
    }

    public async Task<bool> DeleteImageAsync(int imageId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM goods_image WHERE image_id = :ImageId";
        var rows = await conn.ExecuteAsync(sql, new { ImageId = imageId });
        return rows > 0;
    }

    // ==================== 辅助业务方法 ====================

    public async Task<bool> IsOwnedByUserAsync(int goodsId, int sellerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM goods WHERE goods_id = :GoodsId AND seller_id = :SellerId";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { GoodsId = goodsId, SellerId = sellerId });
        return count > 0;
    }

    public async Task<bool> IsAvailableForPurchaseAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM goods WHERE goods_id = :GoodsId AND goods_status = 'approved'";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { GoodsId = goodsId });
        return count > 0;
    }

    public async Task<string?> GetStatusAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "SELECT goods_status FROM goods WHERE goods_id = :GoodsId";
        return await conn.ExecuteScalarAsync<string?>(sql, new { GoodsId = goodsId });
    }

    public async Task<bool> LockForPurchaseAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET goods_status = 'locked', updated_at = SYSTIMESTAMP
            WHERE goods_id = :GoodsId AND goods_status = 'approved'
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId });
        return rows > 0;
    }

    public async Task<bool> UnlockGoodsAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET goods_status = 'approved', updated_at = SYSTIMESTAMP
            WHERE goods_id = :GoodsId AND goods_status = 'locked'
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId });
        return rows > 0;
    }

    public async Task<bool> MarkAsSoldAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET goods_status = 'sold', updated_at = SYSTIMESTAMP
            WHERE goods_id = :GoodsId AND goods_status IN ('locked', 'approved')
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId });
        return rows > 0;
    }
}
using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public class GoodsRepository : IGoodsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GoodsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<GoodsDto> Items, int Total)> GetPagedAsync(
        int page, int size,
        int? categoryId, string? keyword,
        string? sortBy, bool ascending)
    {
        using var conn = _connectionFactory.CreateConnection();

        // 构建动态 WHERE 条件
        var conditions = new List<string> { "g.goods_status NOT IN ('offline', 'rejected')" }; // 默认不展示下架/驳回
        var parameters = new DynamicParameters();

        if (categoryId.HasValue)
        {
            conditions.Add("g.category_id = :CategoryId");
            parameters.Add("CategoryId", categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            conditions.Add("(g.title LIKE :Keyword OR g.description LIKE :Keyword)");
            parameters.Add("Keyword", $"%{keyword}%");
        }

        var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

        // 排序字段映射
        var sortColumn = sortBy?.ToLower() switch
        {
            "price" => "g.price",
            "createdat" => "g.created_at",
            "viewcount" => "g.view_count",
            _ => "g.created_at" // 默认按发布时间
        };
        var sortOrder = ascending ? "ASC" : "DESC";

        // 查询总数
        var countSql = $@"
            SELECT COUNT(*)
            FROM goods g
            {whereClause}
        ";
        var total = await conn.ExecuteScalarAsync<int>(countSql, parameters);

        // 分页查询（Oracle 12c+ 使用 OFFSET FETCH）
        var offset = (page - 1) * size;
        var dataSql = $@"
            SELECT
                g.goods_id AS GoodsId,
                g.seller_id AS SellerId,
                u.nickname AS SellerNickname,
                g.category_id AS CategoryId,
                c.category_name AS CategoryName,
                g.title AS Title,
                g.description AS Description,
                g.price AS Price,
                g.condition AS Condition,
                g.goods_status AS Status,
                g.view_count AS ViewCount,
                g.created_at AS CreatedAt,
                (
                    SELECT image_url
                    FROM goods_image
                    WHERE goods_id = g.goods_id
                    ORDER BY sort_order ASC
                    FETCH FIRST 1 ROW ONLY
                ) AS ImageUrl
            FROM goods g
            LEFT JOIN ""user"" u ON g.seller_id = u.user_id
            LEFT JOIN category c ON g.category_id = c.category_id
            {whereClause}
            ORDER BY {sortColumn} {sortOrder}
            OFFSET :Offset ROWS FETCH NEXT :Size ROWS ONLY
        ";
        parameters.Add("Offset", offset);
        parameters.Add("Size", size);

        var items = await conn.QueryAsync<GoodsDto>(dataSql, parameters);

        return (items.ToList(), total);
    }

    public async Task<GoodsDto?> GetByIdAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT
                g.goods_id AS GoodsId,
                g.seller_id AS SellerId,
                u.nickname AS SellerNickname,
                g.category_id AS CategoryId,
                c.category_name AS CategoryName,
                g.title AS Title,
                g.description AS Description,
                g.price AS Price,
                g.condition AS Condition,
                g.goods_status AS Status,
                g.view_count AS ViewCount,
                g.created_at AS CreatedAt,
                (
                    SELECT image_url
                    FROM goods_image
                    WHERE goods_id = g.goods_id
                    ORDER BY sort_order ASC
                    FETCH FIRST 1 ROW ONLY
                ) AS ImageUrl
            FROM goods g
            LEFT JOIN ""user"" u ON g.seller_id = u.user_id
            LEFT JOIN category c ON g.category_id = c.category_id
            WHERE g.goods_id = :GoodsId
        ";
        return await conn.QueryFirstOrDefaultAsync<GoodsDto>(sql, new { GoodsId = goodsId });
    }

    public async Task<int> CreateAsync(CreateGoodsRequest request, int sellerId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO goods
                (seller_id, category_id, title, description, price, condition, goods_status, view_count, created_at)
            VALUES
                (:SellerId, :CategoryId, :Title, :Description, :Price, :Condition, 'pending', 0, SYSDATE)
            RETURNING goods_id INTO :GoodsId
        ";
        var parameters = new DynamicParameters(request);
        parameters.Add("SellerId", sellerId);
        parameters.Add("GoodsId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await conn.ExecuteAsync(sql, parameters);
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
        if (request.Description != null) // 允许置空
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
            updates.Add("condition = :Condition");
            parameters.Add("Condition", request.Condition);
        }

        if (updates.Count == 0) return true; // 无更新

        var sql = $@"
            UPDATE goods
            SET {string.Join(", ", updates)}, updated_at = SYSDATE
            WHERE goods_id = :GoodsId
        ";
        var rows = await conn.ExecuteAsync(sql, parameters);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM goods WHERE goods_id = :GoodsId";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId });
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(int goodsId, string newStatus)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET goods_status = :NewStatus, updated_at = SYSDATE
            WHERE goods_id = :GoodsId
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId, NewStatus = newStatus });
        return rows > 0;
    }

    public async Task<bool> IncrementViewCountAsync(int goodsId)
    {
        using var conn = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE goods
            SET view_count = view_count + 1
            WHERE goods_id = :GoodsId
        ";
        var rows = await conn.ExecuteAsync(sql, new { GoodsId = goodsId });
        return rows > 0;
    }

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
            VALUES (:GoodsId, :ImageUrl, :SortOrder, SYSDATE)
            RETURNING image_id INTO :ImageId
        ";
        var parameters = new DynamicParameters();
        parameters.Add("GoodsId", goodsId);
        parameters.Add("ImageUrl", imageUrl);
        parameters.Add("SortOrder", sortOrder);
        parameters.Add("ImageId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

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
}
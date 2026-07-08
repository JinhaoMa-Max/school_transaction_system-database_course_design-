using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 收藏数据访问层（F10）
/// 数据库: favorite 表 / fn_favorite_goods_ids / v_goods_list
/// </summary>
public class FavoriteRepository : IFavoriteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public FavoriteRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    /// <summary>收藏列表 — JOIN v_goods_list 获取商品名/价格/首图</summary>
    public async Task<(List<FavoriteDto> Items, int Total)> GetPagedAsync(int userId, int page, int size)
    {
        using var connection = _connectionFactory.CreateConnection();
        var off = (page - 1) * size;
        var countSql = "SELECT COUNT(1) FROM favorite WHERE user_id = :UserId";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId });
        var sql = $"""
            SELECT f.favorite_id AS FavoriteId, f.user_id AS UserId, f.goods_id AS GoodsId,
                   g.title AS GoodsTitle, g.price AS GoodsPrice, g.goods_status AS GoodsStatus,
                   (SELECT MIN(gi.image_url) FROM goods_image gi WHERE gi.goods_id = f.goods_id
                    AND gi.sort_order = (SELECT MIN(sort_order) FROM goods_image WHERE goods_id = f.goods_id)) AS CoverImage,
                   f.created_at AS CreateTime
            FROM favorite f JOIN goods g ON f.goods_id = g.goods_id
            WHERE f.user_id = :UserId
            ORDER BY f.created_at DESC
            OFFSET {off} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<FavoriteDto>(sql, new { UserId = userId });
        return (items.ToList(), total);
    }

    /// <summary>添加收藏 — 唯一约束防重复（user_id+goods_id）</summary>
    public async Task<int> AddAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO favorite (user_id, goods_id, created_at) VALUES (:UserId, :GoodsId, SYSDATE)
            RETURNING favorite_id INTO :NewId
            """;
        var p = new DynamicParameters();
        p.Add(":UserId", userId); p.Add(":GoodsId", goodsId);
        p.Add(":NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        await connection.ExecuteAsync(sql, p);
        return p.Get<int>(":NewId");
    }

    public async Task<bool> RemoveAsync(int favoriteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("DELETE FROM favorite WHERE favorite_id=:id", new { id = favoriteId }) > 0;
    }

    public async Task<bool> RemoveByGoodsAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("DELETE FROM favorite WHERE user_id=:u AND goods_id=:g", new { u = userId, g = goodsId }) > 0;
    }

    public async Task<bool> IsFavoritedAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM favorite WHERE user_id=:u AND goods_id=:g", new { u = userId, g = goodsId }) > 0;
    }

    /// <summary>收藏商品ID列表 — fn_favorite_goods_ids 返回逗号分隔字符串 "1,3,7"</summary>
    public async Task<string> GetFavoriteGoodsIdsAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<string>("SELECT fn_favorite_goods_ids(:u) FROM DUAL", new { u = userId }) ?? string.Empty;
    }
}

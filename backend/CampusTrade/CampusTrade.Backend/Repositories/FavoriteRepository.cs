using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public class FavoriteRepository : IFavoriteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private const string FavoriteSelectColumns = """
        f.favorite_id AS FavoriteId,
        f.user_id AS UserId,
        f.goods_id AS GoodsId,
        g.title AS GoodsTitle,
        g.price AS GoodsPrice,
        g.goods_status AS GoodsStatus,
        (SELECT MIN(gi.image_url)
         FROM goods_image gi
         WHERE gi.goods_id = f.goods_id
           AND gi.sort_order = (SELECT MIN(sort_order) FROM goods_image WHERE goods_id = f.goods_id)) AS CoverImage,
        f.created_at AS CreateTime,
        f.created_at AS FavoriteTime
        """;

    public FavoriteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<FavoriteDto> Items, int Total)> GetPagedAsync(int userId, int page, int size)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (page - 1) * size;

        const string countSql = "SELECT COUNT(1) FROM favorite WHERE user_id = :UserId";
        var total = await connection.ExecuteScalarAsync<int>(countSql, new { UserId = userId });

        var sql = $"""
            SELECT {FavoriteSelectColumns}
            FROM favorite f
            JOIN goods g ON f.goods_id = g.goods_id
            WHERE f.user_id = :UserId
            ORDER BY f.created_at DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<FavoriteDto>(sql, new { UserId = userId });
        return (items.ToList(), total);
    }

    public async Task<FavoriteDto?> GetByIdAsync(int userId, int favoriteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT {FavoriteSelectColumns}
            FROM favorite f
            JOIN goods g ON f.goods_id = g.goods_id
            WHERE f.user_id = :UserId AND f.favorite_id = :FavoriteId
            """;
        return await connection.QuerySingleOrDefaultAsync<FavoriteDto>(sql, new { UserId = userId, FavoriteId = favoriteId });
    }

    public async Task<FavoriteDto?> GetByUserAndGoodsAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT {FavoriteSelectColumns}
            FROM favorite f
            JOIN goods g ON f.goods_id = g.goods_id
            WHERE f.user_id = :UserId AND f.goods_id = :GoodsId
            """;
        return await connection.QuerySingleOrDefaultAsync<FavoriteDto>(sql, new { UserId = userId, GoodsId = goodsId });
    }

    public async Task<int> AddAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string insertSql = """
            INSERT INTO favorite (user_id, goods_id, created_at)
            VALUES (:UserId, :GoodsId, SYSTIMESTAMP)
            """;
        await connection.ExecuteAsync(insertSql, new { UserId = userId, GoodsId = goodsId });

        const string selectSql = """
            SELECT favorite_id
            FROM favorite
            WHERE user_id = :UserId AND goods_id = :GoodsId
            """;
        return await connection.ExecuteScalarAsync<int>(selectSql, new { UserId = userId, GoodsId = goodsId });
    }

    public async Task<bool> RemoveAsync(int userId, int favoriteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM favorite WHERE user_id = :UserId AND favorite_id = :FavoriteId";
        return await connection.ExecuteAsync(sql, new { UserId = userId, FavoriteId = favoriteId }) > 0;
    }

    public async Task<bool> RemoveByGoodsAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM favorite WHERE user_id = :UserId AND goods_id = :GoodsId";
        return await connection.ExecuteAsync(sql, new { UserId = userId, GoodsId = goodsId }) > 0;
    }

    public async Task<bool> IsFavoritedAsync(int userId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM favorite WHERE user_id = :UserId AND goods_id = :GoodsId";
        return await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, GoodsId = goodsId }) > 0;
    }

    public async Task<string> GetFavoriteGoodsIdsAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<string>("SELECT fn_favorite_goods_ids(:UserId) FROM DUAL", new { UserId = userId }) ?? string.Empty;
    }
}

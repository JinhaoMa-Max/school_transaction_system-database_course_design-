using System.Data;
using CampusSecondHand.Api.Database;
using CampusSecondHand.Api.Models.Entities;
using CampusSecondHand.Api.Repositories.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace CampusSecondHand.Api.Repositories;

public class GoodsRepository : IGoodsRepository
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public GoodsRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Goods>> GetAllAsync(int page, int size)
    {
        var offset = (page - 1) * size;
        var sql = $"""
            SELECT goods_id, seller_id, seller_name, category_id, category_name,
                   title, price, goods_condition, goods_status, view_count,
                   cover_image, created_at
            FROM v_goods_list
            ORDER BY created_at DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;

        var goods = new List<Goods>();

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = new OracleCommand(sql, connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            goods.Add(MapGoods(reader));
        }

        return goods;
    }

    public async Task<int> GetCountAsync()
    {
        const string sql = "SELECT COUNT(*) FROM v_goods_list";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = new OracleCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<Goods?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT goods_id, seller_id, seller_name, category_id, category_name,
                   title, description, price, goods_condition, goods_status,
                   view_count, cover_image, created_at
            FROM v_goods_detail
            WHERE goods_id = :id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = new OracleCommand(sql, connection) { BindByName = true };
        command.Parameters.Add(new OracleParameter("id", OracleDbType.Int32) { Value = id });

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapGoods(reader) : null;
    }

    private static Goods MapGoods(IDataRecord record)
    {
        return new Goods
        {
            GoodsId = Convert.ToInt32(record["goods_id"]),
            SellerId = Convert.ToInt32(record["seller_id"]),
            CategoryId = Convert.ToInt32(record["category_id"]),
            Title = Convert.ToString(record["title"]) ?? string.Empty,
            Description = SafeGetString(record, "description"),
            Price = Convert.ToDecimal(record["price"]),
            GoodsCondition = Convert.ToString(record["goods_condition"]) ?? string.Empty,
            GoodsStatus = Convert.ToString(record["goods_status"]) ?? string.Empty,
            ViewCount = Convert.ToInt32(record["view_count"]),
            CreatedAt = Convert.ToDateTime(record["created_at"]),
            SellerName = SafeGetString(record, "seller_name") ?? string.Empty,
            CategoryName = SafeGetString(record, "category_name") ?? string.Empty,
            CoverImage = SafeGetString(record, "cover_image")
        };
    }

    private static string? SafeGetString(IDataRecord record, string name)
    {
        try
        {
            var val = record[name];
            return val == DBNull.Value ? null : Convert.ToString(val);
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<Goods?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT goods_id, seller_id, category_id, title, description, price, condition, goods_status, view_count, created_at, updated_at
            FROM goods
            WHERE goods_id = :id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var goods = MapGoods(reader);
            goods.Images = await GetImagesByGoodsIdAsync(goods.GoodsId);
            return goods;
        }
        return null;
    }

    public async Task<List<Goods>> SearchAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? condition, string? sortBy, bool ascending, int page, int pageSize)
    {
        var sqlBuilder = new StringBuilder("""
            SELECT goods_id, seller_id, category_id, title, description, price, condition, goods_status, view_count, created_at, updated_at
            FROM goods
            WHERE goods_status = 'approved'
            """);

        var parameters = new List<OracleParameter>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sqlBuilder.Append(" AND (LOWER(title) LIKE LOWER(:keyword) OR LOWER(description) LIKE LOWER(:keyword))");
            parameters.Add(CreateStringParameter("keyword", $"%{keyword}%", 400));
        }

        if (categoryId.HasValue)
        {
            sqlBuilder.Append(" AND category_id = :category_id");
            parameters.Add(CreateIntParameter("category_id", categoryId.Value));
        }

        if (minPrice.HasValue)
        {
            sqlBuilder.Append(" AND price >= :min_price");
            parameters.Add(CreateDecimalParameter("min_price", minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            sqlBuilder.Append(" AND price <= :max_price");
            parameters.Add(CreateDecimalParameter("max_price", maxPrice.Value));
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            sqlBuilder.Append(" AND condition = :condition");
            parameters.Add(CreateStringParameter("condition", condition, 20));
        }

        var sortColumn = sortBy?.ToLower() switch
        {
            "price" => "price",
            "view_count" => "view_count",
            _ => "created_at"
        };
        var sortDirection = ascending ? "ASC" : "DESC";
        sqlBuilder.Append($" ORDER BY {sortColumn} {sortDirection}");

        var offset = (page - 1) * pageSize;
        sqlBuilder.Append($" OFFSET :offset ROWS FETCH NEXT :page_size ROWS ONLY");
        parameters.Add(CreateIntParameter("offset", offset));
        parameters.Add(CreateIntParameter("page_size", pageSize));

        var goodsList = new List<Goods>();
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sqlBuilder.ToString());
        command.Parameters.AddRange(parameters.ToArray());

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            goodsList.Add(MapGoods(reader));
        }
        return goodsList;
    }

    public async Task<int> CountSearchAsync(string? keyword, int? categoryId, decimal? minPrice, decimal? maxPrice, string? condition)
    {
        var sqlBuilder = new StringBuilder("""
            SELECT COUNT(1)
            FROM goods
            WHERE goods_status = 'approved'
            """);

        var parameters = new List<OracleParameter>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sqlBuilder.Append(" AND (LOWER(title) LIKE LOWER(:keyword) OR LOWER(description) LIKE LOWER(:keyword))");
            parameters.Add(CreateStringParameter("keyword", $"%{keyword}%", 400));
        }

        if (categoryId.HasValue)
        {
            sqlBuilder.Append(" AND category_id = :category_id");
            parameters.Add(CreateIntParameter("category_id", categoryId.Value));
        }

        if (minPrice.HasValue)
        {
            sqlBuilder.Append(" AND price >= :min_price");
            parameters.Add(CreateDecimalParameter("min_price", minPrice.Value));
        }

        if (maxPrice.HasValue)
        {
            sqlBuilder.Append(" AND price <= :max_price");
            parameters.Add(CreateDecimalParameter("max_price", maxPrice.Value));
        }

        if (!string.IsNullOrWhiteSpace(condition))
        {
            sqlBuilder.Append(" AND condition = :condition");
            parameters.Add(CreateStringParameter("condition", condition, 20));
        }

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sqlBuilder.ToString());
        command.Parameters.AddRange(parameters.ToArray());

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<int> CreateAsync(Goods goods)
    {
        const string sql = """
            INSERT INTO goods (seller_id, category_id, title, description, price, condition, goods_status, view_count, created_at, updated_at)
            VALUES (:seller_id, :category_id, :title, :description, :price, :condition, :goods_status, :view_count, SYSDATE, SYSDATE)
            RETURNING goods_id INTO :new_id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        command.Parameters.Add(CreateIntParameter("seller_id", goods.SellerId));
        command.Parameters.Add(CreateIntParameter("category_id", goods.CategoryId));
        command.Parameters.Add(CreateStringParameter("title", goods.Title, 200));
        command.Parameters.Add(CreateStringParameter("description", goods.Description ?? string.Empty, 4000));
        command.Parameters.Add(CreateDecimalParameter("price", goods.Price));
        command.Parameters.Add(CreateStringParameter("condition", goods.Condition, 20));
        command.Parameters.Add(CreateStringParameter("goods_status", goods.GoodsStatus, 20));
        command.Parameters.Add(CreateIntParameter("view_count", 0));

        var newIdParam = new OracleParameter("new_id", OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(newIdParam);

        await command.ExecuteNonQueryAsync();
        return Convert.ToInt32(newIdParam.Value?.ToString());
    }

    public async Task<bool> UpdateAsync(Goods goods)
    {
        const string sql = """
            UPDATE goods
            SET category_id = :category_id,
                title = :title,
                description = :description,
                price = :price,
                condition = :condition,
                updated_at = SYSDATE
            WHERE goods_id = :goods_id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        command.Parameters.Add(CreateIntParameter("category_id", goods.CategoryId));
        command.Parameters.Add(CreateStringParameter("title", goods.Title, 200));
        command.Parameters.Add(CreateStringParameter("description", goods.Description ?? string.Empty, 4000));
        command.Parameters.Add(CreateDecimalParameter("price", goods.Price));
        command.Parameters.Add(CreateStringParameter("condition", goods.Condition, 20));
        command.Parameters.Add(CreateIntParameter("goods_id", goods.GoodsId));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM goods WHERE goods_id = :id";
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> UpdateStatusAsync(int goodsId, string status)
    {
        const string sql = """
            UPDATE goods
            SET goods_status = :status,
                updated_at = SYSDATE
            WHERE goods_id = :goods_id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateStringParameter("status", status, 20));
        command.Parameters.Add(CreateIntParameter("goods_id", goodsId));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<List<GoodsImage>> GetImagesByGoodsIdAsync(int goodsId)
    {
        const string sql = """
            SELECT image_id, goods_id, image_url, sort_order, created_at
            FROM goods_image
            WHERE goods_id = :goods_id
            ORDER BY sort_order
            """;

        var images = new List<GoodsImage>();
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("goods_id", goodsId));

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            images.Add(new GoodsImage
            {
                ImageId = Convert.ToInt32(reader["image_id"]),
                GoodsId = Convert.ToInt32(reader["goods_id"]),
                ImageUrl = Convert.ToString(reader["image_url"]) ?? string.Empty,
                SortOrder = Convert.ToInt32(reader["sort_order"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"])
            });
        }
        return images;
    }

    public async Task AddImageAsync(GoodsImage image)
    {
        const string sql = """
            INSERT INTO goods_image (goods_id, image_url, sort_order, created_at)
            VALUES (:goods_id, :image_url, :sort_order, SYSDATE)
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("goods_id", image.GoodsId));
        command.Parameters.Add(CreateStringParameter("image_url", image.ImageUrl, 255));
        command.Parameters.Add(CreateIntParameter("sort_order", image.SortOrder));

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteImagesByGoodsIdAsync(int goodsId)
    {
        const string sql = "DELETE FROM goods_image WHERE goods_id = :goods_id";
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("goods_id", goodsId));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM goods WHERE goods_id = :id";
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // ---- Helper Methods ----
    private static OracleCommand CreateCommand(OracleConnection connection, string sql)
    {
        return new OracleCommand(sql, connection) { BindByName = true };
    }

    private static OracleParameter CreateIntParameter(string name, int value) =>
        new(name, OracleDbType.Int32) { Value = value };

    private static OracleParameter CreateDecimalParameter(string name, decimal value) =>
        new(name, OracleDbType.Decimal) { Value = value };

    private static OracleParameter CreateStringParameter(string name, string value, int size) =>
        new(name, OracleDbType.Varchar2, size) { Value = value };

    private static Goods MapGoods(IDataRecord record)
    {
        return new Goods
        {
            GoodsId = Convert.ToInt32(record["goods_id"]),
            SellerId = Convert.ToInt32(record["seller_id"]),
            CategoryId = Convert.ToInt32(record["category_id"]),
            Title = Convert.ToString(record["title"]) ?? string.Empty,
            Description = Convert.ToString(record["description"]),
            Price = Convert.ToDecimal(record["price"]),
            Condition = Convert.ToString(record["condition"]) ?? string.Empty,
            GoodsStatus = Convert.ToString(record["goods_status"]) ?? "pending",
            ViewCount = Convert.ToInt32(record["view_count"]),
            CreatedAt = Convert.ToDateTime(record["created_at"]),
            UpdatedAt = Convert.ToDateTime(record["updated_at"])
        };
    }
}
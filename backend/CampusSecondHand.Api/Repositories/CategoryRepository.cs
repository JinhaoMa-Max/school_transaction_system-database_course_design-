using System.Data;
using CampusSecondHand.Api.Database;
using CampusSecondHand.Api.Models.Entities;
using CampusSecondHand.Api.Repositories.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace CampusSecondHand.Api.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IOracleConnectionFactory _connectionFactory;

    public CategoryRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        const string sql = """
            SELECT category_id, category_name, parent_id, sort_order, created_at
            FROM category
            ORDER BY sort_order, category_id
            """;

        var categories = new List<Category>();

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT category_id, category_name, parent_id, sort_order, created_at
            FROM category
            WHERE category_id = :id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategory(reader) : null;
    }

    public async Task<int> CreateAsync(Category category)
    {
        const string sql = """
            INSERT INTO category (category_name, parent_id, sort_order)
            VALUES (:category_name, :parent_id, :sort_order)
            RETURNING category_id INTO :new_id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        command.Parameters.Add(CreateStringParameter("category_name", category.CategoryName, 100));
        command.Parameters.Add(CreateNullableIntParameter("parent_id", category.ParentId));
        command.Parameters.Add(CreateIntParameter("sort_order", category.SortOrder));

        var newIdParameter = new OracleParameter("new_id", OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };
        command.Parameters.Add(newIdParameter);

        await command.ExecuteNonQueryAsync();

        return Convert.ToInt32(newIdParameter.Value?.ToString());
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        const string sql = """
            UPDATE category
            SET category_name = :category_name,
                parent_id = :parent_id,
                sort_order = :sort_order
            WHERE category_id = :category_id
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);

        command.Parameters.Add(CreateStringParameter("category_name", category.CategoryName, 100));
        command.Parameters.Add(CreateNullableIntParameter("parent_id", category.ParentId));
        command.Parameters.Add(CreateIntParameter("sort_order", category.SortOrder));
        command.Parameters.Add(CreateIntParameter("category_id", category.CategoryId));

        var affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM category WHERE category_id = :id";

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));

        var affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM category WHERE category_id = :id";
        return await CountAsync(sql, id) > 0;
    }

    public async Task<bool> HasChildrenAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM category WHERE parent_id = :id";
        return await CountAsync(sql, id) > 0;
    }

    public async Task<bool> HasGoodsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM goods WHERE category_id = :id";
        return await CountAsync(sql, id) > 0;
    }

    private async Task<int> CountAsync(string sql, int id)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await using var command = CreateCommand(connection, sql);
        command.Parameters.Add(CreateIntParameter("id", id));

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static OracleCommand CreateCommand(OracleConnection connection, string sql)
    {
        return new OracleCommand(sql, connection)
        {
            BindByName = true
        };
    }

    private static OracleParameter CreateIntParameter(string name, int value)
    {
        return new OracleParameter(name, OracleDbType.Int32)
        {
            Value = value
        };
    }

    private static OracleParameter CreateNullableIntParameter(string name, int? value)
    {
        return new OracleParameter(name, OracleDbType.Int32)
        {
            Value = value.HasValue ? value.Value : DBNull.Value
        };
    }

    private static OracleParameter CreateStringParameter(string name, string value, int size)
    {
        return new OracleParameter(name, OracleDbType.Varchar2, size)
        {
            Value = value
        };
    }

    private static Category MapCategory(IDataRecord record)
    {
        return new Category
        {
            CategoryId = Convert.ToInt32(record["category_id"]),
            CategoryName = Convert.ToString(record["category_name"]) ?? string.Empty,
            ParentId = record["parent_id"] == DBNull.Value ? null : Convert.ToInt32(record["parent_id"]),
            SortOrder = Convert.ToInt32(record["sort_order"]),
            CreatedAt = Convert.ToDateTime(record["created_at"])
        };
    }
}

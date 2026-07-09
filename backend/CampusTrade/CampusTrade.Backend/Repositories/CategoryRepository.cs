using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using System.Data;

namespace CampusTrade.Backend.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT
                category_id AS CategoryId,
                category_name AS CategoryName,
                parent_id AS ParentId,
                sort_order AS SortOrder
            FROM category
            ORDER BY sort_order ASC, category_id ASC
        """;
        var categories = await connection.QueryAsync<CategoryDto>(sql);
        return categories.ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT
                category_id AS CategoryId,
                category_name AS CategoryName,
                parent_id AS ParentId,
                sort_order AS SortOrder
            FROM category
            WHERE category_id = :CategoryId
        """;
        return await connection.QueryFirstOrDefaultAsync<CategoryDto>(sql, new { CategoryId = categoryId });
    }

    public async Task<int> CreateAsync(CreateCategoryRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO category (category_name, parent_id, sort_order, created_at)
            VALUES (:CategoryName, :ParentId, :SortOrder, SYSDATE)
            RETURNING category_id INTO :NewId
        """;
        var parameters = new DynamicParameters();
        parameters.Add(":CategoryName", request.CategoryName);
        parameters.Add(":ParentId", request.ParentId, DbType.Int32);
        parameters.Add(":SortOrder", request.SortOrder);
        parameters.Add(":NewId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(sql, parameters);
        return parameters.Get<int>(":NewId");
    }

    public async Task<bool> UpdateAsync(int categoryId, UpdateCategoryRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add(":CategoryId", categoryId);

        if (!string.IsNullOrWhiteSpace(request.CategoryName))
        {
            updates.Add("category_name = :CategoryName");
            parameters.Add(":CategoryName", request.CategoryName);
        }
        if (request.ParentId.HasValue)
        {
            updates.Add("parent_id = :ParentId");
            parameters.Add(":ParentId", request.ParentId.Value);
        }
        if (request.SortOrder.HasValue)
        {
            updates.Add("sort_order = :SortOrder");
            parameters.Add(":SortOrder", request.SortOrder.Value);
        }

        if (updates.Count == 0) return true;

        var sql = $@"UPDATE category SET {string.Join(", ", updates)} WHERE category_id = :CategoryId";
        var affected = await connection.ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM category WHERE category_id = :CategoryId";
        var affected = await connection.ExecuteAsync(sql, new { CategoryId = categoryId });
        return affected > 0;
    }

    public async Task<bool> HasChildrenAsync(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM category WHERE parent_id = :CategoryId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
        return count > 0;
    }

    public async Task<int> GetMaxSortOrderAsync(int? parentId)
   {
        using var connection = _connectionFactory.CreateConnection();
        string sql = @"
            SELECT NVL(MAX(sort_order), 0)
            FROM category
            WHERE (parent_id IS NULL AND :ParentId IS NULL)
               OR (parent_id = :ParentId)
        ";
        var max = await connection.ExecuteScalarAsync<int>(sql, new { ParentId = parentId });
        return max;
    }


    public async Task<bool> HasGoodsAsync(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM goods WHERE category_id = :CategoryId";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });
        return count > 0;
    }
}



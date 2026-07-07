using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllAsync();
}

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
}
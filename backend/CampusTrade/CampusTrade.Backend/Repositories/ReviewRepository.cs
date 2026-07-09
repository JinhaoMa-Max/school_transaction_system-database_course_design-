using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 评价数据访问层（F21）
/// 查询: v_review_detail 视图  事务: sp_create_review（校验+写入+自动更新信用分）
/// 辅助: fn_avg_rating / fn_calc_credit
/// </summary>
public class ReviewRepository : IReviewRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public ReviewRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    /// <summary>评价列表 — 查 v_review_detail（含评价者/被评者昵称、商品名）</summary>
public async Task<(List<ReviewDto> Items, int Total)> GetPagedAsync(int page, int size, int? reviewerId, int? reviewedUserId, int? orderId)
{
    using var connection = _connectionFactory.CreateConnection();
    var where = new List<string>();
    var p = new DynamicParameters();
    if (reviewerId.HasValue) { where.Add("reviewer_id = :Rid"); p.Add(":Rid", reviewerId.Value); }
    if (reviewedUserId.HasValue) { where.Add("reviewed_user_id = :Ruid"); p.Add(":Ruid", reviewedUserId.Value); }
    if (orderId.HasValue) { where.Add("order_id = :Oid"); p.Add(":Oid", orderId.Value); }
    var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
    var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM v_review_detail {w}", p);
    var off = (page - 1) * size;
    var sql = $"""
        SELECT review_id AS ReviewId, order_id AS OrderId, reviewer_id AS ReviewerId,
               reviewer_name AS ReviewerName, reviewed_user_id AS ReviewedUserId,
               reviewed_user_name AS ReviewedUserName, rating AS Rating, content AS Content,
               created_at AS CreateTime, goods_id AS GoodsId, goods_title AS GoodsTitle
        FROM v_review_detail {w} ORDER BY created_at DESC
        OFFSET {off} ROWS FETCH NEXT {size} ROWS ONLY
        """;
    var items = await connection.QueryAsync<ReviewDto>(sql, p);
    return (items.ToList(), total);
}

    public async Task<ReviewDto?> GetByIdAsync(int reviewId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT review_id AS ReviewId, order_id AS OrderId, reviewer_id AS ReviewerId,
                   reviewer_name AS ReviewerName, reviewed_user_id AS ReviewedUserId,
                   reviewed_user_name AS ReviewedUserName, rating AS Rating, content AS Content,
                   created_at AS CreateTime, goods_id AS GoodsId, goods_title AS GoodsTitle
            FROM v_review_detail WHERE review_id = :Id
            """;
        return await connection.QueryFirstOrDefaultAsync<ReviewDto>(sql, new { Id = reviewId });
    }

    /// <summary>创建评价 — sp_create_review: 校验已完成+不重复+自动更新信用分</summary>
    public async Task<int> CreateAsync(int orderId, int reviewerId, int rating, string? content)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_create_review(p_order_id=>:o, p_reviewer_id=>:r, p_rating=>:ra, p_content=>:c, p_review_id=>:outId); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("o", OracleDbType.Int32) { Value = orderId });
        cmd.Parameters.Add(new OracleParameter("r", OracleDbType.Int32) { Value = reviewerId });
        cmd.Parameters.Add(new OracleParameter("ra", OracleDbType.Int32) { Value = rating });
        cmd.Parameters.Add(new OracleParameter("c", OracleDbType.Clob) { Value = content ?? (object)DBNull.Value });
        var outP = new OracleParameter("outId", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
        cmd.Parameters.Add(outP);
        await cmd.ExecuteNonQueryAsync();
        return Convert.ToInt32(outP.Value?.ToString());
    }

    public async Task<bool> HasReviewedAsync(int orderId, int reviewerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM review WHERE order_id=:o AND reviewer_id=:r", new { o = orderId, r = reviewerId }) > 0;
    }

    public async Task<decimal> GetAvgRatingAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>("SELECT fn_avg_rating(:u) FROM DUAL", new { u = userId });
    }

    public async Task<int> RecalcCreditAsync(int userId)
{
    using var connection = _connectionFactory.CreateConnection();
    return await connection.ExecuteScalarAsync<int>("SELECT fn_calc_credit(:u) FROM DUAL", new { u = userId });
}

public async Task<bool> UpdateAsync(int reviewId, int rating, string? content)
{
    using var connection = _connectionFactory.CreateConnection();
    const string sql = "UPDATE review SET rating=:r, content=:c, updated_at=SYSTIMESTAMP WHERE review_id=:id";
    var rows = await connection.ExecuteAsync(sql, new { r = rating, c = content ?? (object)DBNull.Value, id = reviewId });
    return rows > 0;
}

public async Task<bool> DeleteAsync(int reviewId)
{
    using var connection = _connectionFactory.CreateConnection();
    const string sql = "DELETE FROM review WHERE review_id=:id";
    var rows = await connection.ExecuteAsync(sql, new { id = reviewId });
    return rows > 0;
}
}

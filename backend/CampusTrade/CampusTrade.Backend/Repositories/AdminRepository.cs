using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Globalization;

namespace CampusTrade.Backend.Repositories;

public class AdminRepository : IAdminRepository
{
    private const string AuditLogColumns = """
        log_id AS LogId,
        admin_id AS AdminId,
        audit_type AS AuditType,
        target_id AS TargetId,
        action AS Action,
        result AS Result,
        remark AS Remark,
        created_at AS HandleTime
        """;

    private const string NoticeColumns = """
        notice_id AS NoticeId,
        title AS Title,
        content AS Content,
        notice_type AS NoticeType,
        publisher_id AS PublisherId,
        created_at AS PublishTime
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public AdminRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<AuditLogDto> Items, int Total)> GetAuditLogsAsync(int page, int size, int? adminId, string? auditType)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        var where = new List<string>();

        if (adminId.HasValue)
        {
            where.Add("admin_id = :AdminId");
            parameters.Add(":AdminId", adminId.Value);
        }

        if (!string.IsNullOrWhiteSpace(auditType))
        {
            where.Add("audit_type = :AuditType");
            parameters.Add(":AuditType", auditType.Trim());
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM audit_log {whereClause}", parameters);
        var offset = (page - 1) * size;
        var sql = $"""
            SELECT {AuditLogColumns}
            FROM audit_log
            {whereClause}
            ORDER BY created_at DESC, log_id DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;

        var items = await connection.QueryAsync<AuditLogDto>(sql, parameters);
        return (items.ToList(), total);
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(int logId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT {AuditLogColumns}
            FROM audit_log
            WHERE log_id = :LogId
            """;
        return await connection.QuerySingleOrDefaultAsync<AuditLogDto>(sql, new { LogId = logId });
    }

    public async Task<AuditLogDto> CreateAuditLogAsync(CreateAuditLogRequest request, int adminId)
    {
        var logId = await InsertAuditLogAsync(request, adminId);
        return await GetAuditLogByIdAsync(logId)
            ?? throw new InvalidOperationException("audit log was created but could not be loaded");
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var userCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM users");
        var goodsCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM goods");
        var orderCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM orders");
        var reportCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM report");
        return new AdminStatsDto
        {
            UserCount = userCount,
            GoodsCount = goodsCount,
            OrderCount = orderCount,
            ReportCount = reportCount
        };
    }

    public async Task<(List<NoticeDto> Items, int Total)> GetNoticesAsync(int page, int size, string? noticeType)
    {
        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();
        var where = new List<string>();

        if (!string.IsNullOrWhiteSpace(noticeType))
        {
            where.Add("notice_type = :NoticeType");
            parameters.Add(":NoticeType", noticeType.Trim());
        }

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM notice {whereClause}", parameters);
        var offset = (page - 1) * size;
        var sql = $"""
            SELECT {NoticeColumns}
            FROM notice
            {whereClause}
            ORDER BY created_at DESC, notice_id DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;

        var items = await connection.QueryAsync<NoticeDto>(sql, parameters);
        return (items.ToList(), total);
    }

    public async Task<NoticeDto?> GetNoticeByIdAsync(int noticeId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT {NoticeColumns}
            FROM notice
            WHERE notice_id = :NoticeId
            """;
        return await connection.QuerySingleOrDefaultAsync<NoticeDto>(sql, new { NoticeId = noticeId });
    }

    public async Task<NoticeDto> CreateNoticeAsync(CreateNoticeRequest request, int publisherId)
    {
        var noticeId = await InsertNoticeAsync(request, publisherId);
        return await GetNoticeByIdAsync(noticeId)
            ?? throw new InvalidOperationException("notice was created but could not be loaded");
    }

    public async Task<NoticeDto?> UpdateNoticeAsync(int noticeId, UpdateNoticeRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sets = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add(":NoticeId", noticeId);

        if (request.Title != null)
        {
            sets.Add("title = :Title");
            parameters.Add(":Title", request.Title.Trim());
        }

        if (request.Content != null)
        {
            sets.Add("content = :Content");
            parameters.Add(":Content", request.Content.Trim());
        }

        if (request.NoticeType != null)
        {
            sets.Add("notice_type = :NoticeType");
            parameters.Add(":NoticeType", request.NoticeType.Trim());
        }

        if (sets.Count == 0)
        {
            return await GetNoticeByIdAsync(noticeId);
        }

        var sql = $"UPDATE notice SET {string.Join(", ", sets)} WHERE notice_id = :NoticeId";
        var affected = await connection.ExecuteAsync(sql, parameters);
        return affected == 0 ? null : await GetNoticeByIdAsync(noticeId);
    }

    public async Task<bool> DeleteNoticeAsync(int noticeId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM notice WHERE notice_id = :NoticeId";
        return await connection.ExecuteAsync(sql, new { NoticeId = noticeId }) > 0;
    }

    private async Task<int> InsertAuditLogAsync(CreateAuditLogRequest request, int adminId)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oracleConnection)
        {
            throw new InvalidOperationException("Expected OracleConnection");
        }

        await oracleConnection.OpenAsync();
        const string sql = """
            INSERT INTO audit_log (admin_id, audit_type, target_id, action, result, remark)
            VALUES (:AdminId, :AuditType, :TargetId, :Action, :Result, :Remark)
            RETURNING log_id INTO :NewId
            """;

        using var command = new OracleCommand(sql, oracleConnection) { BindByName = true };
        command.Parameters.Add(new OracleParameter("AdminId", OracleDbType.Int32) { Value = adminId });
        command.Parameters.Add(new OracleParameter("AuditType", OracleDbType.Varchar2, 30) { Value = request.AuditType.Trim() });
        command.Parameters.Add(new OracleParameter("TargetId", OracleDbType.Int32) { Value = request.TargetId });
        command.Parameters.Add(new OracleParameter("Action", OracleDbType.Varchar2, 50) { Value = request.Action.Trim() });
        command.Parameters.Add(new OracleParameter("Result", OracleDbType.Varchar2, 100) { Value = string.IsNullOrWhiteSpace(request.Result) ? "success" : request.Result.Trim() });
        command.Parameters.Add(new OracleParameter("Remark", OracleDbType.Clob) { Value = string.IsNullOrWhiteSpace(request.Remark) ? DBNull.Value : request.Remark.Trim() });
        var idParameter = new OracleParameter("NewId", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        command.Parameters.Add(idParameter);

        await command.ExecuteNonQueryAsync();
        return ReadOutputInt(idParameter);
    }

    private async Task<int> InsertNoticeAsync(CreateNoticeRequest request, int publisherId)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oracleConnection)
        {
            throw new InvalidOperationException("Expected OracleConnection");
        }

        await oracleConnection.OpenAsync();
        const string sql = """
            INSERT INTO notice (title, content, notice_type, publisher_id)
            VALUES (:Title, :Content, :NoticeType, :PublisherId)
            RETURNING notice_id INTO :NewId
            """;

        using var command = new OracleCommand(sql, oracleConnection) { BindByName = true };
        command.Parameters.Add(new OracleParameter("Title", OracleDbType.Varchar2, 200) { Value = request.Title.Trim() });
        command.Parameters.Add(new OracleParameter("Content", OracleDbType.Clob) { Value = request.Content.Trim() });
        command.Parameters.Add(new OracleParameter("NoticeType", OracleDbType.Varchar2, 30) { Value = request.NoticeType.Trim() });
        command.Parameters.Add(new OracleParameter("PublisherId", OracleDbType.Int32) { Value = publisherId });
        var idParameter = new OracleParameter("NewId", OracleDbType.Int32) { Direction = ParameterDirection.Output };
        command.Parameters.Add(idParameter);

        await command.ExecuteNonQueryAsync();
        return ReadOutputInt(idParameter);
    }

    private static int ReadOutputInt(OracleParameter parameter)
    {
        return Convert.ToInt32(parameter.Value.ToString(), CultureInfo.InvariantCulture);
    }
}
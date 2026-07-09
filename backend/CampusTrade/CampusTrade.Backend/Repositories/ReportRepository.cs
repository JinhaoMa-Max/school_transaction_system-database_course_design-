using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 举报数据访问层（F22-F23）
/// 数据库: report 表 / sp_handle_report / trg_report_threshold_alert（触发器自动）
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public ReportRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    /// <summary>举报列表</summary>
    public async Task<(List<ReportDto> Items, int Total)> GetPagedAsync(int page, int size, string? reportType, string? status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var where = new List<string>();
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(reportType)) { where.Add("report_type = :T"); p.Add(":T", reportType); }
        if (!string.IsNullOrWhiteSpace(status)) { where.Add("report_status = :S"); p.Add(":S", status); }
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM report {w}", p);
        var off = (page - 1) * size;
        var sql = $"""
            SELECT report_id AS ReportId, reporter_id AS ReporterId, report_type AS ReportType,
                   target_goods_id AS ReportedGoodsId, target_user_id AS ReportedUserId,
                   target_order_id AS ReportedOrderId, reason AS Reason,
                   report_status AS Status, created_at AS CreateTime
            FROM report {w} ORDER BY created_at DESC
            OFFSET {off} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<ReportDto>(sql, p);
        return (items.ToList(), total);
    }

    public async Task<ReportDto?> GetByIdAsync(int reportId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT report_id AS ReportId, reporter_id AS ReporterId, report_type AS ReportType,
                   target_goods_id AS ReportedGoodsId, target_user_id AS ReportedUserId,
                   target_order_id AS ReportedOrderId, reason AS Reason,
                   report_status AS Status, created_at AS CreateTime
            FROM report WHERE report_id = :Id
            """;
        return await connection.QueryFirstOrDefaultAsync<ReportDto>(sql, new { Id = reportId });
    }

    /// <summary>提交举报 — INSERT report，触发器 trg_report_threshold_alert 自动检测≥5次被举报</summary>
    public async Task<int> CreateAsync(int reporterId, string reportType, int? targetGoodsId, int? targetUserId, int? targetOrderId, string reason)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO report (reporter_id, report_type, target_goods_id, target_user_id, target_order_id, reason, report_status, created_at)
            VALUES (:Rid, :RT, :TGid, :TUid, :TOid, :Reason, 'pending', SYSDATE)
            RETURNING report_id INTO :NewId
            """;
        var p = new DynamicParameters();
        p.Add(":Rid", reporterId); p.Add(":RT", reportType);
        p.Add(":TGid", targetGoodsId ?? (object)DBNull.Value);
        p.Add(":TUid", targetUserId ?? (object)DBNull.Value);
        p.Add(":TOid", targetOrderId ?? (object)DBNull.Value);
        p.Add(":Reason", reason);
        p.Add(":NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        await connection.ExecuteAsync(sql, p);
        return p.Get<int>(":NewId");
    }

    /// <summary>处理举报 — sp_handle_report: 校验管理员身份→UPDATE report→INSERT audit_log</summary>
    public async Task<bool> HandleAsync(int reportId, int adminId, string result, string? remark)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_handle_report(p_report_id=>:rid, p_admin_id=>:aid, p_result=>:r, p_remark=>:rm); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("rid", OracleDbType.Int32) { Value = reportId });
        cmd.Parameters.Add(new OracleParameter("aid", OracleDbType.Int32) { Value = adminId });
        cmd.Parameters.Add(new OracleParameter("r", OracleDbType.Varchar2, 50) { Value = result });
        cmd.Parameters.Add(new OracleParameter("rm", OracleDbType.Clob) { Value = remark ?? (object)DBNull.Value });
        await cmd.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int reportId, string newStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("UPDATE report SET report_status=:s WHERE report_id=:r", new { r = reportId, s = newStatus }) > 0;
    }
}

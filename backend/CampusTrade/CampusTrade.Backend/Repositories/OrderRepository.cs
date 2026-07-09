using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(List<OrderDto> Items, int Total)> GetPagedAsync(int page, int size, int? buyerId, int? sellerId, string? status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var where = new List<string>();
        var p = new DynamicParameters();

        if (buyerId.HasValue) { where.Add("buyer_id = :BuyerId"); p.Add(":BuyerId", buyerId.Value); }
        if (sellerId.HasValue) { where.Add("seller_id = :SellerId"); p.Add(":SellerId", sellerId.Value); }
        if (!string.IsNullOrWhiteSpace(status)) { where.Add("order_status = :Status"); p.Add(":Status", status); }

        return await QueryPagedAsync(connection, page, size, where, p);
    }

    public async Task<(List<OrderDto> Items, int Total)> GetPagedByParticipantAsync(int page, int size, int userId, string? status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var where = new List<string> { "(buyer_id = :UserId OR seller_id = :UserId)" };
        var p = new DynamicParameters();
        p.Add(":UserId", userId);

        if (!string.IsNullOrWhiteSpace(status)) { where.Add("order_status = :Status"); p.Add(":Status", status); }

        return await QueryPagedAsync(connection, page, size, where, p);
    }

    public async Task<OrderDto?> GetByIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT order_id AS OrderId, goods_id AS GoodsId, goods_title AS GoodsTitle,
                   buyer_id AS BuyerId, buyer_name AS BuyerName, seller_id AS SellerId,
                   seller_name AS SellerName, final_price AS DealPrice, order_status AS Status,
                   created_at AS CreateTime, meet_time AS MeetTime, meet_place AS MeetLocation,
                   confirm_code AS ConfirmCode, buyer_reviewed AS BuyerReviewed,
                   seller_reviewed AS SellerReviewed
            FROM v_order_list WHERE order_id = :OrderId
            """;
        return await connection.QueryFirstOrDefaultAsync<OrderDto>(sql, new { OrderId = orderId });
    }

    public async Task<int> CreateAsync(int goodsId, int buyerId, decimal price)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_place_order(p_goods_id=>:g, p_buyer_id=>:b, p_price=>:p, p_order_id=>:o); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("g", OracleDbType.Int32) { Value = goodsId });
        cmd.Parameters.Add(new OracleParameter("b", OracleDbType.Int32) { Value = buyerId });
        cmd.Parameters.Add(new OracleParameter("p", OracleDbType.Decimal) { Value = price });
        var outP = new OracleParameter("o", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
        cmd.Parameters.Add(outP);
        await cmd.ExecuteNonQueryAsync();
        return Convert.ToInt32(outP.Value?.ToString());
    }

    public async Task<bool> CancelAsync(int orderId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = "BEGIN sp_cancel_order(p_order_id=>:o, p_user_id=>:u); END;";
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("o", OracleDbType.Int32) { Value = orderId });
        cmd.Parameters.Add(new OracleParameter("u", OracleDbType.Int32) { Value = userId });
        await cmd.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> CompleteAsync(int orderId, string confirmCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = "BEGIN sp_complete_meet(p_order_id=>:o, p_confirm_code=>:c); END;";
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("o", OracleDbType.Int32) { Value = orderId });
        cmd.Parameters.Add(new OracleParameter("c", OracleDbType.Varchar2, 10) { Value = confirmCode });
        await cmd.ExecuteNonQueryAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, string newStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE trade_order SET order_status=:s, updated_at=SYSDATE WHERE order_id=:o";
        return await connection.ExecuteAsync(sql, new { o = orderId, s = newStatus }) > 0;
    }

    public async Task<string> GenerateConfirmCodeAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<string>("SELECT fn_gen_confirm_code FROM DUAL") ?? string.Empty;
    }

    public async Task<int> CreateAppointmentAsync(int orderId, DateTime meetTime, string meetLocation, string confirmCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            INSERT INTO appointment (order_id, meet_time, meet_place, confirm_code, appointment_status, created_at)
            VALUES (:OrderId, :MeetTime, :MeetLocation, :Code, 'confirmed', SYSDATE)
            RETURNING appointment_id INTO :NewId
            """;
        var p = new DynamicParameters();
        p.Add(":OrderId", orderId);
        p.Add(":MeetTime", meetTime);
        p.Add(":MeetLocation", meetLocation);
        p.Add(":Code", confirmCode);
        p.Add(":NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
        await connection.ExecuteAsync(sql, p);
        return p.Get<int>(":NewId");
    }

    public async Task<AppointmentDto?> GetAppointmentByOrderIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT appointment_id AS AppointmentId, order_id AS OrderId, meet_time AS MeetTime,
                   meet_place AS MeetLocation, confirm_code AS ConfirmCode,
                   appointment_status AS Status, created_at AS CreateTime
            FROM appointment WHERE order_id = :OrderId
            """;
        return await connection.QueryFirstOrDefaultAsync<AppointmentDto>(sql, new { OrderId = orderId });
    }

    public async Task<bool> UpdateAppointmentStatusAsync(int appointmentId, string newStatus)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE appointment SET appointment_status=:s WHERE appointment_id=:a";
        return await connection.ExecuteAsync(sql, new { a = appointmentId, s = newStatus }) > 0;
    }

    public async Task<bool> IsParticipantAsync(int orderId, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM trade_order WHERE order_id=:o AND (buyer_id=:u OR seller_id=:u)";
        return await connection.ExecuteScalarAsync<int>(sql, new { o = orderId, u = userId }) > 0;
    }

    public async Task<bool> CanPurchaseAsync(int buyerId, int goodsId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT fn_can_purchase(:b,:g) FROM DUAL", new { b = buyerId, g = goodsId }) == 1;
    }

    public async Task<string?> GetStatusAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<string>("SELECT order_status FROM trade_order WHERE order_id=:o", new { o = orderId });
    }

    private static async Task<(List<OrderDto> Items, int Total)> QueryPagedAsync(
        System.Data.IDbConnection connection,
        int page,
        int size,
        List<string> where,
        DynamicParameters parameters)
    {
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM v_order_list {w}", parameters);
        var offset = (page - 1) * size;
        var sql = $"""
            SELECT order_id AS OrderId, goods_id AS GoodsId, goods_title AS GoodsTitle,
                   buyer_id AS BuyerId, buyer_name AS BuyerName, seller_id AS SellerId,
                   seller_name AS SellerName, final_price AS DealPrice, order_status AS Status,
                   created_at AS CreateTime, meet_time AS MeetTime, meet_place AS MeetLocation,
                   confirm_code AS ConfirmCode, buyer_reviewed AS BuyerReviewed,
                   seller_reviewed AS SellerReviewed
            FROM v_order_list {w} ORDER BY created_at DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<OrderDto>(sql, parameters);
        return (items.ToList(), total);
    }
}

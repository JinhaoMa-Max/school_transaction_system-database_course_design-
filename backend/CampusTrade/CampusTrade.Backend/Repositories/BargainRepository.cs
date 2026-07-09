using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models.DTOs;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace CampusTrade.Backend.Repositories;

/// <summary>
/// 议价数据访问层（F12-F13）
/// 查询: v_active_bargains 视图  事务: sp_create_bargain / sp_respond_bargain
/// 触发器: trg_bargain_price_check（报价≤原价，自动生效）
/// </summary>
public class BargainRepository : IBargainRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    public BargainRepository(IDbConnectionFactory connectionFactory) { _connectionFactory = connectionFactory; }

    /// <summary>议价列表 — 查 v_active_bargains 视图（含商品名、买家名、原价、卖家ID）</summary>
    public async Task<(List<BargainOfferDto> Items, int Total)> GetPagedAsync(int page, int size, int? goodsId, int? buyerId, int? sellerId, string? status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var where = new List<string>();
        var p = new DynamicParameters();
        if (goodsId.HasValue) { where.Add("goods_id = :Gid"); p.Add(":Gid", goodsId.Value); }
        if (buyerId.HasValue) { where.Add("buyer_id = :Bid"); p.Add(":Bid", buyerId.Value); }
        if (sellerId.HasValue) { where.Add("seller_id = :Sid"); p.Add(":Sid", sellerId.Value); }
        if (!string.IsNullOrWhiteSpace(status)) { where.Add("offer_status = :St"); p.Add(":St", status); }
        var w = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM v_active_bargains {w}", p);
        var off = (page - 1) * size;
        var sql = $"""
            SELECT offer_id AS BargainId, goods_id AS GoodsId, buyer_id AS BuyerId,
                   offer_price AS OfferPrice, seller_response AS SellerResult,
                   counter_price AS CounterPrice, offer_status AS Status, created_at AS CreateTime
            FROM v_active_bargains {w} ORDER BY created_at DESC
            OFFSET {off} ROWS FETCH NEXT {size} ROWS ONLY
            """;
        var items = await connection.QueryAsync<BargainOfferDto>(sql, p);
        return (items.ToList(), total);
    }

    public async Task<BargainOfferDto?> GetByIdAsync(int bargainId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT offer_id AS BargainId, goods_id AS GoodsId, buyer_id AS BuyerId,
                   offer_price AS OfferPrice, seller_response AS SellerResult,
                   counter_price AS CounterPrice, offer_status AS Status, created_at AS CreateTime
            FROM bargain_offer WHERE offer_id = :Id
            """;
        return await connection.QueryFirstOrDefaultAsync<BargainOfferDto>(sql, new { Id = bargainId });
    }

    /// <summary>创建议价 — sp_create_bargain: 校验非自议+商品approved+不重复，触发器校验收价≤原价</summary>
    public async Task<BargainOfferDto> CreateAsync(int goodsId, int buyerId, decimal offerPrice)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();
        const string sql = """
            BEGIN sp_create_bargain(p_goods_id=>:g, p_buyer_id=>:b, p_offer_price=>:p, p_offer_id=>:outId); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("g", OracleDbType.Int32) { Value = goodsId });
        cmd.Parameters.Add(new OracleParameter("b", OracleDbType.Int32) { Value = buyerId });
        cmd.Parameters.Add(new OracleParameter("p", OracleDbType.Decimal) { Value = offerPrice });
        var outP = new OracleParameter("outId", OracleDbType.Int32) { Direction = System.Data.ParameterDirection.Output };
        cmd.Parameters.Add(outP);
        await cmd.ExecuteNonQueryAsync();
        return (await GetByIdAsync(Convert.ToInt32(outP.Value?.ToString())))!;
    }

    /// <summary>卖家回复 — sp_respond_bargain: 校验卖家身份→更新状态（accepted/rejected/countered）</summary>
    public async Task<BargainOfferDto> RespondAsync(int bargainId, string sellerResult, decimal? counterPrice)
    {
        using var connection = _connectionFactory.CreateConnection();
        var newStatus = sellerResult switch { "accepted" => "accepted", "rejected" => "rejected", "countered" => "active", _ => throw new ArgumentException($"无效回复类型: {sellerResult}") };
        const string sql = """
            UPDATE bargain_offer SET seller_response=:sr, counter_price=:cp, offer_status=:ns, updated_at=SYSDATE
            WHERE offer_id=:id
            """;
        await connection.ExecuteAsync(sql, new { id = bargainId, sr = sellerResult, cp = counterPrice ?? (object)DBNull.Value, ns = newStatus });
        return (await GetByIdAsync(bargainId))!;
    }

    /// <summary>关闭议价 — 条件 UPDATE：仅 active 状态可关闭</summary>
    public async Task<bool> CloseAsync(int bargainId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("UPDATE bargain_offer SET offer_status='closed', updated_at=SYSDATE WHERE offer_id=:id AND offer_status='active'", new { id = bargainId }) > 0;
    }

    /// <summary>买家回应卖家还价 — sp_buyer_handle_bargain: 校验买家身份+议价状态+卖家已还价→原子更新</summary>
    public async Task<BargainOfferDto> BuyerHandleAsync(int bargainId, string buyerResult, decimal? offerPrice)
    {
        // buyerResult 由前端定义：accepted=接受卖家还价 / rejected=拒绝 / countered=继续还价
        // countered 时必须传 offerPrice，否则存储过程抛 ORA-20053
        await CallBuyerHandleSp(bargainId, buyerResult, offerPrice);
        return (await GetByIdAsync(bargainId))!;
    }

    /// <summary>调用 sp_buyer_handle_bargain 存储过程</summary>
    private async Task CallBuyerHandleSp(int bargainId, string buyerResult, decimal? offerPrice)
    {
        using var connection = _connectionFactory.CreateConnection();
        if (connection is not OracleConnection oc) throw new InvalidOperationException("Expected OracleConnection");
        await oc.OpenAsync();

        // 接口未传 buyer_id，先从 bargain_offer 查出 buyer_id，保证存储过程校验能通过
        var buyerId = await connection.QueryFirstOrDefaultAsync<int?>(
            "SELECT buyer_id FROM bargain_offer WHERE offer_id=:id", new { id = bargainId });

        const string sql = """
            BEGIN sp_buyer_handle_bargain(p_offer_id=>:oid, p_buyer_id=>:bid, p_buyer_result=>:br, p_offer_price=>:op); END;
            """;
        await using var cmd = new OracleCommand(sql, oc) { BindByName = true };
        cmd.Parameters.Add(new OracleParameter("oid", OracleDbType.Int32) { Value = bargainId });
        cmd.Parameters.Add(new OracleParameter("bid", OracleDbType.Int32) { Value = buyerId ?? 0 });
        cmd.Parameters.Add(new OracleParameter("br", OracleDbType.Varchar2, 20) { Value = buyerResult });
        cmd.Parameters.Add(new OracleParameter("op", OracleDbType.Decimal) { Value = offerPrice ?? (object)DBNull.Value });
        await cmd.ExecuteNonQueryAsync();
    }
}

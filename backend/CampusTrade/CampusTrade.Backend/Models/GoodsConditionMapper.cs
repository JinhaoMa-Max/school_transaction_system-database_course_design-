namespace CampusTrade.Backend.Models;

/// <summary>
/// 商品成色中英文映射
/// 前端使用英文枚举，数据库使用中文 CHECK 约束，本类负责双向转换
/// </summary>
public static class GoodsConditionMapper
{
    private static readonly Dictionary<string, string> EnToCn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["new"] = "全新",
        ["like_new"] = "几乎全新",
        ["slight_use"] = "轻微使用",
        ["obvious_trace"] = "明显痕迹"
    };

    private static readonly Dictionary<string, string> CnToEn = new()
    {
        ["全新"] = "new",
        ["几乎全新"] = "like_new",
        ["轻微使用"] = "slight_use",
        ["明显痕迹"] = "obvious_trace"
    };

    /// <summary>
    /// 前端英文 → 数据库中文（写入时调用）
    /// 如果已是中文则直接返回
    /// </summary>
    public static string ToDatabase(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return "全新"; // 默认值

        return EnToCn.TryGetValue(condition.Trim(), out var cn)
            ? cn
            : condition.Trim(); // 已是中文或未知值，原样返回（由 DB CHECK 兜底）
    }

    /// <summary>
    /// 数据库中文 → 前端英文（读取时调用）
    /// 如果已是英文则直接返回
    /// </summary>
    public static string ToApi(string? condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return "new";

        return CnToEn.TryGetValue(condition.Trim(), out var en)
            ? en
            : condition.Trim(); // 已是英文或未知值，原样返回
    }

    /// <summary>
    /// 批量转换：将列表中所有 DTO 的 Condition 字段转为前端英文
    /// </summary>
    public static void TranslateListToApi(IEnumerable<DTOs.GoodsDto> items)
    {
        foreach (var item in items)
        {
            item.Condition = ToApi(item.Condition);
        }
    }
}

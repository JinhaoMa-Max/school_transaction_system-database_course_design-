# 举报功能修复说明文档

> 修复日期：2026-07-10  
> 分支：Review_renew

---

## 一、修复概览

本次修复解决了举报（Report）功能后端代码中的 **7 个问题**，涵盖从接口定义、DTO 字段对齐、业务校验、控制器到依赖注入的完整链路。

| # | 问题 | 严重程度 | 状态 |
|---|------|:---:|:---:|
| 1 | `IReportService.cs` 接口缺失 | 🔴 编译失败 | ✅ 已修复 |
| 2 | DTO 字段名前后端不一致 | 🔴 数据丢失 | ✅ 已修复 |
| 3 | `HandleReportRequest.Result` vs `Status` 属性名错误 | 🔴 编译失败 | ✅ 已修复 |
| 4 | `ReportController.cs` 缺少管理权和异常处理 | 🟡 功能缺陷 | ✅ 已修复 |
| 5 | 订单举报缺少买卖方校验 | 🟡 业务逻辑缺失 | ✅ 已修复 |
| 6 | 举报目标存在性未校验 | 🟡 数据完整性 | ✅ 已修复 |
| 7 | 用户可举报自己 | 🟡 逻辑漏洞 | ✅ 已修复 |

---

## 二、逐文件变更明细

### 2.1 新建：`Services/IReportService.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Services/IReportService.cs`

**原因**: `ReportService.cs` 声明了 `public class ReportService : IReportService`，但接口文件不存在，导致 CS0246 编译错误。

**内容**: 定义了 4 个方法签名，与 `ReportService` 的实现一一对应：

```csharp
public interface IReportService
{
    Task<ApiResponse<ReportListResult>> GetReportsAsync(int page, int size, string? reportType, string? status);
    Task<ApiResponse<ReportDto>> GetReportByIdAsync(int reportId);
    Task<ApiResponse<ReportDto>> CreateReportAsync(CreateReportRequest request, int currentUserId);
    Task<ApiResponse<ReportDto>> HandleReportAsync(int reportId, HandleReportRequest request, int adminId);
}
```

---

### 2.2 修改：`Models/DTOs/ReportDto.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Models/DTOs/ReportDto.cs`

**原因**: 两处字段名不一致：

| 位置 | 旧名称 | 新名称 | 原因 |
|------|--------|--------|------|
| `ReportDto` | `TargetGoodsId` | `ReportedGoodsId` | 与 SQL 别名 `AS ReportedGoodsId`、前端 `reportedGoodsId` 对齐 |
| `ReportDto` | `TargetUserId` | `ReportedUserId` | 同上 |
| `ReportDto` | `TargetOrderId` | `ReportedOrderId` | 同上 |
| `CreateReportRequest` | `TargetGoodsId` | `ReportedGoodsId` | 与前端 POST body 字段对齐 |
| `CreateReportRequest` | `TargetUserId` | `ReportedUserId` | 同上 |
| `CreateReportRequest` | `TargetOrderId` | `ReportedOrderId` | 同上 |
| `HandleReportRequest` | `Result` | `Status` | 与前端 PUT body 字段 `status`、Service 内部 `request.Status` 对齐 |

> **影响范围**: Repository 的 SQL 别名早已使用 `Reported*` 命名，Service 层代码也一直引用 `request.Reported*`，只是 DTO 层未同步。修复后 Dapper 映射、JSON 序列化/反序列化全部走通。

---

### 2.3 修改：`Repositories/IReportRepository.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Repositories/IReportRepository.cs`

**新增方法**:

```csharp
/// <summary>校验举报目标是否存在</summary>
Task<bool> TargetExistsAsync(string reportType, int targetId);
```

---

### 2.4 修改：`Repositories/ReportRepository.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Repositories/ReportRepository.cs`

**新增实现**:

```csharp
public async Task<bool> TargetExistsAsync(string reportType, int targetId)
{
    using var connection = _connectionFactory.CreateConnection();
    var sql = reportType switch
    {
        "goods" => "SELECT COUNT(1) FROM goods WHERE goods_id = :Id",
        "user"  => "SELECT COUNT(1) FROM app_user WHERE user_id = :Id",
        "order" => "SELECT COUNT(1) FROM trade_order WHERE order_id = :Id",
        _       => "SELECT 0 FROM DUAL"
    };
    return await connection.ExecuteScalarAsync<int>(sql, new { Id = targetId }) > 0;
}
```

---

### 2.5 修改：`Services/ReportService.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Services/ReportService.cs`

**变更摘要**:

#### a) 解决合并冲突
文件中存在 `<<<<<<< Updated upstream` / `=======` / `>>>>>>> Stashed changes` 标记，已合并为正确版本。

#### b) 注入 `IOrderRepository`
```csharp
// 原来
public ReportService(IReportRepository reportRepository)

// 现在
public ReportService(IReportRepository reportRepository, IOrderRepository orderRepository)
```

#### c) 新增完整的业务校验链

`CreateReportAsync` 方法现在按举报类型执行 `switch` 分支校验：

```
CreateReportAsync(request, currentUserId)
├── 基础校验
│   ├── reportType 非空
│   ├── reportType ∈ {goods, user, order}
│   ├── 至少一个目标ID已填写
│   └── reason 非空
│
├── 专项校验 (switch reportType)
│   ├── case "goods":
│   │   ├── ReportedGoodsId 已填写
│   │   └── 商品存在性 (TargetExistsAsync) → 404
│   │
│   ├── case "user":
│   │   ├── ReportedUserId 已填写
│   │   ├── 不能举报自己 → 400
│   │   └── 用户存在性 (TargetExistsAsync) → 404
│   │
│   └── case "order":
│       ├── ReportedOrderId 已填写
│       ├── 订单存在性 (TargetExistsAsync) → 404
│       └── 是否为买卖方 (IsParticipantAsync) → 403
│
└── 创建举报记录
```

**关键错误码语义**:

| HTTP 状态码 | 场景 | 示例消息 |
|:---:|---|---|
| 400 | 参数/业务规则校验失败 | "不能举报自己" |
| 403 | 订单举报时非买卖方 | "只有订单的买卖方才能举报该订单" |
| 404 | 举报目标不存在 | "被举报的订单不存在" |

---

### 2.6 修改：`Controllers/ReportController.cs`

**路径**: `backend/CampusTrade/CampusTrade.Backend/Controllers/ReportController.cs`

**变更摘要**:

#### a) 注入 `IAdminService`
```csharp
// 原来
public ReportController(IReportService reportService, IAuthService authService)

// 现在
public ReportController(IReportService reportService, IAuthService authService, IAdminService adminService)
```

#### b) 管理员权限校验
`HandleReport` 端点现在通过 `IAdminService.RequireAdminAsync()` 校验管理员身份，而非直接信任请求方声称的 adminId：

```csharp
// 原来：直接使用当前用户ID作为adminId，无权限校验
var adminId = ResolveCurrentUserId();

// 现在：通过 RequireAdminAsync 校验，非管理员会抛出 UnauthorizedAccessException
var adminId = await _adminService.RequireAdminAsync(ResolveCurrentUserId());
```

#### c) 统一异常处理
所有端点均包裹在 `try-catch` 中，通过 `ToErrorResult()` 分发为规范化的 HTTP 错误响应。

---

## 三、完整的数据流

```
POST /api/reports
Body: { reportType:"order", reportedOrderId:123, reason:"卖家不发货" }

    │
    ▼
ReportController.CreateReport()
    ├── [Authorize] → 验证 Bearer Token
    ├── ResolveCurrentUserId() → 获取当前用户ID
    │
    ▼
ReportService.CreateReportAsync(request, currentUserId)
    ├── 基础校验: reportType / reason / hasTarget
    ├── switch "order":
    │   ├── TargetExistsAsync("order", 123) → SELECT COUNT(1) FROM trade_order WHERE order_id=123
    │   │   └── 返回 false? → 404 "被举报的订单不存在"
    │   └── IsParticipantAsync(123, currentUserId) → 查询 buyer_id / seller_id
    │       └── 返回 false? → 403 "只有订单的买卖方才能举报该订单"
    │
    ▼
ReportRepository.CreateAsync()
    ├── INSERT INTO report (...)
    ├── 触发器 trg_report_threshold_alert 自动检查
    └── RETURNING report_id
```

---

## 四、编译验证

```
$ dotnet build
已成功生成。
    0 个警告
    0 个错误
```

---

## 五、测试建议

### 5.1 订单举报-买卖方校验

| 用例 | 请求方 | 目标订单 | 预期结果 |
|------|--------|----------|----------|
| 买方举报 | buyer_id=5 | order buyer=5, seller=8 | ✅ 200 成功 |
| 卖方举报 | seller_id=8 | order buyer=5, seller=8 | ✅ 200 成功 |
| 第三方举报 | user_id=99 | order buyer=5, seller=8 | ❌ 403 |
| 订单不存在 | 任意 | order_id=99999 | ❌ 404 |

### 5.2 用户举报-自举报阻止

| 用例 | 请求方 | 目标用户 | 预期结果 |
|------|--------|----------|----------|
| 举报他人 | user_id=5 | user_id=8 | ✅ 200 |
| 举报自己 | user_id=5 | user_id=5 | ❌ 400 |

### 5.3 目标存在性

| 用例 | 请求方 | 目标 | 预期结果 |
|------|--------|------|----------|
| 举报存在的商品 | user_id=5 | goods_id=1 | ✅ 200 |
| 举报不存在的商品 | user_id=5 | goods_id=99999 | ❌ 404 |
| 举报不存在的用户 | user_id=5 | user_id=99999 | ❌ 404 |

### 5.4 管理员处理

| 用例 | 请求方 | 预期结果 |
|------|--------|----------|
| 管理员处理举报 | role=admin | ✅ 200 |
| 普通用户处理举报 | role=user | ❌ 403 |
| 未登录处理举报 | 无token | ❌ 401 |

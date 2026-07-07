# 后端重构契约说明

本文只根据当前前端接口与数据库设计整理，不依赖现有后端实现。重构后端时，目标是把前端请求契约和 Oracle 数据库对象稳定连接起来。

## 1. 基础环境

前端请求统一以 `/api` 为前缀，开发环境通过 Vite 代理到后端：

```txt
Frontend dev: http://localhost:5173
Backend API:  http://localhost:5000
API prefix:   /api
```

数据库使用 Oracle：

```txt
Host:     localhost
Port:     1521
Service:  FREEPDB1
User:     CAMPUS
Password: Campus123456
```

.NET 连接串建议：

```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=CAMPUS;Password=Campus123456;Data Source=localhost:1521/FREEPDB1;"
  }
}
```

完整数据库能力需要执行以下脚本：

```txt
database/ddl/001_create_tables_docker.sql
database/ddl/003_views.sql
database/ddl/004_functions.sql
database/ddl/005_procedures.sql
database/ddl/006_triggers.sql
```

`002_verify.sql` 用于验证结构，`007_test.sql` 用于完整测试。

## 2. 后端分层建议

建议按以下分层实现：

```txt
Controller -> Service -> Repository -> Oracle
```

各层职责：

```txt
Controller: 处理 HTTP 请求、参数绑定、返回统一响应
Service:    处理业务规则、权限判断、调用函数/存储过程
Repository: 执行 SQL、查询视图、调用存储过程
Oracle:     基础表、视图、函数、过程、触发器
```

推荐使用方式：

```txt
简单 CRUD: 直接操作基础表
复杂查询: 优先查询数据库视图
复杂事务: 优先调用存储过程
业务校验: 可调用数据库函数
```

## 3. 通用响应格式

前端拦截器当前期望响应格式为：

```ts
{
  code: number
  message: string
  data: T
}
```

成功响应：

```json
{
  "code": 200,
  "message": "success",
  "data": {}
}
```

失败响应示例：

```json
{
  "code": 400,
  "message": "请求参数错误",
  "data": null
}
```

未登录或 Token 失效：

```json
{
  "code": 401,
  "message": "未登录或登录已过期",
  "data": null
}
```

分页响应数据结构：

```ts
{
  list: T[]
  total: number
  page: number
  size: number
}
```

注意：前端部分页面现在直接使用 `res.data.list`、`res.data.token`。如果后端返回外层包装 `{ code, message, data }`，前端 request 层最好统一解包为业务 `data`，否则页面代码和后端包装格式会冲突。

## 4. 认证与权限

登录成功后前端会保存 `accessToken`，后续请求会携带：

```txt
Authorization: Bearer <token>
```

后端应实现 JWT 或等价 Token 机制，并从 Token 中解析：

```txt
userId
username
role: buyer | seller | admin
```

很多接口不应信任前端传入的用户身份，应从 Token 获取当前用户，例如：

```txt
收藏商品
发起议价
下订单
发送消息
创建举报
创建评价
管理员审核
```

角色建议：

```txt
buyer:  买家
seller: 卖家
admin:  管理员
```

## 5. 前端要求的 API

### 5.1 Auth

```txt
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/current
POST /api/auth/register
POST /api/auth/student-auth
GET  /api/auth/student-auth/{userId}
PUT  /api/auth/student-auth/{authId}
```

登录入参：

```ts
{
  username: string
  password: string
}
```

登录返回：

```ts
{
  token: string
  user: User
}
```

### 5.2 User

```txt
GET    /api/users?page&size&role
GET    /api/users/{userId}
PUT    /api/users/{userId}
DELETE /api/users/{userId}
PUT    /api/users/{userId}/ban
PUT    /api/users/{userId}/unban
PUT    /api/users/{userId}/credit
```

`PUT /api/users/{userId}/credit` 入参：

```ts
{
  score: number
}
```

### 5.3 Category

```txt
GET    /api/categories
GET    /api/categories/{categoryId}
POST   /api/categories
PUT    /api/categories/{categoryId}
DELETE /api/categories/{categoryId}
```

### 5.4 Goods

```txt
GET    /api/goods?keyword&categoryId&minPrice&maxPrice&condition&status&page&size
GET    /api/goods/{goodsId}
POST   /api/goods
PUT    /api/goods/{goodsId}
DELETE /api/goods/{goodsId}
GET    /api/goods/{goodsId}/images
POST   /api/goods/{goodsId}/images
DELETE /api/goods/images/{imageId}
PUT    /api/goods/{goodsId}/audit
PUT    /api/goods/{goodsId}/offline
PUT    /api/goods/{goodsId}/view
```

商品查询参数：

```ts
{
  keyword?: string
  categoryId?: number
  minPrice?: number
  maxPrice?: number
  condition?: string
  status?: string
  page?: number
  size?: number
}
```

审核商品入参：

```ts
{
  status: "approved" | "rejected"
  remark?: string
}
```

上传图片入参：

```ts
{
  imageUrl: string
  sortOrder: number
}
```

### 5.5 Favorite

```txt
GET    /api/favorites?page&size
GET    /api/favorites/{favoriteId}
POST   /api/favorites
DELETE /api/favorites/{favoriteId}
GET    /api/favorites/check?goodsId={goodsId}
```

创建收藏入参：

```ts
{
  goodsId: number
}
```

### 5.6 Bargain

```txt
GET /api/bargains?goodsId&buyerId&status&page&size
GET /api/bargains/{bargainId}
POST /api/bargains
PUT /api/bargains/{bargainId}/handle
PUT /api/bargains/{bargainId}/close
```

创建议价入参：

```ts
{
  goodsId: number
  offerPrice: number
}
```

处理议价入参：

```ts
{
  sellerResult: "accepted" | "rejected" | "countered"
  counterPrice?: number
}
```

### 5.7 Order

```txt
GET  /api/orders?status&page&size
GET  /api/orders/{orderId}
POST /api/orders
PUT  /api/orders/{orderId}
PUT  /api/orders/{orderId}/cancel
PUT  /api/orders/{orderId}/complete
PUT  /api/orders/{orderId}/start-meet
```

创建订单入参：

```ts
{
  goodsId: number
  dealPrice: number
}
```

### 5.8 Appointment

```txt
GET  /api/appointments/order/{orderId}
POST /api/appointments
PUT  /api/appointments/{appointmentId}/confirm
PUT  /api/appointments/{appointmentId}/complete
PUT  /api/appointments/{appointmentId}/cancel
POST /api/appointments/verify
```

创建预约入参：

```ts
{
  orderId: number
  meetTime: string
  meetLocation: string
}
```

验证确认码入参：

```ts
{
  orderId: number
  confirmCode: string
}
```

### 5.9 Chat

```txt
GET  /api/chat/sessions
GET  /api/chat/sessions/{sessionId}
POST /api/chat/sessions
GET  /api/chat/sessions/{sessionId}/messages?page&size
POST /api/chat/messages
PUT  /api/chat/sessions/{sessionId}/read
GET  /api/chat/unread-count
```

创建会话入参：

```ts
{
  goodsId: number
  sellerId: number
}
```

发送消息入参：

```ts
{
  sessionId: number
  content: string
}
```

### 5.10 Review

```txt
GET    /api/reviews?orderId&reviewerId&reviewedUserId&page&size
GET    /api/reviews/{reviewId}
POST   /api/reviews
PUT    /api/reviews/{reviewId}
DELETE /api/reviews/{reviewId}
```

创建评价入参：

```ts
{
  orderId: number
  reviewedUserId: number
  rating: number
  content?: string
}
```

### 5.11 Report

```txt
GET  /api/reports?reportType&status&page&size
GET  /api/reports/{reportId}
POST /api/reports
PUT  /api/reports/{reportId}/handle
```

创建举报入参：

```ts
{
  reportType: "goods" | "user" | "order"
  reportedGoodsId?: number
  reportedUserId?: number
  reportedOrderId?: number
  reason: string
}
```

处理举报入参：

```ts
{
  status: "processing" | "resolved" | "rejected"
  remark?: string
}
```

### 5.12 Admin

```txt
GET  /api/admin/audit-logs?adminId&auditType&page&size
GET  /api/admin/audit-logs/{logId}
POST /api/admin/audit-logs

GET    /api/admin/notices?noticeType&page&size
GET    /api/admin/notices/{noticeId}
POST   /api/admin/notices
PUT    /api/admin/notices/{noticeId}
DELETE /api/admin/notices/{noticeId}
```

创建公告入参：

```ts
{
  title: string
  content: string
  noticeType: "system" | "transaction" | "violation"
}
```

## 6. 前端数据模型

### 6.1 User

```ts
{
  userId: number
  username: string
  password: string
  nickname: string
  avatarUrl: string
  phone: string
  email: string
  role: "buyer" | "seller" | "admin"
  status: "normal" | "banned"
  creditScore: number
  registerTime: string
}
```

### 6.2 StudentAuth

```ts
{
  authId: number
  userId: number
  studentId: string
  realName: string
  college: string
  authStatus: "pending" | "approved" | "rejected"
  authTime: string
}
```

### 6.3 Category

```ts
{
  categoryId: number
  categoryName: string
  parentId: number | null
  sortOrder: number
  children?: Category[]
}
```

### 6.4 Goods

```ts
{
  goodsId: number
  sellerId: number
  categoryId: number
  title: string
  description: string
  price: number
  condition: "new" | "like_new" | "slight_use" | "obvious_trace"
  status: "pending" | "approved" | "rejected" | "locked" | "sold" | "offline"
  viewCount: number
  publishTime: string
  imageUrl?: string
  sellerNickname?: string
}
```

### 6.5 Other Models

```ts
GoodsImage: { imageId, goodsId, imageUrl, sortOrder }
Favorite: { favoriteId, userId, goodsId, favoriteTime }
BargainOffer: { bargainId, goodsId, buyerId, offerPrice, sellerResult, counterPrice, status, createTime }
TradeOrder: { orderId, goodsId, buyerId, sellerId, dealPrice, status, createTime }
Appointment: { appointmentId, orderId, meetTime, meetLocation, confirmCode, status, createTime }
ChatSession: { sessionId, goodsId, buyerId, sellerId, createTime }
ChatMessage: { messageId, sessionId, senderId, content, readStatus, sendTime }
Review: { reviewId, orderId, reviewerId, reviewedUserId, rating, content, reviewTime }
Report: { reportId, reporterId, reportType, reportedGoodsId, reportedUserId, reportedOrderId, reason, status, reportTime }
AuditLog: { logId, adminId, auditType, targetId, action, result, remark, handleTime }
Notice: { noticeId, title, content, noticeType, publisherId, publishTime }
```

## 7. 数据库表

数据库基础表共 15 张：

```txt
app_user
student_auth
category
goods
goods_image
favorite
bargain_offer
trade_order
appointment
chat_session
chat_message
review
report
audit_log
notice
```

主要关系：

```txt
student_auth.user_id -> app_user.user_id
goods.seller_id -> app_user.user_id
goods.category_id -> category.category_id
goods_image.goods_id -> goods.goods_id
favorite.user_id -> app_user.user_id
favorite.goods_id -> goods.goods_id
bargain_offer.goods_id -> goods.goods_id
bargain_offer.buyer_id -> app_user.user_id
trade_order.goods_id -> goods.goods_id
trade_order.buyer_id -> app_user.user_id
trade_order.seller_id -> app_user.user_id
appointment.order_id -> trade_order.order_id
chat_session.goods_id -> goods.goods_id
chat_session.buyer_id -> app_user.user_id
chat_session.seller_id -> app_user.user_id
chat_message.session_id -> chat_session.session_id
review.order_id -> trade_order.order_id
report.target_goods_id -> goods.goods_id
report.target_user_id -> app_user.user_id
report.target_order_id -> trade_order.order_id
```

## 8. 字段映射重点

数据库使用 `snake_case`，前端需要 `camelCase`。后端 DTO 需要做映射。

```txt
app_user.user_id        -> userId
app_user.avatar         -> avatarUrl
app_user.credit_score   -> creditScore
app_user.created_at     -> registerTime

student_auth.auth_id    -> authId
student_auth.auth_status -> authStatus
student_auth.auth_time  -> authTime

category.category_id    -> categoryId
category.category_name  -> categoryName
category.parent_id      -> parentId
category.sort_order     -> sortOrder

goods.goods_id          -> goodsId
goods.seller_id         -> sellerId
goods.category_id       -> categoryId
goods.goods_condition   -> condition
goods.goods_status      -> status
goods.view_count        -> viewCount
goods.created_at        -> publishTime
v_goods_list.cover_image -> imageUrl
v_goods_list.seller_name -> sellerNickname

goods_image.image_id    -> imageId
goods_image.image_url   -> imageUrl
goods_image.sort_order  -> sortOrder

favorite.favorite_id    -> favoriteId
favorite.created_at     -> favoriteTime

bargain_offer.offer_id  -> bargainId
bargain_offer.seller_response -> sellerResult
bargain_offer.offer_status -> status
bargain_offer.created_at -> createTime

trade_order.order_id    -> orderId
trade_order.final_price -> dealPrice
trade_order.order_status -> status
trade_order.created_at  -> createTime

appointment.appointment_id -> appointmentId
appointment.meet_place  -> meetLocation
appointment.appointment_status -> status
appointment.created_at  -> createTime

chat_session.session_id -> sessionId
chat_session.created_at -> createTime

chat_message.message_id -> messageId
chat_message.is_read    -> readStatus
chat_message.created_at -> sendTime

review.review_id        -> reviewId
review.reviewed_user_id -> reviewedUserId
review.created_at       -> reviewTime

report.report_id        -> reportId
report.reporter_id      -> reporterId
report.target_goods_id  -> reportedGoodsId
report.target_user_id   -> reportedUserId
report.target_order_id  -> reportedOrderId
report.report_status    -> status
report.created_at       -> reportTime

audit_log.log_id        -> logId
audit_log.admin_id      -> adminId
audit_log.audit_type    -> auditType
audit_log.target_id     -> targetId
audit_log.created_at    -> handleTime

notice.notice_id        -> noticeId
notice.notice_type      -> noticeType
notice.publisher_id     -> publisherId
notice.created_at       -> publishTime
```

## 9. 枚举映射重点

前端商品成色：

```txt
new
like_new
slight_use
obvious_trace
```

数据库商品成色：

```txt
全新
几乎全新
轻微使用
明显痕迹
```

建议后端统一映射：

```txt
new           <-> 全新
like_new      <-> 几乎全新
slight_use    <-> 轻微使用
obvious_trace <-> 明显痕迹
```

其他状态基本可以直接使用数据库英文值：

```txt
goods_status: pending | approved | rejected | locked | sold | offline
order_status: pending_meet | in_meet | completed | cancelled
auth_status: pending | approved | rejected
report_status: pending | processing | resolved | rejected
appointment_status: pending | confirmed | completed | cancelled
```

## 10. 数据库视图

查询型接口优先使用视图：

```txt
v_goods_list      商品列表，含首图、卖家名、分类名
v_goods_detail    商品详情，含卖家信用、认证、图片数、收藏数
v_order_list      订单列表，含商品名、买卖双方、预约信息
v_user_profile    用户主页，含认证和统计信息
v_seller_stats    卖家排行或统计
v_pending_audit   待审核商品
v_unread_messages 未读消息
v_category_tree   分类树和商品数
v_review_detail   评价详情
v_active_bargains 活跃议价
```

示例：

```sql
SELECT *
FROM v_goods_list
WHERE category_id = :categoryId
ORDER BY created_at DESC;
```

## 11. 数据库函数

Service 层可调用函数做校验或计算：

```txt
fn_avg_rating(user_id)             用户平均评分
fn_sold_count(seller_id)           已售数量
fn_can_purchase(user_id, goods_id) 购买资格检查，返回 1/0
fn_unread_count(user_id)           未读消息数
fn_status_text(status, type)       状态转中文文本
fn_is_verified(user_id)            是否实名认证，返回 1/0
fn_gen_confirm_code                生成确认码
fn_calc_credit(user_id)            重算信用分
fn_favorite_goods_ids(user_id)     收藏商品 ID CSV
fn_increment_view(goods_id)        浏览量 +1，返回新浏览量
```

示例：

```sql
SELECT fn_can_purchase(:userId, :goodsId) FROM dual;
```

## 12. 数据库存储过程

复杂事务优先调用存储过程：

```txt
sp_register_user         用户注册，重名检查 + 插入用户
sp_place_order           下订单，校验购买资格 + 锁商品 + 关闭议价 + 插入订单
sp_cancel_order          取消订单，恢复商品，取消预约
sp_complete_meet         完成面交，订单 completed，商品 sold
sp_update_credit         更新信用分
sp_create_bargain        创建议价
sp_respond_bargain       卖家回复议价
sp_create_review         创建评价并更新信用分
sp_audit_goods           审核商品并记录日志
sp_handle_report         处理举报并记录日志
sp_manage_user_ban       封禁/解封用户
sp_send_message          发送消息
sp_get_or_create_session 获取或创建聊天会话
```

调用下单示例：

```sql
BEGIN
  sp_place_order(
    p_goods_id => :goodsId,
    p_buyer_id => :buyerId,
    p_price    => :price,
    p_order_id => :orderId
  );
END;
```

## 13. 数据库触发器

触发器会自动执行，后端不需要主动调用。

自动更新时间：

```txt
trg_app_user_updated_at
trg_goods_updated_at
trg_bargain_offer_updated_at
trg_trade_order_updated_at
trg_notice_updated_at
```

业务规则：

```txt
trg_order_status_log
trg_prevent_self_trade
trg_bargain_price_check
trg_goods_default_category
trg_after_review_credit
trg_report_threshold_alert
trg_cancel_appointment_on_order
```

## 14. 推荐实现顺序

按依赖关系重构：

```txt
1. 通用响应、异常处理、Oracle 连接工厂
2. JWT 认证、当前用户上下文、权限过滤
3. Auth / User / StudentAuth
4. Category
5. Goods / GoodsImage
6. Favorite
7. Bargain
8. Order / Appointment
9. Chat
10. Review
11. Report
12. Admin / Notice / AuditLog
```

## 15. 关键注意事项

1. 数据库用户是 `CAMPUS`，不要误写成其他用户。
2. 前端请求都在 `/api` 下。
3. 后端响应要和前端拦截器统一。
4. 需要从 Token 获取当前用户，不要让前端传敏感身份字段。
5. 数据库字段和前端字段命名不同，DTO 必须明确映射。
6. 商品成色枚举前后端不同，必须转换。
7. 查询列表优先使用视图，交易类操作优先使用存储过程。
8. 存储过程和触发器可能抛 Oracle 异常，后端要统一转换成业务错误响应。
9. 分页接口返回 `{ list, total, page, size }`。
10. 管理接口需要校验 `admin` 角色。


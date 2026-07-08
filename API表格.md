# CampusSecondHand 系统 API 一览表

> **统一前缀**: `/api`
> **说明**: 前端 `request.ts` 中 `baseURL` 已配置为 `/api`，所有前端 API 调用路径均以此前缀发起。后端控制器通过 `[Route("api/...")]` 定义路由。
> **状态**: ✅ = 后端已实现 &nbsp;&nbsp; ❌ = 后端未实现（仅前端定义/期望）

---

## 1. 认证模块 — `/api/auth`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/auth/login` | POST | 用户登录，返回 Token 和用户信息 | ✅ `AuthController` |
| `/api/auth/logout` | POST | 用户登出 | ✅ `AuthController` |
| `/api/auth/current` | GET | 获取当前登录用户信息（需 Bearer Token） | ✅ `AuthController` |
| `/api/auth/register` | POST | 用户注册 | ✅ `AuthController` |
| `/api/auth/student-auth` | POST | 提交学生身份认证信息 | ✅ `AuthController` |
| `/api/auth/student-auth/{userId}` | GET | 根据用户 ID 获取学生认证信息 | ✅ `AuthController` |
| `/api/auth/student-auth/{authId}` | PUT | 更新学生认证信息 | ✅ `AuthController` |

---

## 2. 用户管理模块 — `/api/users`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/users` | GET | 获取用户列表（支持 page/size/role 参数） | ❌ |
| `/api/users/{userId}` | GET | 根据 ID 获取用户详情 | ❌ |
| `/api/users/{userId}` | PUT | 更新用户信息 | ❌ |
| `/api/users/{userId}` | DELETE | 删除用户 | ❌ |
| `/api/users/{userId}/ban` | PUT | 封禁用户 | ❌ |
| `/api/users/{userId}/unban` | PUT | 解封用户 | ❌ |
| `/api/users/{userId}/credit` | PUT | 更新用户信用分 | ❌ |

---

## 3. 商品模块 — `/api/goods`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/goods` | GET | 获取商品列表（支持 page/size/categoryId/keyword/sortBy/ascending 参数） | ✅ `GoodsController` |
| `/api/goods/{goodsId}` | GET | 根据 ID 获取商品详情 | ✅ `GoodsController` |
| `/api/goods` | POST | 发布新商品 | ✅ `GoodsController` |
| `/api/goods/{goodsId}` | PUT | 更新商品信息 | ✅ `GoodsController` |
| `/api/goods/{goodsId}` | DELETE | 删除商品 | ✅ `GoodsController` |
| `/api/goods/{goodsId}/images` | GET | 获取商品图片列表 | ✅ `GoodsController` |
| `/api/goods/{goodsId}/images` | POST | 上传商品图片 | ✅ `GoodsController` |
| `/api/goods/images/{imageId}` | DELETE | 删除商品图片 | ✅ `GoodsController` |
| `/api/goods/{goodsId}/audit` | PUT | 审核商品（通过/拒绝） | ✅ `GoodsController` |
| `/api/goods/{goodsId}/offline` | PUT | 下架商品 | ✅ `GoodsController` |
| `/api/goods/{goodsId}/view` | PUT | 增加商品浏览量 | ✅ `GoodsController` |

---

## 4. 分类模块 — `/api/categories`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/categories` | GET | 获取所有分类列表 | ✅ `CategoriesController` |
| `/api/categories/{categoryId}` | GET | 根据 ID 获取分类详情 | ❌ |
| `/api/categories` | POST | 创建新分类 | ❌ |
| `/api/categories/{categoryId}` | PUT | 更新分类信息 | ❌ |
| `/api/categories/{categoryId}` | DELETE | 删除分类 | ❌ |

---

## 5. 订单模块 — `/api/orders`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/orders` | GET | 获取订单列表（支持 status 等参数） | ❌ |
| `/api/orders/{orderId}` | GET | 根据 ID 获取订单详情 | ❌ |
| `/api/orders` | POST | 创建订单（传入 goodsId 和 dealPrice） | ❌ |
| `/api/orders/{orderId}` | PUT | 更新订单信息 | ❌ |
| `/api/orders/{orderId}/cancel` | PUT | 取消订单 | ❌ |
| `/api/orders/{orderId}/complete` | PUT | 完成订单 | ❌ |
| `/api/orders/{orderId}/start-meet` | PUT | 开始面交 | ❌ |

---

## 6. 议价模块 — `/api/bargains`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/bargains` | GET | 获取议价列表（支持 goodsId/buyerId/status 参数） | ✅ `BargainsController` |
| `/api/bargains/{bargainId}` | GET | 根据 ID 获取议价详情 | ✅ `BargainsController` |
| `/api/bargains` | POST | 创建议价申请（传入 goodsId 和 offerPrice） | ✅ `BargainsController` |
| `/api/bargains/{bargainId}/handle` | PUT | 处理议价（接受/拒绝/还价） | ✅ `BargainsController` |
| `/api/bargains/{bargainId}/close` | PUT | 关闭议价 | ✅ `BargainsController` |

---

## 7. 面交预约模块 — `/api/appointments`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/appointments/order/{orderId}` | GET | 根据订单 ID 获取预约信息 | ❌ |
| `/api/appointments` | POST | 创建面交预约（时间/地点） | ❌ |
| `/api/appointments/{appointmentId}/confirm` | PUT | 确认预约 | ❌ |
| `/api/appointments/{appointmentId}/complete` | PUT | 完成预约 | ❌ |
| `/api/appointments/{appointmentId}/cancel` | PUT | 取消预约 | ❌ |
| `/api/appointments/verify` | POST | 验证确认码 | ❌ |

---

## 8. 聊天模块 — `/api/chat`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/chat/sessions` | GET | 获取聊天会话列表 | ❌ |
| `/api/chat/sessions/{sessionId}` | GET | 根据 ID 获取会话详情 | ❌ |
| `/api/chat/sessions` | POST | 创建聊天会话（传入 goodsId 和 sellerId） | ❌ |
| `/api/chat/sessions/{sessionId}/messages` | GET | 获取会话消息列表（支持分页） | ❌ |
| `/api/chat/messages` | POST | 发送消息 | ❌ |
| `/api/chat/sessions/{sessionId}/read` | PUT | 标记会话为已读 | ❌ |
| `/api/chat/unread-count` | GET | 获取未读消息数量 | ❌ |

---

## 9. 收藏模块 — `/api/favorites`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/favorites` | GET | 获取收藏列表（支持分页） | ❌ |
| `/api/favorites/{favoriteId}` | GET | 根据 ID 获取收藏详情 | ❌ |
| `/api/favorites` | POST | 添加收藏（传入 goodsId） | ❌ |
| `/api/favorites/{favoriteId}` | DELETE | 取消收藏 | ❌ |
| `/api/favorites/check` | GET | 检查商品是否已收藏（参数 goodsId） | ❌ |

---

## 10. 评价模块 — `/api/reviews`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/reviews` | GET | 获取评价列表（支持 orderId/reviewerId/reviewedUserId 参数） | ❌ |
| `/api/reviews/{reviewId}` | GET | 根据 ID 获取评价详情 | ❌ |
| `/api/reviews` | POST | 创建评价（订单评价/用户评分） | ❌ |
| `/api/reviews/{reviewId}` | PUT | 更新评价 | ❌ |
| `/api/reviews/{reviewId}` | DELETE | 删除评价 | ❌ |

---

## 11. 举报模块 — `/api/reports`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/reports` | GET | 获取举报列表（支持 reportType/status 参数） | ❌ |
| `/api/reports/{reportId}` | GET | 根据 ID 获取举报详情 | ❌ |
| `/api/reports` | POST | 创建举报（支持举报商品/用户/订单） | ❌ |
| `/api/reports/{reportId}/handle` | PUT | 处理举报（处理中/已解决/已驳回） | ❌ |

---

## 12. 管理员模块 — `/api/admin`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/admin/audit-logs` | GET | 获取审计日志列表（支持 adminId/auditType 参数） | ❌ |
| `/api/admin/audit-logs/{logId}` | GET | 获取审计日志详情 | ❌ |
| `/api/admin/audit-logs` | POST | 创建审计日志 | ❌ |
| `/api/admin/notices` | GET | 获取通知列表（支持 noticeType 参数） | ❌ |
| `/api/admin/notices/{noticeId}` | GET | 获取通知详情 | ❌ |
| `/api/admin/notices` | POST | 创建通知 | ❌ |
| `/api/admin/notices/{noticeId}` | PUT | 更新通知 | ❌ |
| `/api/admin/notices/{noticeId}` | DELETE | 删除通知 | ❌ |

---

## 13. 健康检查 — `/api/health`

| API 路径 | 方法 | 功能描述 | 后端状态 |
|---|---|---|---|
| `/api/health` | GET | 后端服务健康检查 | ✅ `HealthController` |

---

## 汇总统计

| 类别 | 总数 | 已实现 | 未实现 |
|---|---|---|---|
| 认证 (Auth) | 7 | 7 | 0 |
| 用户管理 (Users) | 7 | 0 | 7 |
| 商品 (Goods) | 11 | 11 | 0 |
| 分类 (Categories) | 5 | 1 | 4 |
| 订单 (Orders) | 7 | 0 | 7 |
| 议价 (Bargains) | 5 | 5 | 0 |
| 面交预约 (Appointments) | 6 | 0 | 6 |
| 聊天 (Chat) | 7 | 0 | 7 |
| 收藏 (Favorites) | 5 | 0 | 5 |
| 评价 (Reviews) | 5 | 0 | 5 |
| 举报 (Reports) | 4 | 0 | 4 |
| 管理员 (Admin) | 8 | 0 | 8 |
| 健康检查 (Health) | 1 | 1 | 0 |
| **合计** | **78** | **25** | **53** |

> ⚠️ 注意：`WeatherForecastController` 使用 `[Route("[controller]")]`，路由为 `/WeatherForecast`，不属于 `/api` 前缀体系，且前端未引用，为项目模板默认代码，建议清理。

# API 功能文档

> 本文档汇总了 `frontend/src/api/` 目录下所有 API 模块的功能、参数及返回值。

## 目录

- [auth.ts —— 认证模块](#authts--认证模块)
- [user.ts —— 用户模块](#userts--用户模块)
- [category.ts —— 分类模块](#categoryts--分类模块)
- [goods.ts —— 商品模块](#goodsts--商品模块)
- [favorite.ts —— 收藏模块](#favoritets--收藏模块)
- [bargain.ts —— 议价模块](#bargaints--议价模块)
- [order.ts —— 订单模块](#orderts--订单模块)
- [appointment.ts —— 预约模块](#appointmentts--预约模块)
- [chat.ts —— 聊天模块](#chatts--聊天模块)
- [review.ts —— 评价模块](#reviewts--评价模块)
- [report.ts —— 举报模块](#reportts--举报模块)
- [admin.ts —— 管理员模块](#admints--管理员模块)
- [index.ts —— 统一导出](#indexts--统一导出)

---

## auth.ts —— 认证模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `login` | `params: LoginParams` | `LoginResult` |
| `logout` | 无 | `void` |
| `getCurrentUser` | 无 | `User` |
| `register` | `params: Partial<User>` | `User` |
| `submitStudentAuth` | `params: Partial<StudentAuth>` | `StudentAuth` |
| `getStudentAuth` | `userId: number` | `StudentAuth` |
| `updateStudentAuth` | `authId: number`, `params: Partial<StudentAuth>` | `StudentAuth` |

## user.ts —— 用户模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getUserList` | `params?: { page?: number; size?: number; role?: string }` | `PageResult<User>` |
| `getUserById` | `userId: number` | `User` |
| `updateUser` | `userId: number`, `params: Partial<User>` | `User` |
| `deleteUser` | `userId: number` | `void` |
| `banUser` | `userId: number` | `void` |
| `unbanUser` | `userId: number` | `void` |
| `updateCreditScore` | `userId: number`, `score: number` | `void` |

## category.ts —— 分类模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getCategoryList` | 无 | `Category[]` |
| `getCategoryById` | `categoryId: number` | `Category` |
| `createCategory` | `params: Partial<Category>` | `Category` |
| `updateCategory` | `categoryId: number`, `params: Partial<Category>` | `Category` |
| `deleteCategory` | `categoryId: number` | `void` |

## goods.ts —— 商品模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getGoodsList` | `params?: GoodsQuery` | `PageResult<Goods>` |
| `getGoodsById` | `goodsId: number` | `Goods` |
| `createGoods` | `params: Partial<Goods>` | `Goods` |
| `updateGoods` | `goodsId: number`, `params: Partial<Goods>` | `Goods` |
| `deleteGoods` | `goodsId: number` | `void` |
| `getGoodsImages` | `goodsId: number` | `GoodsImage[]` |
| `uploadGoodsImage` | `goodsId: number`, `params: { imageUrl: string; sortOrder: number }` | `GoodsImage` |
| `deleteGoodsImage` | `imageId: number` | `void` |
| `auditGoods` | `goodsId: number`, `params: { status: 'approved' \| 'rejected'; remark?: string }` | `void` |
| `offlineGoods` | `goodsId: number` | `void` |
| `incrementViewCount` | `goodsId: number` | `void` |

## favorite.ts —— 收藏模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getFavoriteList` | `params?: { page?: number; size?: number }` | `PageResult<Favorite>` |
| `getFavoriteById` | `favoriteId: number` | `Favorite` |
| `createFavorite` | `params: { goodsId: number }` | `Favorite` |
| `deleteFavorite` | `favoriteId: number` | `void` |
| `checkFavorite` | `goodsId: number` | `void`（收藏状态） |

## bargain.ts —— 议价模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getBargainList` | `params?: { goodsId?: number; buyerId?: number; status?: string; page?: number; size?: number }` | `PageResult<BargainOffer>` |
| `getBargainById` | `bargainId: number` | `BargainOffer` |
| `createBargain` | `params: { goodsId: number; offerPrice: number }` | `BargainOffer` |
| `handleBargain` | `bargainId: number`, `params: { sellerResult: 'accepted' \| 'rejected' \| 'countered'; counterPrice?: number }` | `BargainOffer` |
| `closeBargain` | `bargainId: number` | `void` |

## order.ts —— 订单模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getOrderList` | `params?: OrderQuery` | `PageResult<TradeOrder>` |
| `getOrderById` | `orderId: number` | `TradeOrder` |
| `createOrder` | `params: { goodsId: number; dealPrice: number }` | `TradeOrder` |
| `updateOrder` | `orderId: number`, `params: Partial<TradeOrder>` | `TradeOrder` |
| `cancelOrder` | `orderId: number` | `void` |
| `completeOrder` | `orderId: number` | `void` |
| `startMeet` | `orderId: number` | `void` |

## appointment.ts —— 预约模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getAppointmentByOrderId` | `orderId: number` | `Appointment` |
| `createAppointment` | `params: { orderId: number; meetTime: string; meetLocation: string }` | `Appointment` |
| `confirmAppointment` | `appointmentId: number` | `void` |
| `completeAppointment` | `appointmentId: number` | `void` |
| `cancelAppointment` | `appointmentId: number` | `void` |
| `verifyConfirmCode` | `params: { orderId: number; confirmCode: string }` | `void` |

## chat.ts —— 聊天模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getSessionList` | 无 | `ChatSession[]` |
| `getSessionById` | `sessionId: number` | `ChatSession` |
| `createSession` | `params: { goodsId: number; sellerId: number }` | `ChatSession` |
| `getMessages` | `sessionId: number`, `params?: { page?: number; size?: number }` | `PageResult<ChatMessage>` |
| `sendMessage` | `params: { sessionId: number; content: string }` | `ChatMessage` |
| `markAsRead` | `sessionId: number` | `void` |
| `getUnreadCount` | 无 | `void`（未读消息数量） |

## review.ts —— 评价模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getReviewList` | `params?: { orderId?: number; reviewerId?: number; reviewedUserId?: number; page?: number; size?: number }` | `PageResult<Review>` |
| `getReviewById` | `reviewId: number` | `Review` |
| `createReview` | `params: { orderId: number; reviewedUserId: number; rating: number; content?: string }` | `Review` |
| `updateReview` | `reviewId: number`, `params: Partial<Review>` | `Review` |
| `deleteReview` | `reviewId: number` | `void` |

## report.ts —— 举报模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getReportList` | `params?: { reportType?: string; status?: string; page?: number; size?: number }` | `PageResult<Report>` |
| `getReportById` | `reportId: number` | `Report` |
| `createReport` | `params: { reportType: 'goods' \| 'user' \| 'order'; reportedGoodsId?: number; reportedUserId?: number; reportedOrderId?: number; reason: string }` | `Report` |
| `handleReport` | `reportId: number`, `params: { status: 'processing' \| 'resolved' \| 'rejected'; remark?: string }` | `Report` |

## admin.ts —— 管理员模块

| 功能名称 | 参数 | 返回值 |
|---|---|---|
| `getAuditLogList` | `params?: { adminId?: number; auditType?: string; page?: number; size?: number }` | `PageResult<AuditLog>` |
| `getAuditLogById` | `logId: number` | `AuditLog` |
| `createAuditLog` | `params: Partial<AuditLog>` | `AuditLog` |
| `getNoticeList` | `params?: { noticeType?: string; page?: number; size?: number }` | `PageResult<Notice>` |
| `getNoticeById` | `noticeId: number` | `Notice` |
| `createNotice` | `params: { title: string; content: string; noticeType: 'system' \| 'transaction' \| 'violation' }` | `Notice` |
| `updateNotice` | `noticeId: number`, `params: Partial<Notice>` | `Notice` |
| `deleteNotice` | `noticeId: number` | `void` |

## index.ts —— 统一导出

该文件为纯导出入口，通过 `export * from './xxx'` 统一聚合所有模块，不含独立函数实现。被导出的模块包括：

`auth` | `user` | `category` | `goods` | `favorite` | `bargain` | `order` | `appointment` | `chat` | `review` | `report` | `admin`

---

## 汇总统计

| 模块 | 文件 | 函数数量 |
|---|---|---|
| 认证 | `auth.ts` | 7 |
| 用户 | `user.ts` | 7 |
| 分类 | `category.ts` | 5 |
| 商品 | `goods.ts` | 11 |
| 收藏 | `favorite.ts` | 5 |
| 议价 | `bargain.ts` | 5 |
| 订单 | `order.ts` | 7 |
| 预约 | `appointment.ts` | 6 |
| 聊天 | `chat.ts` | 7 |
| 评价 | `review.ts` | 5 |
| 举报 | `report.ts` | 4 |
| 管理员 | `admin.ts` | 8 |
| **合计** | | **77** |

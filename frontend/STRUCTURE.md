# 前端项目结构文档

> 校园二手交易平台 — CampusSecondHand  
> 技术栈：Vue 3 + TypeScript + Vite + Arco Design Vue + Pinia + Vue Router + Axios

---

## 一、项目根目录

```
frontend/
├── index.html              # 入口 HTML
├── package.json             # 依赖与脚本
├── package-lock.json        # 依赖锁定
├── tsconfig.json            # TypeScript 编译配置
├── tsconfig.node.json       # Node 端 TS 配置（Vite 等）
├── vite.config.ts           # Vite 构建配置（别名 @、代理 /api → localhost:5000）
├── dist/                    # 构建产物
├── node_modules/            # 依赖包
└── src/                     # 源码（见下）
```

### 技术依赖

| 依赖 | 版本 | 用途 |
|------|------|------|
| `vue` | ^3.4.19 | UI 框架 |
| `vue-router` | ^4.3.0 | 前端路由 |
| `pinia` | ^2.1.7 | 状态管理 |
| `@arco-design/web-vue` | ^2.55.0 | UI 组件库 |
| `axios` | ^1.6.7 | HTTP 客户端 |
| `typescript` | ^5.3.3 | 类型检查 |
| `vite` | ^5.2.0 | 构建工具 |

### 开发命令

| 命令 | 作用 |
|------|------|
| `npm run dev` | 启动开发服务器（端口 5173，代理 `/api` → `localhost:5000`） |
| `npm run build` | 类型检查 + 生产构建 |
| `npm run preview` | 预览构建产物 |

---

## 二、源码目录结构 `src/`

```
src/
├── main.ts                  # 应用入口：挂载 Vue、Pinia、Router、ArcoVue
├── App.vue                  # 根组件，仅包含 <RouterView />
├── router/
│   └── index.ts             # 路由表 + 导航守卫
├── stores/
│   ├── index.ts             # store 统一导出
│   ├── user.ts              # 用户状态（登录/登出/token/角色）
│   └── app.ts               # 应用全局状态（分类/公告/侧边栏）
├── api/
│   ├── index.ts             # API 统一导出
│   ├── auth.ts              # 登录/注册/登出/认证
│   ├── user.ts              # 用户 CRUD
│   ├── category.ts          # 商品分类
│   ├── goods.ts             # 商品 CRUD
│   ├── favorite.ts          # 收藏
│   ├── bargain.ts           # 砍价
│   ├── order.ts             # 订单
│   ├── appointment.ts       # 面交预约
│   ├── chat.ts              # 聊天
│   ├── review.ts            # 评价
│   ├── report.ts            # 举报
│   └── admin.ts             # 管理后台（仪表盘/审核/公告）
├── types/
│   └── index.ts             # 全局类型定义（User/Category/Goods/Order 等）
├── utils/
│   ├── index.ts             # 工具统一导出
│   ├── request.ts           # Axios 实例封装（拦截器/token 注入/错误处理）
│   ├── errorHandler.ts      # ApiError 类 + 错误处理工具函数
│   └── mock.ts              # Mock 数据（开发/测试用）
└── views/
    ├── Login.vue            # 登录页
    ├── Register.vue         # 注册页
    ├── Home.vue             # 首页
    ├── Profile.vue          # 个人中心
    ├── StudentAuth.vue      # 学生实名认证
    ├── Favorite.vue         # 收藏列表
    ├── Bargain.vue          # 砍价列表
    ├── Chat.vue             # 聊天
    ├── Review.vue           # 评价列表
    ├── Report.vue           # 举报页面
    ├── Appointment.vue      # 面交预约
    ├── goods/
    │   ├── List.vue         # 商品广场（列表/搜索）
    │   ├── Detail.vue       # 商品详情
    │   ├── Publish.vue      # 发布商品
    │   └── Edit.vue         # 编辑商品
    ├── order/
    │   ├── List.vue         # 订单列表
    │   └── Detail.vue       # 订单详情
    └── admin/
        ├── Layout.vue       # 管理后台布局（侧边栏 + 内容区）
        ├── Dashboard.vue    # 仪表盘
        ├── UserList.vue     # 用户管理
        ├── GoodsList.vue    # 商品管理
        ├── OrderList.vue    # 订单管理
        ├── ReportList.vue   # 举报处理
        ├── NoticeList.vue   # 公告管理
        └── AuditLogs.vue    # 审核日志
```

---

## 三、核心文件详解

### 3.1 入口文件

#### `main.ts` — 应用启动

```
Vue 实例 → 注册 ArcoVue → 注册 Pinia → 注册 Router → 挂载到 #app
```

#### `App.vue` — 根组件

仅包含 `<RouterView />`，所有页面由路由决定。无全局布局壳。

---

### 3.2 路由 `router/index.ts`

| 路径 | 组件 | 权限 | 说明 |
|------|------|------|------|
| `/login` | Login.vue | 公开 | 登录页 |
| `/register` | Register.vue | 公开 | 注册页 |
| `/` | Home.vue | 公开 | 首页 |
| `/goods` | goods/List.vue | 公开 | 商品广场 |
| `/goods/:id` | goods/Detail.vue | 公开 | 商品详情 |
| `/goods/publish` | goods/Publish.vue | 需登录，seller | 发布商品 |
| `/goods/:id/edit` | goods/Edit.vue | 需登录，seller | 编辑商品 |
| `/favorites` | Favorite.vue | 需登录 | 收藏列表 |
| `/bargains` | Bargain.vue | 需登录 | 砍价列表 |
| `/orders` | order/List.vue | 需登录 | 订单列表 |
| `/orders/:id` | order/Detail.vue | 需登录 | 订单详情 |
| `/appointments` | Appointment.vue | 需登录 | 面交预约 |
| `/chat` | Chat.vue | 需登录 | 聊天 |
| `/reviews` | Review.vue | 需登录 | 评价列表 |
| `/reports` | Report.vue | 需登录 | 举报 |
| `/profile` | Profile.vue | 需登录 | 个人中心 |
| `/student-auth` | StudentAuth.vue | 需登录 | 学生认证 |
| `/admin` | admin/Layout.vue | 需登录，admin | 管理后台（含 7 个子路由） |

**导航守卫逻辑**（`beforeEach`）：

1. 已登录用户访问 `/login` 或 `/register` → 重定向 `/`
2. 未登录用户访问需认证页面 → 重定向 `/login?redirect=原路径`
3. 已登录但缺少 user 对象 → 自动调用 `fetchCurrentUser()` 补全；失败则登出
4. 角色不匹配（如 buyer 访问 `/admin`）→ 重定向 `/`

---

### 3.3 状态管理 `stores/`

#### `user.ts` — 用户 Store

| 属性/方法 | 说明 |
|-----------|------|
| `user` | `ref<User \| null>`，当前用户对象 |
| `token` | `ref<string>`，从 `localStorage` / `sessionStorage` 初始化 |
| `isLoggedIn` | `computed`，`token` 和 `user` 同时存在 |
| `isAdmin` / `isSeller` / `isBuyer` | `computed`，角色判断快捷方式 |
| `login(username, password, rememberMe)` | 调用登录 API → 存 token → 按 `rememberMe` 选择 `localStorage` 或 `sessionStorage` |
| `logout()` | 调登出 API → 清 token → 清 `localStorage` + `sessionStorage` |
| `fetchCurrentUser()` | 获取当前用户信息，路由守卫用于恢复登录态 |
| `setUser(user)` | 外部设置用户信息 |

#### `app.ts` — 应用全局 Store

| 属性/方法 | 说明 |
|-----------|------|
| `categories` | 商品分类列表 |
| `notices` | 系统公告 |
| `sidebarCollapsed` | 侧边栏折叠状态 |
| `categoryTree` | `computed`，分类列表构建成树结构 |
| `recentNotices` | `computed`，最近 5 条公告 |
| `loadCategories()` | 加载分类 |
| `loadNotices()` | 加载公告 |
| `toggleSidebar()` | 切换侧边栏 |

---

### 3.4 API 层 `api/`

所有 API 模块均通过 `request.ts` 封装的 Axios 实例发请求。

#### `request.ts` — Axios 实例封装

| 配置项 | 值 |
|--------|-----|
| `baseURL` | `/api`（Vite 代理到 `localhost:5000`） |
| `timeout` | 15000ms |
| `Content-Type` | `application/json` |

**请求拦截器**：自动从 `localStorage` 读取 `accessToken` 注入 `Authorization: Bearer <token>`

**响应拦截器**：
- `code !== 200` → 弹 `Message.error`，401 时清 token 跳 `/login`
- 网络/超时错误 → 弹通用错误提示

#### API 模块清单

| 文件 | 函数 | 接口 |
|------|------|------|
| `auth.ts` | `login`, `logout`, `register`, `getCurrentUser`, `submitStudentAuth`, `getStudentAuth`, `updateStudentAuth` | `/auth/*` |
| `user.ts` | `getUserList`, `getUserById`, `updateUser`, `banUser`, `unbanUser` | `/users/*` |
| `category.ts` | `getCategoryList`, `getCategoryTree`, `createCategory`, `updateCategory`, `deleteCategory` | `/categories/*` |
| `goods.ts` | `getGoodsList`, `getGoodsById`, `createGoods`, `updateGoods`, `deleteGoods`, `offShelfGoods`, `searchGoods` | `/goods/*` |
| `favorite.ts` | `getFavoriteList`, `addFavorite`, `removeFavorite`, `checkFavorite` | `/favorites/*` |
| `bargain.ts` | `getBargainList`, `createBargain`, `acceptBargain`, `rejectBargain` | `/bargains/*` |
| `order.ts` | `getOrderList`, `getOrderById`, `createOrder`, `cancelOrder`, `confirmOrder`, `payOrder` | `/orders/*` |
| `appointment.ts` | `getAppointmentList`, `createAppointment`, `confirmAppointment`, `cancelAppointment` | `/appointments/*` |
| `chat.ts` | `getSessionList`, `getMessages`, `sendMessage` | `/chat/*` |
| `review.ts` | `getReviewList`, `createReview` | `/reviews/*` |
| `report.ts` | `getReportList`, `createReport`, `handleReport` | `/reports/*` |
| `admin.ts` | `getDashboardStats`, `getAuditLogs`, `getNoticeList`, `createNotice`, `updateNotice`, `deleteNotice` | `/admin/*` |

#### `api/index.ts` — 统一导出入口

全部 re-export，页面组件统一 `import { xxx } from '@/api'`。

---

### 3.5 类型定义 `types/index.ts`

| 类型 | 关键字段 |
|------|---------|
| `User` | `userId`, `username`, `password`, `nickname`, `avatarUrl`, `phone`, `email`, `role`, `status`, `creditScore`, `registerTime` |
| `LoginParams` | `username`, `password` |
| `LoginResult` | `token`, `user` |
| `Category` | `categoryId`, `name`, `parentId`, `icon`, `children` |
| `Goods` | `goodsId`, `title`, `description`, `price`, `originalPrice`, `images`, `categoryId`, `sellerId`, `status`, `viewCount`, `favoriteCount`, `publishTime` |
| `GoodsImage` | `imageId`, `goodsId`, `url`, `sortOrder` |
| `Favorite` | `favoriteId`, `userId`, `goodsId`, `createTime` |
| `Bargain` | `bargainId`, `goodsId`, `buyerId`, `offerPrice`, `status`, `message`, `createTime` |
| `Order` | `orderId`, `goodsId`, `buyerId`, `sellerId`, `status`, `totalAmount`, `paymentMethod`, `createTime`, `payTime`, `completeTime` |
| `Appointment` | `appointmentId`, `orderId`, `location`, `appointTime`, `status` |
| `ChatSession` | `sessionId`, `user1Id`, `user2Id`, `goodsId`, `lastMessage`, `lastTime` |
| `ChatMessage` | `messageId`, `sessionId`, `senderId`, `content`, `sendTime` |
| `Review` | `reviewId`, `orderId`, `reviewerId`, `targetUserId`, `rating`, `content`, `type`, `createTime` |
| `Report` | `reportId`, `reporterId`, `targetType`, `targetId`, `reason`, `description`, `status`, `handleResult`, `createTime`, `handleTime` |
| `StudentAuth` | `authId`, `userId`, `studentId`, `realName`, `college`, `authStatus` |
| `Notice` | `noticeId`, `title`, `content`, `publishTime`, `publisherId` |
| `AuditLog` | `logId`, `adminId`, `action`, `targetType`, `targetId`, `detail`, `createTime` |
| `PageResult<T>` | `list: T[]`, `total: number`, `page: number`, `size: number` |

**角色枚举**：`'buyer' | 'seller' | 'admin'`  
**用户状态**：`'normal' | 'banned'`  
**商品状态**：`'on' | 'off' | 'sold'`  
**订单状态**：`'pending' | 'paid' | 'completed' | 'cancelled'`  
**认证状态**：`'pending' | 'approved' | 'rejected'`

---

### 3.6 工具函数 `utils/`

| 文件 | 说明 |
|------|------|
| `request.ts` | Axios 实例封装，请求/响应拦截器（[详见 3.4](#34-api-层-api)） |
| `errorHandler.ts` | `ApiError` 类，`handleApiError` / `handleNetworkError` / `getErrorMessage` 工具函数 |
| `mock.ts` | 全套 Mock 数据（users/categories/goods/orders 等），用于开发阶段 |
| `index.ts` | 统一导出 |

---

### 3.7 页面视图 `views/`

#### 登录模块（已完成）

| 文件 | 状态 | 功能 |
|------|------|------|
| `Login.vue` | ✅ | Arco 表单 + 校验规则 + 记住我 + 回车提交 + redirect 跳转 |
| `Register.vue` | ✅ | Arco 表单 + 校验规则 + 确认密码 + 区分错误类型 |

#### 商品模块

| 文件 | 状态 | 说明 |
|------|------|------|
| `goods/List.vue` | 待确认 | 商品广场（列表/搜索/分类筛选） |
| `goods/Detail.vue` | 待确认 | 商品详情（图片/描述/卖家信息/收藏/砍价） |
| `goods/Publish.vue` | 待确认 | 发布商品（需 seller 角色） |
| `goods/Edit.vue` | 待确认 | 编辑商品 |

#### 交易模块

| 文件 | 状态 | 说明 |
|------|------|------|
| `order/List.vue` | 待确认 | 订单列表（买/卖双视角） |
| `order/Detail.vue` | 待确认 | 订单详情 + 状态流转 |
| `Appointment.vue` | 待确认 | 面交预约 |
| `Bargain.vue` | 待确认 | 砍价记录 |

#### 社交模块

| 文件 | 状态 | 说明 |
|------|------|------|
| `Favorite.vue` | 待确认 | 收藏列表 |
| `Chat.vue` | 待确认 | 聊天（会话列表 + 消息） |
| `Review.vue` | 待确认 | 评价列表 |
| `Report.vue` | 待确认 | 举报 |

#### 用户模块

| 文件 | 状态 | 说明 |
|------|------|------|
| `Home.vue` | 待确认 | 首页 |
| `Profile.vue` | 待确认 | 个人中心（信息/头像/密码修改） |
| `StudentAuth.vue` | ⚠️ | 学生实名认证（规划中） |

#### 管理后台

| 文件 | 状态 | 说明 |
|------|------|------|
| `admin/Layout.vue` | 待确认 | 后台布局（侧边栏菜单） |
| `admin/Dashboard.vue` | 待确认 | 仪表盘/数据统计 |
| `admin/UserList.vue` | 待确认 | 用户管理 |
| `admin/GoodsList.vue` | 待确认 | 商品管理 |
| `admin/OrderList.vue` | 待确认 | 订单管理 |
| `admin/ReportList.vue` | 待确认 | 举报处理 |
| `admin/NoticeList.vue` | 待确认 | 公告管理 |
| `admin/AuditLogs.vue` | 待确认 | 审核日志 |

---

## 四、数据流总结

```
用户操作
  → 页面组件（Login.vue / Register.vue / goods/List.vue ...）
    → Pinia Store（user.ts / app.ts）或 直接调用 API 模块
      → Axios 实例（request.ts）
        → 请求拦截器：注入 Bearer token
        → 发送 HTTP 请求 → Vite 代理 → 后端 :5000
        → 响应拦截器：code≠200 弹 Message.error，401 跳 /login
      ← 返回数据
    ← 更新状态
  ← 渲染 UI（Arco Design 组件）
```

**关键约定**：
- `@/` 别名映射到 `src/`
- 页面组件不直接调 `axios`，统一通过 `@/api` 模块
- 后端返回格式：`{ code: 200, message: "success", data: ... }`
- token 存储在 `localStorage` 或 `sessionStorage`（由"记住我"控制）
- 表单校验使用 `b-validate`（Arco 内置），属性名为 `minLength` / `maxLength` / `match` / `validator`

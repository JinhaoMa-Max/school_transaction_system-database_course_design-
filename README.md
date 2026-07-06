# 校园二手交易系统

> 飞书云文档：[https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth](https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth?from=from_copylink)

## 技术栈

| 层         | 技术                                                           |
| ---------- | -------------------------------------------------------------- |
| 前端       | Vue 3 + Vite + Arco Design + Axios + Pinia                     |
| 后端       | ASP.NET Core Web API (.NET 10) + Oracle.ManagedDataAccess.Core |
| 数据库     | Oracle 23c Free (Docker: gvenzl/oracle-free)                   |
| 数据库封装 | 10 视图 + 10 函数 + 13 存储过程 + 12 触发器                    |

## 项目结构

```
campus-secondhand/
├── frontend/                               # Vue 3 前端
│   ├── src/
│   │   ├── views/                          #   20+ 页面组件
│   │   │   ├── Home.vue                    #     首页
│   │   │   ├── Login.vue / Register.vue    #     登录/注册
│   │   │   ├── goods/                      #     商品（列表/详情/发布/编辑）
│   │   │   ├── order/                      #     订单（列表/详情）
│   │   │   ├── admin/                      #     管理后台（7个子页面）
│   │   │   └── ...                         #     议价/收藏/聊天/评价/举报
│   │   ├── api/                            #   API 调用层
│   │   ├── router/                         #   路由配置
│   │   ├── stores/                         #   Pinia 状态管理
│   │   └── types/                          #   TypeScript 类型定义
│   ├── dist/                               #   构建产物
│   └── package.json
│
├── backend/
│   └── CampusSecondHand.Api/               # ASP.NET Core Web API
│       ├── Controllers/                    #   控制器
│       ├── Services/                       #   业务逻辑层
│       ├── Repositories/                   #   数据访问层
│       ├── Models/                         #   实体 + DTO
│       ├── Common/                         #   ApiResponse 统一响应
│       ├── Database/                       #   Oracle 连接工厂
│       ├── Middleware/                     #   异常处理中间件
│       └── Program.cs                      #   应用入口
│
├── database/
│   ├── ddl/
│   │   ├── 001_create_tables_docker.sql    #   15张表 + 26索引 + updated_at触发器
│   │   ├── 002_verify.sql                  #   建表验证
│   │   ├── 003_views.sql                   #   10个视图
│   │   ├── 004_functions.sql               #   10个函数
│   │   ├── 005_procedures.sql              #   13个存储过程
│   │   ├── 006_triggers.sql                #   7个业务触发器
│   │   └── 007_test.sql                    #   完整功能测试
│   ├── seed/
│   │   └── seed_data.sql                   #   种子数据（74条记录）
│   ├── init/
│   │   └── 01_create_campus_user.sql       #   Docker 首次初始化
│   └── scripts/
│       ├── docker-compose.yml              #   Oracle 容器编排
│       ├── deploy_all.bat                  #   一键部署脚本
│       └── docker_setup_windows.ps1        #   Windows Docker 配置
│
├── docs/
│   ├── 数据库设计文档/
│   │   ├── ER_diagram.md
│   │   └── 数据库接口文档.md               #   封装层接口说明 + C# 调用示例
│   ├── 需求分析文档/
│   ├── 系统设计与实现文档/
│   └── 答辩PPT/
│
└── deploy/                                 # 全栈容器编排（预留）
```

## 环境要求

| 依赖            | 版本 | 用途               |
| --------------- | ---- | ------------------ |
| Docker Desktop  | 最新 | 运行 Oracle 容器   |
| .NET SDK        | 10.0 | 运行后端 API       |
| Node.js         | 18+  | 运行前端开发服务器 |
| DBeaver（可选） | 最新 | 数据库 GUI 管理    |

## 数据库信息

| 项目     | 值                                 |
| -------- | ---------------------------------- |
| 数据库   | Oracle 23c Free                    |
| 容器名   | campus_trade_db                    |
| 服务名   | FREEPDB1                           |
| 端口     | 1521                               |
| 管理员   | SYSTEM / 123456                    |
| 应用用户 | CAMPUS / Campus123456              |
| 数据卷   | campus_trade_oracle_data（持久化） |

---

## 🚀 完整启动流程

### 第一步：启动 Oracle 数据库

```cmd
cd database/scripts
docker compose up -d
```

验证容器运行：

```cmd
docker ps --filter name=campus_trade_db
```

看到 `campus_trade_db` 且 STATUS 为 `Up` 即成功。

> **日常使用：** 数据库数据保存在 Docker 卷中，`docker compose down` 不会丢数据。
> 只有 `docker compose down -v` 会清除数据，需要重新部署。

---

### 第二步：部署数据库对象

> **首次才需要。** 日常重启只需第一步 `docker compose up -d`。

#### 方式一：批处理（一键到位）

```cmd
deploy_all.bat
```

自动依次执行：建表 → 视图 → 函数 → 存储过程 → 触发器 → 种子数据。

#### 方式二：逐条命令（精确控制）

```cmd
:: 建表（已存在则跳过）
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql

:: 封装层（CREATE OR REPLACE，可重复执行）
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\003_views.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\004_functions.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\005_procedures.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\006_triggers.sql

:: 种子数据（开头自动清旧数据，可重复跑）
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\seed\seed_data.sql
```

**什么时候用哪种：**

| 场景                  | 方式                                  |
| --------------------- | ------------------------------------- |
| 第一次部署 / 完全重建 | 批处理`deploy_all.bat`              |
| 只修改了视图          | 单独跑`003_views.sql`               |
| 想重置测试数据        | 单独跑`seed_data.sql`               |
| 日常开机              | 不需要！`docker compose up -d` 就行 |

---

### 第三步：启动后端

```cmd
cd backend/CampusSecondHand.Api
dotnet run
```

成功标志：

```
Now listening on: http://localhost:5000
Hosting environment: Development
```

验证：

| 地址                                     | 预期                 |
| ---------------------------------------- | -------------------- |
| `http://localhost:5000/swagger`        | Swagger API 文档页面 |
| `http://localhost:5000/api/health`     | `{"code":200,...}` |
| `http://localhost:5000/api/goods`      | 商品 JSON 数据       |
| `http://localhost:5000/api/categories` | 分类 JSON 数据       |

---

### 第四步：启动前端

```cmd
cd frontend
npx vite --host 0.0.0.0 --port 5173
```

浏览器打开 **`http://localhost:5173`**

| 页面     | 路由           | 说明                                     |
| -------- | -------------- | ---------------------------------------- |
| 首页     | `/`          | 公开访问                                 |
| 商品广场 | `/goods`     | **已连通后端，展示数据库真实商品** |
| 商品详情 | `/goods/:id` | 查看单件商品                             |
| 登录     | `/login`     | 登录页（后端 Auth API 待实现）           |

---

## 当前联调状态

### ✅ 已跑通（前端 ↔ 后端 ↔ 数据库）

| 功能               | 前端                                    | 后端                                                                   | 数据库                  |
| ------------------ | --------------------------------------- | ---------------------------------------------------------------------- | ----------------------- |
| **商品列表** | `GoodsList.vue` → `getGoodsList()` | `GoodsController.GetList` → `GoodsService` → `GoodsRepository` | `v_goods_list` 视图   |
| 商品详情           | `Detail.vue`                          | `GoodsController.GetById`                                            | `v_goods_detail` 视图 |
| 分类管理           | 管理后台页面                            | `CategoryController` (CRUD)                                          | `category` 表         |
| 健康检查           | —                                      | `HealthController`                                                   | `SELECT 1 FROM DUAL`  |

### 🚧 待实现

| 模块           | 说明                                     |
| -------------- | ---------------------------------------- |
| 用户认证       | 前端登录/注册页已有，后端 Auth API 待写  |
| 订单           | 前端页面已有，后端 API 待写              |
| 议价/收藏/聊天 | 前端页面已有，后端 API 待写              |
| 评价/举报      | 前端页面已有，后端 API 待写              |
| 管理后台       | 前端 7 个管理页已有，后端 Admin API 待写 |

> **后端开发参考：** 已有的 `Category + Goods` 实现是标准模板（Entity → DTO → Repository → Service → Controller），其他模块照这个模式开发即可。数据库封装层（视图/函数/存储过程）全部就绪，详见 [数据库接口文档](docs/数据库设计文档/数据库接口文档.md)。

---

## 数据库对象清单

| 类型     | 数量 | 说明                                    |
| -------- | ---- | --------------------------------------- |
| 基础表   | 15   | 用户/商品/交易/沟通/评价/管理 6 大模块  |
| 视图     | 10   | 预连表查询，供 Repository 层直接 SELECT |
| 函数     | 10   | 业务计算与权限校验，供 Service 层调用   |
| 存储过程 | 13   | 事务性复杂操作，供 Service 层调用       |
| 触发器   | 12   | 5 个 updated_at + 7 个业务规则          |
| 索引     | 26   | 覆盖所有外键和高频查询字段              |

种子数据共 74 条：8 用户 + 13 分类 + 15 商品 + 20 图片 + 10 收藏 + 5 议价 + 6 订单 + 5 面交 + 4 会话 + 10 消息 + 4 评价 + 3 举报 + 4 审核日志 + 3 公告。

---

## 表清单

| 表名          | 中文名     | 说明             |
| ------------- | ---------- | ---------------- |
| app_user      | 用户表     | 买家/卖家/管理员 |
| student_auth  | 学生认证表 | 实名认证信息     |
| category      | 商品分类表 | 支持多级分类     |
| goods         | 商品表     | 核心商品信息     |
| goods_image   | 商品图片表 | 多图片支持       |
| favorite      | 收藏表     | 用户收藏商品     |
| bargain_offer | 议价表     | 议价记录         |
| trade_order   | 订单表     | 交易订单         |
| appointment   | 面交预约表 | 线下见面安排     |
| chat_session  | 会话表     | 私聊会话         |
| chat_message  | 消息表     | 聊天内容         |
| review        | 评价表     | 交易互评         |
| report        | 举报表     | 统一举报         |
| audit_log     | 审核日志表 | 管理员操作审计   |
| notice        | 公告表     | 系统公告         |

## 三范式设计说明

- **1NF**：所有列不可再分，无重复组
- **2NF**：每张表都有明确的业务主键，非主键列完全依赖于主键
- **3NF**：非主键列不传递依赖于主键（如 goods 不存 seller_name，只存 seller_id 外键引用 app_user）

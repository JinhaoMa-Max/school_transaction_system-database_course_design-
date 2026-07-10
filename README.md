# 校园二手交易系统

> 飞书云文档：[https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth](https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth?from=from_copylink)

## 技术栈

| 层         | 技术                                                                    |
| ---------- | ----------------------------------------------------------------------- |
| 前端       | Vue 3 + Vite + Arco Design + Axios + Pinia                              |
| 后端       | ASP.NET Core Web API (.NET 10) + Dapper + Oracle.ManagedDataAccess.Core |
| 数据库     | Oracle 23c Free (Docker: gvenzl/oracle-free)                            |
| 数据库封装 | 7 视图 + 7 函数 + 13 存储过程 + 12 触发器                               |

## 项目结构

```
campus-secondhand/
├── frontend/                               # Vue 3 前端
│   ├── src/
│   │   ├── views/                          #   20+ 页面组件
│   │   │   ├── Home.vue, Login.vue, Register.vue
│   │   │   ├── goods/ (List, Detail, Publish, Edit)
│   │   │   ├── order/ (List, Detail)
│   │   │   ├── admin/ (7个子页面)
│   │   │   └── Bargain, Chat, Favorite, Review, Report, Profile, StudentAuth
│   │   ├── api/                            #   前端 API 模块 (13个)
│   │   └── stores/, router/, types/, utils/
│   └── vite.config.ts
│
├── backend/CampusTrade/CampusTrade.Backend/ # ASP.NET Core Web API
│   ├── Controllers/ (12个)
│   ├── Services/ (12个模块)
│   ├── Repositories/ (11个, 全部实现)
│   ├── Models/DTOs/ (14个)
│   ├── Infrastructure/
│   └── Program.cs
│
├── database/
│   ├── ddl/
│   │   ├── 001_create_tables_docker.sql    #   15表 + 26索引 + 5 updated_at触发器
│   │   ├── 002_verify.sql
│   │   ├── 003_views.sql                   #   7个视图
│   │   ├── 004_functions.sql               #   7个函数
│   │   ├── 005_procedures.sql              #   13个存储过程
│   │   ├── 006_triggers.sql                #   7个业务触发器
│   │   ├── 007_test.sql
│   │   └── 008_data_migration.sql          #   旧数据迁移脚本
│   ├── seed/seed_data.sql                  #   种子数据
│   └── scripts/ (docker-compose.yml, deploy_all.bat)
│
└── docs/
    ├── 数据库设计文档/ (ER图, 接口文档[含3NF验证])
    ├── 开发进度.md, 功能实现清单.md, 前端API规范.md
    └── 答辩PPT/
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

## 启动流程

### 第一步：启动 Oracle

```cmd
cd database/scripts
docker compose up -d
```

### 第二步：部署数据库对象（首次）

```cmd
deploy_all.bat                           # 一键全量部署
:: 或逐条：
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\003_views.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\004_functions.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\005_procedures.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\006_triggers.sql
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\seed\seed_data.sql
```

### 第三步：启动后端

```cmd
cd backend/CampusTrade/CampusTrade.Backend
dotnet run
```

成功标志：`Now listening on: http://localhost:5000`

### 第四步：启动前端

```cmd
cd frontend
npx vite --host 0.0.0.0 --port 5173
```

浏览器打开 `http://localhost:5173`

### DBeaver 连接

| 参数              | 值                               |
| ----------------- | -------------------------------- |
| Host/Port         | `localhost:1521`               |
| Service           | `FREEPDB1`                     |
| Username/Password | `CAMPUS` / `Campus123456`    |
| Driver Property   | `oracle.net.disableOob = true` |

---

## 当前状态

### 整体进度

| 层                      | 进度           | 说明                                                                                            |
| ----------------------- | -------------- | ----------------------------------------------------------------------------------------------- |
| 数据库                  | **100%** | 15表 + 7视图 + 7函数 + 13存储过程 + 12触发器，全部3NF                                           |
| 后端 Repository         | **100%** | 11个Repository，0个空壳，全部调用数据库封装对象                                                 |
| 后端 Service+Controller | **~80%** | Auth/Goods/Category/Bargain/Order/Favorite/Chat/Review/Report/Appointment/Admin/Upload 全部就绪 |
| 前端                    | **~70%** | 22个功能点页面齐全，部分交互待完善                                                              |

### ✅ 全部跑通（22个功能点）

详见 [功能实现清单.md](docs/功能实现清单.md)

### 测试账号

| 用户                                     | 密码    | 角色     |
| ---------------------------------------- | ------- | -------- |
| admin                                    | pass123 | 管理员   |
| zhangsan                                 | pass123 | 普通用户 |
| lisi / wangwu / zhaoliu / sunqi / zhouba | pass123 | 普通用户 |

---

## 数据库对象清单

| 类型     | 数量 | 说明                                                                                                                                                                                                                                                   |
| -------- | ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 基础表   | 15   | 全部满足 3NF，见[接口文档](docs/数据库设计文档/数据库接口文档.md) 第十章                                                                                                                                                                                |
| 视图     | 7    | v_goods_list, v_goods_detail, v_order_list, v_category_tree, v_unread_messages, v_review_detail, v_active_bargains                                                                                                                                     |
| 函数     | 7    | fn_can_purchase, fn_calc_credit, fn_avg_rating, fn_unread_count, fn_gen_confirm_code, fn_favorite_goods_ids, fn_increment_view                                                                                                                         |
| 存储过程 | 13   | sp_place_order, sp_cancel_order, sp_complete_meet, sp_update_credit, sp_create_bargain, sp_respond_bargain, sp_buyer_handle_bargain, sp_create_review, sp_audit_goods, sp_handle_report, sp_manage_user_ban, sp_send_message, sp_get_or_create_session |
| 触发器   | 12   | 5 updated_at + 7 业务规则                                                                                                                                                                                                                              |
| 索引     | 26   | 覆盖外键和高频查询                                                                                                                                                                                                                                     |

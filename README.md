# 校园二手交易系统 - 数据库设计

> 飞书云文档：[https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth](https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth?from=from_copylink)

## 项目结构

```
campus-secondhand/
├── frontend/
│   └── campus-secondhand-web/          # Vue 3 前端项目
│
├── backend/
│   └── CampusSecondHand.Api/           # ASP.NET Core Web API 后端项目
│
├── database/
│   ├── ddl/                            # 建表 SQL
│   │   ├── 001_create_tables.sql       #   原始建表脚本（含 CONNECT 语句）
│   │   ├── 001_create_tables_docker.sql #   Docker 环境建表脚本
│   │   └── 002_verify.sql              #   建表后验证脚本
│   ├── seed/                           # 初始化数据 SQL
│   ├── migration/                      # 后续变更 SQL
│   ├── init/                           # Docker 首次启动初始化
│   │   └── 01_create_campus_user.sql   #   补充 CAMPUS 用户权限
│   └── scripts/                        # 运维脚本
│       ├── docker-compose.yml          #   Oracle 容器一键编排
│       ├── docker_setup_windows.ps1    #   Windows 环境自动配置
│       └── run_sql.bat                 #   一键建表/验证菜单
│
├── docs/
│   ├── 需求分析文档/
│   ├── 数据库设计文档/
│   │   └── ER_diagram.md               # ER 图及模块关系说明
│   ├── 系统设计与实现文档/
│   └── 答辩PPT/
│
├── deploy/
│   ├── nginx.conf
│   ├── docker-compose.yml              # 全栈容器编排
│   └── appsettings.Production.json
│
└── README.md                           # 本文件
```

## 数据库信息

| 项目 | 值 |
|------|-----|
| 数据库 | Oracle 23c Free (推荐) / 18c XE |
| 服务名 (23c) | FREEPDB1 |
| 服务名 (18c) | XEPDB1 |
| 端口 | 1521 |
| 管理员用户 | SYSTEM / 123456 |
| 应用用户 | CAMPUS / Campus123456 |

## 快速开始（Docker 方式）

### 1. 启动 Oracle 容器

```powershell
# 进入 database/scripts 目录
cd database/scripts

# 启动容器（首次会拉取镜像）
docker compose up -d

# 查看启动日志
docker compose logs -f
```

看到 `DATABASE IS READY TO USE!` 即启动完成。

### 2. 执行建表脚本

```powershell
# 方式一：CMD 下使用 run_sql.bat
run_sql.bat
# 选择 [1] 创建数据库表

# 方式二：PowerShell 下直接 pipe
Get-Content ..\ddl\001_create_tables_docker.sql | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1

# 方式三：cmd 下使用重定向
docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1 < ..\ddl\001_create_tables_docker.sql
```

### 3. 验证

```powershell
Get-Content ..\ddl\002_verify.sql | docker exec -i campus_trade_db sqlplus CAMPUS/Campus123456@FREEPDB1
```

## 实体关系概览

共 15 张表，分为 6 大模块：

```
用户模块:     app_user ── student_auth
商品模块:     category ── goods ── goods_image ── favorite
交易模块:     bargain_offer ── trade_order ── appointment
沟通模块:     chat_session ── chat_message
评价模块:     review ── report
管理模块:     audit_log ── notice
```

## 表清单

| 表名 | 中文名 | 说明 |
|------|--------|------|
| app_user | 用户表 | 买家/卖家/管理员 |
| student_auth | 学生认证表 | 实名认证信息 |
| category | 商品分类表 | 支持多级分类 |
| goods | 商品表 | 核心商品信息 |
| goods_image | 商品图片表 | 多图片支持 |
| favorite | 收藏表 | 用户收藏商品 |
| bargain_offer | 议价表 | 议价记录 |
| trade_order | 订单表 | 交易订单 |
| appointment | 面交预约表 | 线下见面安排 |
| chat_session | 会话表 | 私聊会话 |
| chat_message | 消息表 | 聊天内容 |
| review | 评价表 | 交易互评 |
| report | 举报表 | 统一举报 |
| audit_log | 审核日志表 | 管理员操作审计 |
| notice | 公告表 | 系统公告 |

## 三范式设计说明

每张表均满足 3NF：

- **1NF**：所有列不可再分，无重复组
- **2NF**：每张表都有明确的业务主键，非主键列完全依赖于主键
- **3NF**：非主键列不传递依赖于主键（如 goods 不存 seller_name，只存 seller_id 外键引用 app_user）

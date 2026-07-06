飞书云文档地址：https://bcnd2uarfv5w.feishu.cn/docx/LJAhdR36doDiimxBMRhcMAK0nth?from=from_copylink

项目文件结构：
campus-secondhand
│
├── frontend
│   └── campus-secondhand-web        # Vue 3 前端项目
│
├── backend
│   └── CampusSecondHand.Api         # ASP.NET Core Web API 后端项目
│
├── database
│   ├── ddl                          # 建表 SQL
│   ├── seed                         # 初始化数据 SQL
│   └── migration                    # 后续变更 SQL
│
├── docs
│   ├── 需求分析文档
│   ├── 数据库设计文档
│   ├── 系统设计与实现文档
│   └── 答辩PPT
│
├── deploy
│   ├── nginx.conf
│   ├── docker-compose.yml
│   └── appsettings.Production.json
│
└── README.md
# CampusSecondHand.Api

ASP.NET Core Web API backend for the campus second-hand trading system. This module currently implements the base five-layer structure plus `category` CRUD as the template for later modules.

## Local run

Run Oracle first, then start the API from the host:

```bash
docker compose up -d oracle
cd backend/CampusSecondHand.Api
dotnet restore
dotnet run
```

When the API runs on the host, `appsettings.json` uses:

```text
User Id=campus_user;Password=Campus123456;Data Source=localhost:1521/FREEPDB1;
```

## Docker Compose run

```bash
docker compose up -d oracle api
```

Inside Docker Compose, the API connects to Oracle through the compose service name:

```text
User Id=campus_user;Password=Campus123456;Data Source=oracle:1521/FREEPDB1;
```

## Swagger

```text
http://localhost:5000/swagger
```

## Health checks

```bash
curl http://localhost:5000/api/health
curl http://localhost:5000/api/health/db
```

## Category API

```bash
curl http://localhost:5000/api/categories
curl http://localhost:5000/api/categories/1
```

Create a category:

```bash
curl -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -d "{\"categoryName\":\"测试分类\",\"parentId\":null,\"sortOrder\":99}"
```

Update a category:

```bash
curl -X PUT http://localhost:5000/api/categories/1 \
  -H "Content-Type: application/json" \
  -d "{\"categoryName\":\"电子产品\",\"parentId\":null,\"sortOrder\":20}"
```

Delete a category:

```bash
curl -X DELETE http://localhost:5000/api/categories/1
```

## Vue configuration

```text
VITE_API_BASE_URL=http://localhost:5000/api
```

## Notes

- SQL is kept in `Repositories`.
- Business validation is kept in `Services`.
- Controllers only receive HTTP requests, call services, and return `ApiResponse`.
- The current schema is expected to contain `category(category_id, category_name, parent_id, sort_order, created_at)` and `goods.category_id`.

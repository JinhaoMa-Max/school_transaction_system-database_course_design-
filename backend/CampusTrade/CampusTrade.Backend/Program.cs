using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Repositories;
using CampusTrade.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IDbConnectionFactory, OracleConnectionFactory>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IGoodsRepository, GoodsRepository>();
builder.Services.AddScoped<IGoodsService, GoodsService>();

var app = builder.Build();

app.MapControllers();

app.Run();
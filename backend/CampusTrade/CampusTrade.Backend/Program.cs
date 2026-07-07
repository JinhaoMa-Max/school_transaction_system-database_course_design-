using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Repositories;
using CampusTrade.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IDbConnectionFactory, OracleConnectionFactory>();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
// 登记你的用户仓储和登录服务
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.MapControllers();

app.Run();
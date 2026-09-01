using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Repositories;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory, OracleConnectionFactory>();
builder.Services.AddSingleton<ITokenService, SignedTokenService>();
builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();

builder.Services
    .AddAuthentication(SignedTokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SignedTokenAuthenticationHandler>(
        SignedTokenAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddAuthorization();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoodsRepository, GoodsRepository>();
builder.Services.AddScoped<IGoodsService, GoodsService>();
builder.Services.AddScoped<IBargainRepository, BargainRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IBargainService, BargainService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Establish the Oracle pool before accepting requests. Oracle Free in Docker can
// occasionally reject the first pooled connection while the PDB is waking up;
// absorbing that transient failure here prevents the first user action becoming a 500.
var databaseReady = false;
Exception? databaseError = null;
for (var attempt = 1; attempt <= 3; attempt++)
{
    try
    {
        using var connection = app.Services.GetRequiredService<IDbConnectionFactory>().CreateConnection();
        connection.Open();
        app.Logger.LogInformation("Oracle database connection is ready.");
        databaseReady = true;
        break;
    }
    catch (Exception ex)
    {
        databaseError = ex;
        app.Logger.LogWarning(ex, "Oracle warm-up attempt {Attempt} failed.", attempt);
        if (attempt < 3)
        {
            await Task.Delay(TimeSpan.FromSeconds(attempt));
        }
    }
}

if (!databaseReady)
{
    throw new InvalidOperationException("Oracle database is unavailable after three attempts.", databaseError);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
        if (feature?.Error != null)
        {
            logger.LogError(feature.Error, "Unhandled request failure for {Path}", context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(500, "internal server error"));
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

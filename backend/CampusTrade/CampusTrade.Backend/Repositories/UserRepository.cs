using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public interface IUserRepository
{
    // === 百分百保留你原有的 10 个核心接口 ===
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByStudentIdAsync(string studentId);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> StudentIdExistsAsync(string studentId, int? excludeUserId = null);
    Task<User> CreateUserWithStudentBindingAsync(RegisterRequestDto request, string hashedPassword);
    Task<StudentAuthDto?> GetStudentAuthByUserIdAsync(int userId);
    Task<StudentAuthDto?> GetStudentAuthByAuthIdAsync(int authId);
    Task<StudentAuthDto> UpsertStudentAuthAsync(StudentAuthRequestDto request);
    Task<StudentAuthDto?> UpdateStudentAuthAsync(int authId, StudentAuthRequestDto request);

    // === ✨ 安全增量：为了适配 /api/users 后台管理而补充的 5 个新接口 ===
    Task<PageResult<UserDto>> GetPagedUsersAsync(int page, int size, string? role);
    Task<User> UpdateUserFieldsAsync(int userId, PartialUserUpdateRequest request);
    Task<bool> DeleteUserAsync(int userId);
    Task<bool> UpdateStatusAsync(int userId, string status);
    Task<bool> UpdateCreditScoreAsync(int userId, int score);
}

public class UserRepository : IUserRepository
{
    // 百分百保留你定义的标准字段映射规范
    private const string UserSelectColumns = """
        user_id AS UserId,
        username AS Username,
        password AS Password,
        nickname AS Nickname,
        avatar AS AvatarUrl,
        phone AS Phone,
        email AS Email,
        role AS Role,
        status AS Status,
        credit_score AS CreditScore,
        created_at AS RegisterTime
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // ==================== 1. 百分百保留你原有的全部业务实现 ====================

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT {UserSelectColumns}
            FROM app_user
            WHERE LOWER(username) = LOWER(:Username)
            FETCH FIRST 1 ROWS ONLY
            """;

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByStudentIdAsync(string studentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"""
            SELECT
                u.user_id AS UserId,
                u.username AS Username,
                u.password AS Password,
                u.nickname AS Nickname,
                u.avatar AS AvatarUrl,
                u.phone AS Phone,
                u.email AS Email,
                u.role AS Role,
                u.status AS Status,
                u.credit_score AS CreditScore,
                u.created_at AS RegisterTime
            FROM app_user u
            JOIN student_auth sa ON sa.user_id = u.user_id
            WHERE sa.student_id = :StudentId
            FETCH FIRST 1 ROWS ONLY
            """;

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { StudentId = studentId });
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await GetByIdAsync(connection, userId);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM app_user WHERE LOWER(username) = LOWER(:Username)";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task<bool> StudentIdExistsAsync(string studentId, int? excludeUserId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT COUNT(1)
            FROM student_auth
            WHERE student_id = :StudentId
              AND (:ExcludeUserId IS NULL OR user_id <> :ExcludeUserId)
            """;
        var count = await connection.ExecuteScalarAsync<int>(sql, new { StudentId = studentId, ExcludeUserId = excludeUserId });
        return count > 0;
    }

    public async Task<User> CreateUserWithStudentBindingAsync(RegisterRequestDto request, string hashedPassword)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string insertUserSql = """
                INSERT INTO app_user (username, password, nickname, phone, email)
                VALUES (:Username, :Password, :Nickname, :Phone, :Email)
                """;

            await connection.ExecuteAsync(insertUserSql, new
            {
                request.Username,
                Password = hashedPassword,
                Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? "新用户" : request.Nickname.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim()
            }, transaction);

            const string selectUserIdSql = "SELECT user_id FROM app_user WHERE username = :Username";
            var userId = await connection.ExecuteScalarAsync<int>(selectUserIdSql, new { request.Username }, transaction);

            const string insertStudentAuthSql = """
                INSERT INTO student_auth (user_id, student_id, real_name, college, auth_status)
                VALUES (:UserId, :StudentId, :RealName, :College, 'pending')
                """;

            await connection.ExecuteAsync(insertStudentAuthSql, new
            {
                UserId = userId,
                StudentId = request.StudentId.Trim(),
                RealName = "待完善",
                College = "待完善"
            }, transaction);

            var user = await GetByIdAsync(connection, userId, transaction)
                ?? throw new InvalidOperationException("注册后未能读取用户信息");

            transaction.Commit();
            return user;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<StudentAuthDto?> GetStudentAuthByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT
                auth_id AS AuthId,
                user_id AS UserId,
                student_id AS StudentId,
                real_name AS RealName,
                college AS College,
                auth_status AS AuthStatus,
                auth_time AS AuthTime
            FROM student_auth
            WHERE user_id = :UserId
            FETCH FIRST 1 ROWS ONLY
            """;

        return await connection.QuerySingleOrDefaultAsync<StudentAuthDto>(sql, new { UserId = userId });
    }

    public async Task<StudentAuthDto?> GetStudentAuthByAuthIdAsync(int authId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            SELECT
                auth_id AS AuthId,
                user_id AS UserId,
                student_id AS StudentId,
                real_name AS RealName,
                college AS College,
                auth_status AS AuthStatus,
                auth_time AS AuthTime
            FROM student_auth
            WHERE auth_id = :AuthId
            """;

        return await connection.QuerySingleOrDefaultAsync<StudentAuthDto>(sql, new { AuthId = authId });
    }

    public async Task<StudentAuthDto> UpsertStudentAuthAsync(StudentAuthRequestDto request)
    {
        if (!request.UserId.HasValue)
        {
            throw new ArgumentException("缺少用户ID");
        }

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string countSql = "SELECT COUNT(1) FROM student_auth WHERE user_id = :UserId";
            var exists = await connection.ExecuteScalarAsync<int>(countSql, new { request.UserId }, transaction) > 0;

            if (exists)
            {
                const string updateSql = """
                    UPDATE student_auth
                    SET student_id = :StudentId,
                        real_name = :RealName,
                        college = :College,
                        auth_status = 'pending',
                        auth_time = NULL
                    WHERE user_id = :UserId
                    """;
                await connection.ExecuteAsync(updateSql, new
                {
                    request.UserId,
                    StudentId = request.StudentId,
                    RealName = request.RealName,
                    College = request.College
                }, transaction);
            }
            else
            {
                const string insertSql = """
                    INSERT INTO student_auth (user_id, student_id, real_name, college, auth_status)
                    VALUES (:UserId, :StudentId, :RealName, :College, 'pending')
                    """;
                await connection.ExecuteAsync(insertSql, new
                {
                    request.UserId,
                    StudentId = request.StudentId,
                    RealName = request.RealName,
                    College = request.College
                }, transaction);
            }

            const string selectSql = """
                SELECT
                    auth_id AS AuthId,
                    user_id AS UserId,
                    student_id AS StudentId,
                    real_name AS RealName,
                    college AS College,
                    auth_status AS AuthStatus,
                    auth_time AS AuthTime
                FROM student_auth
                WHERE user_id = :UserId
                """;
            var result = await connection.QuerySingleAsync<StudentAuthDto>(selectSql, new { request.UserId }, transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<StudentAuthDto?> UpdateStudentAuthAsync(int authId, StudentAuthRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE student_auth
            SET student_id = COALESCE(:StudentId, student_id),
                real_name = COALESCE(:RealName, real_name),
                college = COALESCE(:College, college),
                auth_status = COALESCE(:AuthStatus, auth_status),
                auth_time = CASE WHEN :AuthStatus = 'approved' THEN SYSTIMESTAMP ELSE auth_time END
            WHERE auth_id = :AuthId
            """;

        var affected = await connection.ExecuteAsync(sql, new
        {
            AuthId = authId,
            request.StudentId,
            request.RealName,
            request.College,
            request.AuthStatus
        });

        return affected == 0 ? null : await GetStudentAuthByAuthIdAsync(authId);
    }

    private async Task<User?> GetByIdAsync(System.Data.IDbConnection connection, int userId, System.Data.IDbTransaction? transaction = null)
    {
        var sql = $"""
            SELECT {UserSelectColumns}
            FROM app_user
            WHERE user_id = :UserId
            """;

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId }, transaction);
    }


    // ==================== 2. 🚀 安全增量：严格对齐 app_user 结构的后台管理实现 ====================

    /// <summary>后台管理：用户列表分页查询 — 完美适配 app_user 表与现代 Oracle 分页</summary>
    public async Task<PageResult<UserDto>> GetPagedUsersAsync(int page, int size, string? role)
    {
        using var connection = _connectionFactory.CreateConnection();
        var offset = (page - 1) * size;

        var whereClause = " WHERE 1=1 ";
        var parameters = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(role))
        {
            whereClause += " AND role = :Role ";
            parameters.Add(":Role", role);
        }

        // 查总数
        var countSql = $"SELECT COUNT(1) FROM app_user {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // 查数据（使用你的 UserSelectColumns 保证属性完美填入 UserDto 内部）
        var sql = $"""
            SELECT {UserSelectColumns}
            FROM app_user
            {whereClause}
            ORDER BY user_id DESC
            OFFSET {offset} ROWS FETCH NEXT {size} ROWS ONLY
            """;

        var items = await connection.QueryAsync<UserDto>(sql, parameters);

        return new PageResult<UserDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            Size = size
        };
    }

    /// <summary>后台管理：动态增量更新用户信息字段（对齐 app_user 真实列名）</summary>
    public async Task<User> UpdateUserFieldsAsync(int userId, PartialUserUpdateRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var updates = new List<string>();
        var p = new DynamicParameters();
        p.Add(":UserId", userId);

        // 核心修正：avatar 对应数据库里的真实列名为 avatar
        if (request.Nickname != null) { updates.Add("nickname = :Nickname"); p.Add(":Nickname", request.Nickname); }
        if (request.AvatarUrl != null) { updates.Add("avatar = :AvatarUrl"); p.Add(":AvatarUrl", request.AvatarUrl); }
        if (request.Phone != null) { updates.Add("phone = :Phone"); p.Add(":Phone", request.Phone); }
        if (request.Email != null) { updates.Add("email = :Email"); p.Add(":Email", request.Email); }

        if (updates.Count > 0)
        {
            var sql = $"UPDATE app_user SET {string.Join(", ", updates)} WHERE user_id = :UserId";
            await connection.ExecuteAsync(sql, p);
        }

        return (await GetByIdAsync(userId))!;
    }

    /// <summary>后台管理：物理删除指定用户</summary>
    public async Task<bool> DeleteUserAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync("DELETE FROM app_user WHERE user_id = :UserId", new { UserId = userId }) > 0;
    }

    /// <summary>后台管理：更改封禁状态值（对齐 app_user）</summary>
    public async Task<bool> UpdateStatusAsync(int userId, string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE app_user SET status = :Status WHERE user_id = :UserId";
        return await connection.ExecuteAsync(sql, new { Status = status, UserId = userId }) > 0;
    }

    /// <summary>后台管理：调整信用评分（对齐 app_user）</summary>
    public async Task<bool> UpdateCreditScoreAsync(int userId, int score)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE app_user SET credit_score = :Score WHERE user_id = :UserId";
        return await connection.ExecuteAsync(sql, new { Score = score, UserId = userId }) > 0;
    }
}
using CampusTrade.Backend.Infrastructure;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using Dapper;

namespace CampusTrade.Backend.Repositories;

public interface IUserRepository
{
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
    Task<bool> UpdateAvatarAsync(int userId, string avatarUrl);
}

public class UserRepository : IUserRepository
{
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

    public async Task<bool> UpdateAvatarAsync(int userId, string avatarUrl)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = """
            UPDATE app_user
            SET avatar = :AvatarUrl, updated_at = SYSDATE
            WHERE user_id = :UserId
            """;
        var affected = await connection.ExecuteAsync(sql, new { UserId = userId, AvatarUrl = avatarUrl });
        return affected > 0;
    }
}

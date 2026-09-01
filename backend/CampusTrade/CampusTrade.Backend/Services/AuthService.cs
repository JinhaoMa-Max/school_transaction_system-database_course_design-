using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class AuthService : IAuthService
{
    private static readonly Regex PureNumberPattern = new("^\\d+$", RegexOptions.Compiled);
    private static readonly Regex StudentIdPattern = new("^\\d{6,20}$", RegexOptions.Compiled);
    private static readonly Regex UsernamePattern = new("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);
    private static readonly Regex PasswordPattern = new("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*])[A-Za-z0-9!@#$%^&*]{6,20}$", RegexOptions.Compiled);
    private static readonly Regex PhonePattern = new("^1[3-9]\\d{9}$", RegexOptions.Compiled);
    private static readonly HashSet<string> StudentAuthStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "approved",
        "rejected"
    };

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var account = request.Account?.Trim();
        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new AuthException(400, "缺少必要参数");
        }

        var user = await _userRepository.GetByUsernameAsync(account)
            ?? await _userRepository.GetByStudentIdAsync(account);

        if (user == null || !_passwordHasher.Verify(request.Password, user.Password))
        {
            throw new AuthException(401, "账号或密码错误");
        }

        if (string.Equals(user.Status, "banned", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthException(403, "账号已被封禁");
        }

        return new AuthResponseDto
        {
            Token = _tokenService.GenerateToken(user.UserId),
            User = ToDto(user)
        };
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request)
    {
        ValidateRegisterRequest(request);

        var username = request.Username.Trim();
        var studentId = request.StudentId.Trim();

        if (await _userRepository.UsernameExistsAsync(username))
        {
            throw new AuthException(409, "用户名已存在");
        }

        if (await _userRepository.StudentIdExistsAsync(studentId))
        {
            throw new AuthException(409, "学号已存在");
        }

        request.Username = username;
        request.StudentId = studentId;
        request.Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim();
        request.Phone = request.Phone.Trim();
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        var user = await _userRepository.CreateUserWithStudentBindingAsync(request, _passwordHasher.Hash(request.Password));
        return ToDto(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(string? token)
    {
        var userId = TryGetUserIdFromToken(token);
        if (!userId.HasValue)
        {
            throw new AuthException(401, "未登录或登录已过期");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            throw new AuthException(401, "未登录或登录已过期");
        }

        if (string.Equals(user.Status, "banned", StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthException(403, "账号已被封禁");
        }

        return ToDto(user);
    }

    public int? TryGetUserIdFromToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return _tokenService.TryValidateToken(token, out var userId) ? userId : null;
    }

    public async Task<StudentAuthDto?> GetStudentAuthByUserIdAsync(int userId)
    {
        return await _userRepository.GetStudentAuthByUserIdAsync(userId);
    }

    public async Task<PageResult<StudentAuthAdminDto>> GetPagedStudentAuthAsync(int page, int size, string? status)
    {
        if (!string.IsNullOrWhiteSpace(status) && !StudentAuthStatuses.Contains(status.Trim()))
        {
            throw new AuthException(400, "认证状态不合法");
        }

        return await _userRepository.GetPagedStudentAuthAsync(page, size, status);
    }

    public async Task<StudentAuthDto> SubmitStudentAuthAsync(StudentAuthRequestDto request, int? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            throw new AuthException(401, "login required");
        }

        // Never trust a user id supplied by the client for a self-service submission.
        request.UserId = currentUserId.Value;
        ValidateStudentAuthRequest(request, requireUserId: true);

        var user = await _userRepository.GetByIdAsync(request.UserId!.Value);
        if (user == null)
        {
            throw new AuthException(404, "用户不存在");
        }

        if (await _userRepository.StudentIdExistsAsync(request.StudentId!.Trim(), request.UserId.Value))
        {
            throw new AuthException(409, "学号已存在");
        }

        request.StudentId = request.StudentId.Trim();
        request.RealName = request.RealName!.Trim();
        request.College = request.College!.Trim();

        return await _userRepository.UpsertStudentAuthAsync(request);
    }

    public async Task<StudentAuthDto> UpdateStudentAuthAsync(int authId, StudentAuthRequestDto request)
    {
        var existing = await _userRepository.GetStudentAuthByAuthIdAsync(authId);
        if (existing == null)
        {
            throw new AuthException(404, "认证记录不存在");
        }

        if (!string.IsNullOrWhiteSpace(request.StudentId)
        && await _userRepository.StudentIdExistsAsync(request.StudentId.Trim(), existing.UserId))
        {
            throw new AuthException(409, "学号已存在");
        }

        if (!string.IsNullOrWhiteSpace(request.AuthStatus)
        && !StudentAuthStatuses.Contains(request.AuthStatus.Trim()))
        {
            throw new AuthException(400, "认证状态不合法");
        }

        request.StudentId = string.IsNullOrWhiteSpace(request.StudentId) ? null : request.StudentId.Trim();
        request.RealName = string.IsNullOrWhiteSpace(request.RealName) ? null : request.RealName.Trim();
        request.College = string.IsNullOrWhiteSpace(request.College) ? null : request.College.Trim();
        request.AuthStatus = string.IsNullOrWhiteSpace(request.AuthStatus) ? null : request.AuthStatus.Trim();

        return await _userRepository.UpdateStudentAuthAsync(authId, request)
            ?? throw new AuthException(404, "认证记录不存在");
    }

    private static void ValidateRegisterRequest(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.StudentId)
        || string.IsNullOrWhiteSpace(request.Username)
        || string.IsNullOrWhiteSpace(request.Password)
        || string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new AuthException(400, "缺少必要参数");
        }

        if (PureNumberPattern.IsMatch(request.Username.Trim()))
        {
            throw new AuthException(400, "用户名不能为纯数字");
        }

        if (!StudentIdPattern.IsMatch(request.StudentId.Trim()))
            throw new AuthException(400, "学号必须是 6-20 位数字");
        if (!UsernamePattern.IsMatch(request.Username.Trim()))
            throw new AuthException(400, "用户名只能包含 3-20 位字母、数字或下划线");
        if (!PasswordPattern.IsMatch(request.Password))
            throw new AuthException(400, "密码需包含大小写字母、数字和特殊字符，长度 6-20 位");
        if (!PhonePattern.IsMatch(request.Phone.Trim()))
            throw new AuthException(400, "手机号格式不正确");
        if (!string.IsNullOrWhiteSpace(request.Nickname)
            && Encoding.UTF8.GetByteCount(request.Nickname.Trim()) > 50)
            throw new AuthException(400, "昵称过长");
        if (!string.IsNullOrWhiteSpace(request.Email)
            && (request.Email.Trim().Length > 100 || !MailAddress.TryCreate(request.Email.Trim(), out _)))
            throw new AuthException(400, "邮箱格式不正确");
    }

    private static void ValidateStudentAuthRequest(StudentAuthRequestDto request, bool requireUserId)
    {
        if (requireUserId && !request.UserId.HasValue)
        {
            throw new AuthException(400, "缺少用户ID");
        }

        if (string.IsNullOrWhiteSpace(request.StudentId)
        || string.IsNullOrWhiteSpace(request.RealName)
        || string.IsNullOrWhiteSpace(request.College))
        {
            throw new AuthException(400, "缺少必要参数");
        }

        if (!StudentIdPattern.IsMatch(request.StudentId.Trim()))
            throw new AuthException(400, "学号必须是 6-20 位数字");
        if (Encoding.UTF8.GetByteCount(request.RealName!.Trim()) > 50)
            throw new AuthException(400, "真实姓名过长");
        if (Encoding.UTF8.GetByteCount(request.College!.Trim()) > 100)
            throw new AuthException(400, "学院名称过长");
    }

    public async Task<UserDto> UpdateAvatarAsync(string? token, string avatarUrl)
    {
        var userId = TryGetUserIdFromToken(token);
        if (!userId.HasValue)
        {
            throw new AuthException(401, "未登录或登录已过期");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            throw new AuthException(404, "用户不存在");
        }

        await _userRepository.UpdateAvatarAsync(userId.Value, avatarUrl);

        var updatedUser = await _userRepository.GetByIdAsync(userId.Value);
        if (updatedUser == null)
        {
            throw new InvalidOperationException("更新头像后未能读取用户信息");
        }

        return ToDto(updatedUser);
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Password = string.Empty,
            Nickname = string.IsNullOrWhiteSpace(user.Nickname) ? user.Username : user.Nickname,
            AvatarUrl = user.AvatarUrl ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            Status = user.Status,
            CreditScore = user.CreditScore,
            RegisterTime = user.RegisterTime
        };
    }
}


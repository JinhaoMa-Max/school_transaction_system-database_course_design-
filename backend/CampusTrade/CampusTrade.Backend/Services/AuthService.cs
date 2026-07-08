using System.Text.RegularExpressions;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class AuthService : IAuthService
{
    private static readonly Regex PureNumberPattern = new("^\\d+$", RegexOptions.Compiled);
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
        var auth = await _userRepository.GetStudentAuthByUserIdAsync(userId);
        return IsRegistrationPlaceholder(auth) ? null : auth;
    }

    public async Task<StudentAuthDto> SubmitStudentAuthAsync(StudentAuthRequestDto request, int? currentUserId)
    {
        request.UserId = currentUserId ?? request.UserId;
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
    }

    private static bool IsRegistrationPlaceholder(StudentAuthDto? auth)
    {
        return auth != null
            && string.Equals(auth.RealName, "待完善", StringComparison.Ordinal)
            && string.Equals(auth.College, "待完善", StringComparison.Ordinal);
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


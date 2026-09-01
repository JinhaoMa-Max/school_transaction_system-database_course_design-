using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;
using System.Net.Mail;
using System.Text;

namespace CampusTrade.Backend.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PageResult<UserDto>> GetPagedUsersAsync(int page, int size, string? role)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);
        return await _userRepository.GetPagedUsersAsync(page, size, role);
    }

    public async Task<UserDto?> GetByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        // 复用你们已有的数据对象包装逻辑
        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(int userId, PartialUserUpdateRequest request, int? operatorId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("用户不存在");

        // 核心安全审计：检查操作者是否为本人或者是管理员
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (operatorId != userId && currentUser?.Role != "admin")
        {
            throw new UnauthorizedAccessException("您没有权限修改该用户的信息");
        }

        ValidateUpdate(request);

        // 调用底层 UserRepository 将前端传来的增量字段更新进 Oracle 数据库
        var updated = await _userRepository.UpdateUserFieldsAsync(userId, request);
        return ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int userId, int? operatorId)
    {
        // 鉴权：只有管理员可以执行物理/逻辑删除
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("只有管理员能删除用户");
        if (operatorId == userId) throw new InvalidOperationException("管理员不能删除自己");

        return await _userRepository.DeleteUserAsync(userId);
    }

    public async Task<bool> UpdateStatusAsync(int userId, string status, int? operatorId)
    {
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("admin role required");
        if (operatorId == userId) throw new InvalidOperationException("管理员不能封禁或解封自己");

        var action = status.ToLowerInvariant() switch
        {
            "banned" => "ban",
            "normal" => "unban",
            "active" => "unban",
            _ => throw new ArgumentException("invalid user status")
        };

        return await _userRepository.ManageBanAsync(userId, operatorId!.Value, action, null);
    }
    public async Task<bool> UpdateCreditScoreAsync(int userId, int score, int? operatorId)
    {
        // 鉴权：修改信用分一般由系统自动或管理员进行
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("无权操作信用分");
        if (score < 0 || score > 1000) throw new ArgumentException("信用分必须在 0-1000 之间");

        return await _userRepository.UpdateCreditScoreAsync(userId, score);
    }

    // 仿照你们原版的 ToDto 辅助方法进行模型对齐
    private static UserDto ToDto(CampusTrade.Backend.Models.User user)
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

    private static void ValidateUpdate(PartialUserUpdateRequest request)
    {
        if (request.Nickname != null)
        {
            request.Nickname = request.Nickname.Trim();
            if (request.Nickname.Length == 0) throw new ArgumentException("昵称不能为空");
            if (Encoding.UTF8.GetByteCount(request.Nickname) > 50) throw new ArgumentException("昵称过长");
        }

        if (request.Phone != null)
        {
            request.Phone = request.Phone.Trim();
            if (request.Phone.Length > 0 && (request.Phone.Length is < 10 or > 15 || request.Phone.Any(ch => !char.IsDigit(ch))))
                throw new ArgumentException("手机号必须是 10-15 位数字");
        }

        if (request.Email != null)
        {
            request.Email = request.Email.Trim();
            if (request.Email.Length > 100 || (request.Email.Length > 0 && !MailAddress.TryCreate(request.Email, out _)))
                throw new ArgumentException("邮箱格式不正确");
        }

        if (request.AvatarUrl != null)
        {
            request.AvatarUrl = request.AvatarUrl.Trim();
            if (request.AvatarUrl.Length == 0 || request.AvatarUrl.Length > 255)
                throw new ArgumentException("头像地址不合法");
        }
    }
}

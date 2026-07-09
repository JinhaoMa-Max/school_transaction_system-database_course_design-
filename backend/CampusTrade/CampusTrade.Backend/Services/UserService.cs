using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;

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
        // 传递给底层的 Repository 实施 Dapper 分页查询
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

        // 调用底层 UserRepository 将前端传来的增量字段更新进 Oracle 数据库
        var updated = await _userRepository.UpdateUserFieldsAsync(userId, request);
        return ToDto(updated);
    }

    public async Task<bool> DeleteAsync(int userId, int? operatorId)
    {
        // 鉴权：只有管理员可以执行物理/逻辑删除
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("只有管理员能删除用户");

        return await _userRepository.DeleteUserAsync(userId);
    }

    public async Task<bool> UpdateStatusAsync(int userId, string status, int? operatorId)
    {
        // 鉴权：只有管理员可以封禁(banned)或解封(active)账号
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("只有管理员能操作用户状态");

        return await _userRepository.UpdateStatusAsync(userId, status);
    }

    public async Task<bool> UpdateCreditScoreAsync(int userId, int score, int? operatorId)
    {
        // 鉴权：修改信用分一般由系统自动或管理员进行
        var currentUser = operatorId.HasValue ? await _userRepository.GetByIdAsync(operatorId.Value) : null;
        if (currentUser?.Role != "admin") throw new UnauthorizedAccessException("无权操作信用分");

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
}
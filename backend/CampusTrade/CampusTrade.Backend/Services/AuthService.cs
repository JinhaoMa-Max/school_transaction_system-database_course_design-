using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Repositories;

namespace CampusTrade.Backend.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 核心功能：用户登录验证
    /// </summary>
    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // 1. 去数据库查找该学号的学生
        var user = await _userRepository.GetByStudentIdAsync(request.StudentId);
        
        // 2. 验证用户是否存在
        if (user == null) 
        {
            return null; 
        }

        // 3. 业务安全检查：结合前端的 ban 接口，如果用户状态为 0 (被封禁)，拒绝登录
        if (user.Status == 0)
        {
            throw new Exception("该账号已被封禁，请联系管理员！");
        }

        // 4. 验证密码（比对前端传来的明文和数据库里存的密文哈希）
        // 提示：若项目未引入BCrypt包，可换成普通的 MD5 或 string.Equals 比对
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return null; // 密码错误
        }

        // 5. 验证通过，生成 Token（通关令牌）
        string token = "Bearer_" + Guid.NewGuid().ToString("N");

        return new AuthResponseDto
        {
            Token = token,
            StudentId = user.StudentId,
            Username = user.Username
        };
    }

    /// <summary>
    /// 核心功能：新用户注册
    /// </summary>
    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        // 1. 唯一性检查：学号不能重复注册
        if (await _userRepository.IsStudentIdExistsAsync(request.StudentId))
        {
            return false; 
        }

        // 2. 对密码进行哈希加密，保障数据库合规与安全
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. 组装实体，初始化校园二手系统特有的属性
        var newUser = new User
        {
            StudentId = request.StudentId,
            Username = request.Username,
            PasswordHash = hashedPassword,
            Email = request.Email,
            Phone = request.Phone,
            Balance = 100.00m,       // 初始赠送 100 元模拟交易金
            Status = 1,              // 1 代表正常状态
            CreditScore = 100,       // 对应前端接口：新用户初始信用分 100 分
            CreateTime = DateTime.Now
        };

        // 4. 调用仓储层写入 Oracle 数据库
        return await _userRepository.AddAsync(newUser);
    }
}
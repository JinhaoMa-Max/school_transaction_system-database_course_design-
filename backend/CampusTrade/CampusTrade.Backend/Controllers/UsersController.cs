using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusTrade.Backend.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }

    // 1. GET /api/users - 获取用户列表（支持分页、角色筛选）
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? role = null)
    {
        try
        {
            // 仿照 Bargains 风格，返回封装好的分页结果
            var result = await _userService.GetPagedUsersAsync(page, size, role);
            return Ok(ApiResponse<PageResult<UserDto>>.Success(result));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 2. GET /api/users/{userId} - 根据 ID 获取用户详情
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById(int userId)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<object>.Fail(404, "用户不存在"));

            return Ok(ApiResponse<UserDto>.Success(user));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 3. PUT /api/users/{userId} - 更新用户信息
    [HttpPut("{userId:int}")]
    public async Task<IActionResult> Update(int userId, [FromBody] PartialUserUpdateRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            // 越权安全检查：非管理员只能修改自己的信息
            var updatedUser = await _userService.UpdateAsync(userId, request, currentUserId);
            return Ok(ApiResponse<UserDto>.Success(updatedUser, "用户信息更新成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 4. DELETE /api/users/{userId} - 删除用户
    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> Delete(int userId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _userService.DeleteAsync(userId, currentUserId);
            if (!success)
                return NotFound(ApiResponse<object>.Fail(404, "用户不存在或删除失败"));

            return Ok(ApiResponse<object>.Success(new { success = true }, "用户删除成功"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 5. PUT /api/users/{userId}/ban - 封禁用户
    [HttpPut("{userId:int}/ban")]
    public async Task<IActionResult> BanUser(int userId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _userService.UpdateStatusAsync(userId, "banned", currentUserId);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(400, "操作失败"));

            return Ok(ApiResponse<object>.Success(new { success = true }, "用户已被成功封禁"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 6. PUT /api/users/{userId}/unban - 解封用户
    [HttpPut("{userId:int}/unban")]
    public async Task<IActionResult> UnbanUser(int userId)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _userService.UpdateStatusAsync(userId, "active", currentUserId); // 正常通常为 active
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(400, "操作失败"));

            return Ok(ApiResponse<object>.Success(new { success = true }, "用户已解除封禁"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    // 7. PUT /api/users/{userId}/credit - 更新用户信用分
    [HttpPut("{userId:int}/credit")]
    public async Task<IActionResult> UpdateCreditScore(int userId, [FromBody] UpdateCreditRequest request)
    {
        try
        {
            var currentUserId = ResolveCurrentUserId();
            var success = await _userService.UpdateCreditScoreAsync(userId, request.Score, currentUserId);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(400, "信用分更新失败"));

            return Ok(ApiResponse<object>.Success(new { success = true }, "用户信用分已更新"));
        }
        catch (Exception ex)
        {
            return ToErrorResult(ex);
        }
    }

    private int? ResolveCurrentUserId()
    {
        var token = ReadBearerToken();
        return _authService.TryGetUserIdFromToken(token);
    }

    private string? ReadBearerToken()
    {
        var value = Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(value)) return null;

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? value[prefix.Length..].Trim()
            : value.Trim();
    }

    private IActionResult ToErrorResult(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException uae => Unauthorized(ApiResponse<object>.Fail(401, uae.Message)),
            ArgumentException ae => BadRequest(ApiResponse<object>.Fail(400, ae.Message)),
            InvalidOperationException ioe => BadRequest(ApiResponse<object>.Fail(400, ioe.Message)),
            _ => StatusCode(500, ApiResponse<object>.Fail(500, "服务器内部错误"))
        };
    }
}
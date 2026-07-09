using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IUserService
{
    Task<PageResult<UserDto>> GetPagedUsersAsync(int page, int size, string? role);
    Task<UserDto?> GetByIdAsync(int userId);
    Task<UserDto> UpdateAsync(int userId, PartialUserUpdateRequest request, int? operatorId);
    Task<bool> DeleteAsync(int userId, int? operatorId);
    Task<bool> UpdateStatusAsync(int userId, string status, int? operatorId);
    Task<bool> UpdateCreditScoreAsync(int userId, int score, int? operatorId);
}
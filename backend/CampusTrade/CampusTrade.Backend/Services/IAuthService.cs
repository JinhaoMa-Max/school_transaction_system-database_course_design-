using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto> GetCurrentUserAsync(string? token);
    int? TryGetUserIdFromToken(string? token);
    Task<StudentAuthDto?> GetStudentAuthByUserIdAsync(int userId);
    Task<StudentAuthDto> SubmitStudentAuthAsync(StudentAuthRequestDto request, int? currentUserId);
    Task<StudentAuthDto> UpdateStudentAuthAsync(int authId, StudentAuthRequestDto request);
}

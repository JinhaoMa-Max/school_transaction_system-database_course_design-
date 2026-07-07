using CampusTrade.Backend.Models.DTOs;

namespace CampusTrade.Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<bool> RegisterAsync(RegisterRequestDto request);
}
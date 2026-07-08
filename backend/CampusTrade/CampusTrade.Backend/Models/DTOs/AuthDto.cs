namespace CampusTrade.Backend.Models.DTOs;

public class LoginRequestDto
{
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequestDto
{
    public string StudentId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CreditScore { get; set; }
    public DateTime RegisterTime { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class StudentAuthDto
{
    public int AuthId { get; set; }
    public int UserId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string RealName { get; set; } = string.Empty;
    public string College { get; set; } = string.Empty;
    public string AuthStatus { get; set; } = string.Empty;
    public DateTime? AuthTime { get; set; }
}

public class StudentAuthRequestDto
{
    public int? UserId { get; set; }
    public string? StudentId { get; set; }
    public string? RealName { get; set; }
    public string? College { get; set; }
    public string? AuthStatus { get; set; }
}

public class LoginHttpResponseDto
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "success";
    public AuthResponseDto Data { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();

    public static LoginHttpResponseDto Success(AuthResponseDto data, string message = "success")
    {
        return new LoginHttpResponseDto
        {
            Message = message,
            Data = data,
            Token = data.Token,
            User = data.User
        };
    }
}

public class UserHttpResponseDto : UserDto
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "success";
    public UserDto Data { get; set; } = new();

    public static UserHttpResponseDto Success(UserDto user, string message = "success")
    {
        return new UserHttpResponseDto
        {
            Code = 200,
            Message = message,
            Data = user,
            UserId = user.UserId,
            Username = user.Username,
            Password = user.Password,
            Nickname = user.Nickname,
            AvatarUrl = user.AvatarUrl,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            Status = user.Status,
            CreditScore = user.CreditScore,
            RegisterTime = user.RegisterTime
        };
    }
}

public class StudentAuthHttpResponseDto : StudentAuthDto
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "success";
    public StudentAuthDto Data { get; set; } = new();

    public static StudentAuthHttpResponseDto Success(StudentAuthDto auth, string message = "success")
    {
        return new StudentAuthHttpResponseDto
        {
            Code = 200,
            Message = message,
            Data = auth,
            AuthId = auth.AuthId,
            UserId = auth.UserId,
            StudentId = auth.StudentId,
            RealName = auth.RealName,
            College = auth.College,
            AuthStatus = auth.AuthStatus,
            AuthTime = auth.AuthTime
        };
    }
}

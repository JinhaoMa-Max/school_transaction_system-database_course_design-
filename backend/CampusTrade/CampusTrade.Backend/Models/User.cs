namespace CampusTrade.Backend.Models;

public class User
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;   // 学号，作登录账号
    public string Username { get; set; } = string.Empty;    // 用户名/昵称
    public string PasswordHash { get; set; } = string.Empty;  // 密码哈希值
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal Balance { get; set; }                     // 校园卡余额
    public int Status { get; set; } = 1;                     // 1:正常, 0:封禁
    public int CreditScore { get; set; } = 100;              // 信用分
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
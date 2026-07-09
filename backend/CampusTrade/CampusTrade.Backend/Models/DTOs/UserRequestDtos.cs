namespace CampusTrade.Backend.Models.DTOs;

// 1. 用于接收局部更新的用户字段
public class PartialUserUpdateRequest
{
    public string? Nickname { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

// 2. 用于接收修改信用分的请求体
public class UpdateCreditRequest
{
    public int Score { get; set; }
}

// 3. 通用的分页外壳封装（对应前端 PageResult<User>）
public class PageResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}
using System.Reflection;
using CampusTrade.Backend.Controllers;
using CampusTrade.Backend.Models;
using CampusTrade.Backend.Models.DTOs;
using CampusTrade.Backend.Repositories;
using CampusTrade.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// No Oracle instance or additional test packages required. These checks cover
// the public read boundary while retaining authorization for notice management.
var notice = new NoticeDto { NoticeId = 7, Title = "面交提醒", Content = "请在校内公共场所面交。", NoticeType = "transaction" };
var reads = 0;
var writes = 0;
var repo = Stub.Create<IAdminRepository>((method, args) => method switch
{
    "GetNoticesAsync" => ReadPage(args!),
    "GetNoticeByIdAsync" => Task.FromResult((int)args![0]! == 7 ? notice : null),
    "CreateNoticeAsync" => Create(),
    _ => throw new Exception($"Unexpected repository call: {method}")
});
Task<(List<NoticeDto>, int)> ReadPage(object?[] args)
{
    reads++;
    Check((int)args[0]! == 1 && (int)args[1]! == 100, "public paging is bounded");
    Check((string?)args[2] == "transaction", "notice type reaches repository");
    return Task.FromResult((new List<NoticeDto> { notice }, 1));
}
Task<NoticeDto> Create() { writes++; return Task.FromResult(notice); }
var users = Stub.Create<IUserRepository>((method, args) => method == "GetByIdAsync"
    ? Task.FromResult<User?>(new User { UserId = (int)args![0]!, Role = (int)args[0]! == 1 ? "admin" : "user", Status = "normal" })
    : throw new Exception($"Unexpected user call: {method}"));
var service = new AdminService(repo, users);
var controller = new NoticesController(service);

var result = await controller.GetList(-2, 500, "transaction") as OkObjectResult;
var page = (result?.Value as ApiResponse<NoticeListResult>)?.Data;
Check(page?.List.Single().Title == notice.Title && page.Page == 1 && page.Size == 100, "anonymous list returns readable content");
Check(await controller.GetList(1, 10, "invalid") is BadRequestObjectResult, "invalid notice type returns 400");
Check(reads == 1, "invalid filters never reach repository");
Check(await controller.GetById(7) is OkObjectResult, "anonymous detail is readable");
Check(await controller.GetById(999) is NotFoundObjectResult, "deleted notice returns 404");
Check(typeof(NoticesController).IsDefined(typeof(AllowAnonymousAttribute)), "public controller allows anonymous reads");
Check(!typeof(NoticesController).GetMethods().Any(m => m.IsDefined(typeof(HttpPostAttribute)) || m.IsDefined(typeof(HttpPutAttribute)) || m.IsDefined(typeof(HttpDeleteAttribute))), "public controller exposes no mutations");

var request = new CreateNoticeRequest { Title = notice.Title, Content = notice.Content, NoticeType = notice.NoticeType };
await Denied(() => service.CreateNoticeAsync(null, request), "anonymous cannot publish");
await Denied(() => service.CreateNoticeAsync(2, request), "ordinary user cannot publish");
await Denied(() => service.UpdateNoticeAsync(2, 7, new UpdateNoticeRequest { Title = "修改" }), "ordinary user cannot edit");
await Denied(() => service.DeleteNoticeAsync(2, 7), "ordinary user cannot delete");
await Denied(() => service.GetNoticesAsync(2, 1, 10, null), "admin listing remains protected");
Check(writes == 0, "unauthorized requests never write");
await service.CreateNoticeAsync(1, request);
Check(writes == 1, "admin can still publish");
Console.WriteLine("All display contract checks passed.");

static void Check(bool condition, string name)
{
    if (!condition) throw new Exception($"FAIL: {name}");
    Console.WriteLine($"PASS: {name}");
}
static async Task Denied(Func<Task> action, string name)
{
    try { await action(); }
    catch (UnauthorizedAccessException) { Check(true, name); return; }
    throw new Exception($"FAIL: {name}");
}
public class Stub : DispatchProxy
{
    public Func<string, object?[]?, object?> Handler { get; set; } = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!.Name, args);
    public static T Create<T>(Func<string, object?[]?, object?> handler) where T : class
    {
        var proxy = Create<T, Stub>();
        ((Stub)(object)proxy).Handler = handler;
        return proxy;
    }
}

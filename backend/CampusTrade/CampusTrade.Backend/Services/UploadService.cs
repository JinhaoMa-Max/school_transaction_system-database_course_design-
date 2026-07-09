using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace CampusTrade.Backend.Services;

public interface IUploadService
{
    Task<string> UploadImageAsync(IFormFile file);
    string GetImageUrl(string fileName);
}

public class UploadService : IUploadService
{
    private readonly string _storagePath;
    private readonly string _baseUrl;

    public UploadService(IConfiguration configuration)
    {
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration.GetValue<string>("Upload:BaseUrl") ?? "/uploads/";

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("上传文件不能为空");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            throw new ArgumentException("只允许上传 JPG、PNG、GIF、WebP 格式的图片");
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            throw new ArgumentException("图片大小不能超过 5MB");
        }

        var timestamp = DateTime.Now.Ticks.ToString();
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(timestamp + file.FileName));
        var fileName = BitConverter.ToString(hashBytes).Replace("-", "").ToLower() + extension;
        var filePath = Path.Combine(_storagePath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public string GetImageUrl(string fileName)
    {
        return _baseUrl + fileName;
    }
}
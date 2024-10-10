namespace Koi_Web_BE.Utils;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, string fileName, string folderName);
    Task<bool> DeleteImageAsync(string url);
}
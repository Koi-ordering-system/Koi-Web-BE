using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Koi_Web_BE.Utils;

public class ImageService(Cloudinary cloudinaryService) : IImageService
{
    private readonly int _imageLimit = 10485760; // 10MB

    public bool IsTooLarge(Stream imageStream)
    {
        return imageStream.Length > _imageLimit;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string fileName, string folderName)
    {
        ImageUploadParams uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, file.OpenReadStream()),
            Folder = folderName,
            Overwrite = true,
        };

        var uploadResult = await cloudinaryService.UploadLargeAsync(uploadParams);
        if (uploadResult is null) return string.Empty;
        return uploadResult.Url.ToString();
    }
}
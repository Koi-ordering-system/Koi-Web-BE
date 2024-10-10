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
    public async Task<bool> DeleteImageAsync(string url)
    {
        var segments = url.Split('/');
        var publicIdSegment = string.Join("/", segments[^2..]); // Fetch elements from the one before the last to the end
        var publicId = publicIdSegment.Split('.').First();
        var deleteParams = new DeletionParams(publicId);
        var result = await cloudinaryService.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
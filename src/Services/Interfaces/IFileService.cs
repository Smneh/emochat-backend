using Contract.DTOs.FileHandler;
using Microsoft.AspNetCore.Http;
using FileInfo = Contract.DTOs.FileHandler.FileInfo;

namespace Services.Interfaces;

public interface IFileService
{
    Task<UploadResult> Upload(List<IFormFile>? files);
    public Task<DownloadDto> DownloadWithGuid(string guid);
    Task<List<FileInfo>> GetInfo(string md5);
    Task<UpdateResultDto> UpdateProfilePicture(IFormFile profilePicture);
    Task DeleteWallpaper();
    Task DeleteProfilePicture();
}
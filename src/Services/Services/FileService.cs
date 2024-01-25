using Contract.Commands.Profile;
using Contract.DTOs.FileHandler;
using Core.Enums;
using Core.Exceptions;
using Core.Services;
using Core.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;
using Repository.Minio;
using Services.Interfaces;
using FileInfo = Contract.DTOs.FileHandler.FileInfo;

namespace Services.Services;

public class FileService : IFileService
{
    private readonly MinioRepository _minioRepositories;
    private readonly IdentityService _identityService;
    private readonly long _defaultMaxFileSizeInMB;
    private readonly ISender _sender;

    public FileService(MinioRepository minioRepositories, IdentityService identityService, ISender sender)
    {
        _minioRepositories = minioRepositories;
        _identityService = identityService;
        _sender = sender;
        _defaultMaxFileSizeInMB = Settings.AllSettings.DefaultMaxFileSizeInMB;
    }

    public async Task<UploadResult> Upload(List<IFormFile> files)
    {
        var attachments = new List<string>();
        var strAttachments = "-";
        var strGuids = "-";

        foreach (var file in files)
        {
            if (file == null)
                continue;

            await ValidateFileSizeForUser(file.Length);

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var fileName = file.FileName;

            var uploadDto = new UploadDto
            {
                File = file,
                Guid = guid,
                FileName = fileName,
                Username = _identityService.Username,
                Timestamp = DateTime.Now
            };

            var memoryStream = await _getMemoryStream(uploadDto);

            await _minioRepositories.Upload(uploadDto, memoryStream);

            var attachmentId = guid + Path.GetExtension(uploadDto.FileName);
            attachments.Add(attachmentId);
            strAttachments = string.Join(",", attachments);
        }

        return new UploadResult
            { Attachments = strAttachments, Status = "SUCCESS", Message = "-" };
    }

    private async Task ValidateFileSizeForUser(long fileSize)
    {
        var maxFileSizeInBytes = _defaultMaxFileSizeInMB * 1024 * 1024;
        if (fileSize > maxFileSizeInBytes)
        {
            throw new AppException(new { _identityService.Username, fileSize, _defaultMaxFileSizeInMB },
                Messages.FileSizeExceedsLimit);
        }
    }

    public async Task<DownloadDto> DownloadWithGuid(string guid)
    {
        guid = guid.Split(".")[0];
        var result = await _minioRepositories.Download(guid);
        return result;
    }

    public async Task<List<FileInfo>> GetInfo(string guid)
    {
        var ids = guid.Split(',');
        var fileInfoList = new List<FileInfo>();

        foreach (var id in ids)
        {
            var idStr = id.Split(".")[0];
            var res = await _minioRepositories.GetInfo(idStr);
            res.FileGuid = id;
            fileInfoList.Add(res);
        }

        return fileInfoList;
    }

    public async Task<UpdateResultDto> UpdateProfilePicture(IFormFile profilePicture)
    {
        if (profilePicture == null || profilePicture.Length == 0)
            throw new AppException(Messages.EmptyFiles);

        try
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var fileName = profilePicture.FileName;
            var uploadDto = new UploadDto
            {
                File = profilePicture,
                Guid = guid,
                FileName = fileName,
                Username = _identityService.Username,
                Timestamp = DateTime.Now
            };

            var memoryStream = await _getMemoryStream(uploadDto);
            await _minioRepositories.Upload(uploadDto, memoryStream);
            await _sender.Send(new UpdateProfilePictureCommand
            {
                NewId = guid,
                Field = "profilePictureId",
                Username = _identityService.Username
            });
            return new UpdateResultDto
            {
                Result = guid
            };
        }
        catch (Exception e)
        {
            throw new AppException(e.Message, Messages.UpdateProfilePictureFailed);
        }
    }

    public async Task DeleteWallpaper()
    {
        try
        {
            await _sender.Send(new UpdateProfilePictureCommand
            {
                NewId = "-",
                Field = "wallpaperPictureId",
                Username = _identityService.Username
            });
        }
        catch (Exception e)
        {
            throw new AppException(e.Message, Messages.DeleteWallpaperFailed);
        }
    }

    public async Task DeleteProfilePicture()
    {
        try
        {
            await _sender.Send(new UpdateProfilePictureCommand
            {
                NewId = "-",
                Field = "profilePictureId",
                Username = _identityService.Username
            });
        }
        catch (Exception e)
        {
            throw new AppException(e.Message, Messages.DeleteProfilePictureFailed);
        }
    }

    private async Task<MemoryStream> _getMemoryStream(UploadDto uploadDto)
    {
        var memoryStream = new MemoryStream();
        await uploadDto.File.CopyToAsync(memoryStream);
        return memoryStream;
    }
}
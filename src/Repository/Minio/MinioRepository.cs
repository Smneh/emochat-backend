using System.Globalization;
using System.Net;
using Contract.DTOs.FileHandler;
using Core.Enums;
using Core.Exceptions;
using Core.Extensions;
using Core.Settings;
using Minio;
using FileInfo = Contract.DTOs.FileHandler.FileInfo;

namespace Repository.Minio;

public class MinioRepository
{
    private readonly MinioClient _minioClient = new MinioClient()
        .WithEndpoint(Settings.AllSettings.MinioSettings?.Endpoint)
        .WithCredentials(Settings.AllSettings.MinioSettings?.AccessKey, Settings.AllSettings.MinioSettings?.SecretKey)
        .Build();

    private const string BucketName = "emo-chat"; 

    public async Task<string> Upload(UploadDto uploadDto, MemoryStream memoryStream)
    {
        var metaData = new Dictionary<string, string>
        {
            { "extension", uploadDto.File.ContentType },
            { "fileName", WebUtility.UrlEncode(uploadDto.FileName) },
            { "username", uploadDto.Username },
            { "regDate", uploadDto.Timestamp.ToString() },
            { "regTime", uploadDto.Timestamp.TimeOfDay.ToString() },//Todo : check if its okay
            { "uniqueId", uploadDto.Guid },
            { "isObsolete", "0" },
            { "type", Path.GetExtension(uploadDto.FileName) },
            { "timestamp", uploadDto.Timestamp.ToString(CultureInfo.CurrentCulture) },
            { "fileHandler", "Minio" },
            { "fileSize", memoryStream.Length.ToString() }
        };

        var bucketCreated = await CheckAndCreateBucketAsync(BucketName);
        try
        {
            if (!bucketCreated)
            {
                var objLog = new { BucketName };
                throw new AppException(objLog, Messages.UploadFailed);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            await _minioClient.PutObjectAsync(BucketName, uploadDto.Guid, memoryStream, memoryStream.Length,
                uploadDto.File.ContentType, metaData);

            return uploadDto.Guid;
        }
        catch (Exception e)
        {
            var objLog = new { e.Message, uploadDto, memoryStream };
            throw new AppException(objLog, Messages.UploadFailed);
        }
    }

    public async Task<DownloadDto?> Download(string guid)
    {
        try
        {
            var stream = new MemoryStream();
            var fileInfo = await GetInfo(guid);
            await _minioClient.GetObjectAsync(BucketName, guid, (streamResponse) =>
            {
                streamResponse.CopyTo(stream);
                stream.Position = 0;
            });

            var downloadDto = new DownloadDto()
            {
                FileName = fileInfo.FileName,
                File = ConvertType.ConvertStreamToByteArray(stream),
                Timestamp = fileInfo.Timestamp,
                Extension = fileInfo.Extension
            };
            return downloadDto;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<FileInfo?> GetInfo(string guid)
    {
        try
        {
            var stat = await _minioClient.StatObjectAsync(BucketName, guid);
            var regDate = Convert.ToDateTime(stat.MetaData["regDate"]);
            var regTime = stat.MetaData["regTime"];
            var fileInfo = new FileInfo
            {
                Extension = stat.MetaData["extension"],
                FileName = stat.MetaData["fileName"],
                Username = stat.MetaData["username"],
                RegDate = regDate,
                RegTime = regTime,
                UniqueId = stat.MetaData["uniqueId"],
                Type = stat.MetaData["type"],
                Timestamp = DateTime.Parse(stat.MetaData["timestamp"]),
                FileHandler = stat.MetaData["fileHandler"],
                FileSize = Convert.ToInt64(stat.MetaData["fileSize"])
            };

            return fileInfo;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private async Task<bool> CheckAndCreateBucketAsync(string bucketName)
    {
     
        var bucketExists = await _minioClient.BucketExistsAsync(bucketName);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(bucketName);
        }

        return true;
    }
}
using System.Net;
using System.Net.Mime;
using Contract.DTOs.FileHandler;
using Contract.Queries.File;
using Core.Enums;
using Core.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class FileController : BaseController
{
    private readonly IFileService _fileService;
    private readonly ISender _sender;

    public FileController(IFileService fileService, ISender sender)
    {
        _fileService = fileService;
        _sender = sender;
    }

    [HttpPost]
    public async Task<UploadResult> Upload([FromForm] List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
            throw new AppException(files, Messages.EmptyFiles);
        
        var res = await _fileService.Upload(files);

        return res;
    }

    [AllowAnonymous]
    [HttpGet("{guid}")]
    public async Task<FileContentResult> Download(string guid, string token = "", bool inLine = false)
    {
        if (string.IsNullOrEmpty(guid))
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            throw new BadHttpRequestException("File Not Found!", 404);
        }

        var downloadDto = await _sender.Send(new DownloadQuery
        {
            Guid = guid
        });

        if (downloadDto.File == null)
        {
            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            throw new BadHttpRequestException("File Not Found!", 404);
        }

        var contentDisposition = new ContentDisposition
        {
            FileName = WebUtility.UrlEncode(downloadDto.FileName),
            Inline = inLine
        };

        HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
        HttpContext.Response.Headers["Content-Disposition"] = contentDisposition.ToString();
        HttpContext.Response.Headers["Content-Length"] = downloadDto.File.Length.ToString();

        HttpContext.Response.Headers.Add("Cache-Control", "public,max-age=86400"); // Cache for 1 hour
        HttpContext.Response.Headers.Add("Expires", DateTime.UtcNow.AddHours(24).ToString("R")); // Expire time
        HttpContext.Response.Headers.Add("Last-Modified", downloadDto.Timestamp.ToString("R"));

        return File(downloadDto.File, downloadDto.Extension); 
    }

    [HttpGet("{guid}")]
    public async Task<IActionResult> GetInfo(string guid)
    {
        if (string.IsNullOrEmpty(guid))
            return BadRequest("Empty input");

        var fileInfo = await _fileService.GetInfo(guid);

        if (fileInfo == null)
            return NotFound("File not found");

        foreach (var info in fileInfo)
        {
            info.FileName = WebUtility.UrlDecode(info.FileName);
        }

        return Ok(fileInfo);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] IFormFile profilePicture)
    {
        var result = await _fileService.UpdateProfilePicture(profilePicture);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProfilePicture()
    {
        await _fileService.DeleteProfilePicture();
        return Ok();
    }
}
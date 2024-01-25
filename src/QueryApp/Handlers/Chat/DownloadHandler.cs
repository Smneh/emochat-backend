using Contract.DTOs.FileHandler;
using Contract.Queries.File;
using MediatR;
using Repository.Minio;

namespace QueryApp.Handlers.Chat;

public class DownloadHandler : IRequestHandler<DownloadQuery, DownloadDto>
{
    private readonly MinioRepository _minioRepositories;

    public DownloadHandler(MinioRepository minioRepositories)
    {
        _minioRepositories = minioRepositories;
    }

    public async Task<DownloadDto> Handle(DownloadQuery request, CancellationToken cancellationToken)
    {
        var guid = request.Guid.Split(".")[0];
        var result = await _minioRepositories.Download(guid);
        return result;
    }
}
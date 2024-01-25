using Contract.DTOs.FileHandler;
using MediatR;

namespace Contract.Queries.File;

public class DownloadQuery : IRequest<DownloadDto>
{
    public string Guid { get; set; } = default!;
}
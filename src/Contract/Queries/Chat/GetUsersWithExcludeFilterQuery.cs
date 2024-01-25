using MediatR;

namespace Contract.Queries.Chat;

public class GetUsersWithExcludeFilterQuery : IRequest<List<Entities.Models.Profile.Profile>>
{
    public string GroupId { get; set; } = default!;
    public string? SearchText { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
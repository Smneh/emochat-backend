using MediatR;

namespace Contract.Queries.Profile;

public class GetUsersQuery : IRequest<List<Entities.Models.Profile.Profile>>
{
    public string? SearchText { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
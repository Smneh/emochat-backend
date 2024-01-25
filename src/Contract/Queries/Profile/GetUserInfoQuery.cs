using MediatR;

namespace Contract.Queries.Profile;

public class GetUserInfoQuery : IRequest<Entities.Models.Profile.Profile>
{
    public string Username { get; set; } = default!;
}
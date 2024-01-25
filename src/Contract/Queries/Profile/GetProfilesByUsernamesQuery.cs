using MediatR;

namespace Contract.Queries.Profile;

public class GetProfilesByUsernamesQuery : IRequest<List<Entities.Models.Profile.Profile>>
{
    public List<string> Usernames { get; set; }
}
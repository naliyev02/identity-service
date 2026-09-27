using Identity.Domain.User.Entities;

namespace Identity.Application.Abstractions.Security;

public interface IAccessTokenIssuer
{
    string Issue(User user, DateTime expiresAtUtc);
}

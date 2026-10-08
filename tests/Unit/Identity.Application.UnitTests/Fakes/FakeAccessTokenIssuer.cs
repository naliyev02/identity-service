using Identity.Application.Abstractions.Security;
using Identity.Domain.User.Entities;

namespace Identity.Application.UnitTests.Fakes;

public sealed class FakeAccessTokenIssuer : IAccessTokenIssuer
{
    public string Issue(User user, DateTime expiresAtUtc) => $"access:{user.Id}";
}

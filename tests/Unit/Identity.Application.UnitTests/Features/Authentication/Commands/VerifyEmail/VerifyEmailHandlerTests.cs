using Identity.Application.Features.Authentication.Commands.VerifyEmail;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.User.Entities;
using Identity.Domain.User.Enums;
using Identity.Domain.User.Exceptions;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.VerifyEmail;

public sealed class VerifyEmailHandlerTests
{
    private const string RawToken = "verify-token";

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BlankToken_Throws(string token)
    {
        //Arrange
        var handler = Create(new FakeEmailVerificationTokenRepository(), new FakeUserRepository());

        //Act
        var act = () => handler.Handle(new VerifyEmailCommand(token), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidVerificationTokenException>(act);
    }

    [Fact]
    public async Task UnknownToken_Throws()
    {
        //Arrange
        var handler = Create(new FakeEmailVerificationTokenRepository(), new FakeUserRepository());

        //Act
        var act = () => handler.Handle(new VerifyEmailCommand(RawToken), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidVerificationTokenException>(act);
    }

    [Fact]
    public async Task UsedToken_Throws()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var token = EmailVerificationToken.Create(Guid.NewGuid(), hasher.Hash(RawToken), TimeSpan.FromHours(1));
        token.MarkUsed(DateTime.UtcNow);
        var tokens = new FakeEmailVerificationTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var handler = Create(tokens, new FakeUserRepository(), hasher);

        //Act
        var act = () => handler.Handle(new VerifyEmailCommand(RawToken), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidVerificationTokenException>(act);
    }

    [Fact]
    public async Task MissingUser_Throws()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var token = EmailVerificationToken.Create(Guid.NewGuid(), hasher.Hash(RawToken), TimeSpan.FromHours(1));
        var tokens = new FakeEmailVerificationTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var handler = Create(tokens, new FakeUserRepository(), hasher);

        //Act
        var act = () => handler.Handle(new VerifyEmailCommand(RawToken), CancellationToken.None);

        //Assert
        await Assert.ThrowsAsync<InvalidVerificationTokenException>(act);
    }

    [Fact]
    public async Task ValidToken_ActivatesUser()
    {
        //Arrange
        var hasher = new FakeTokenHasher();
        var user = TestUsers.Pending();
        var token = EmailVerificationToken.Create(user.Id, hasher.Hash(RawToken), TimeSpan.FromHours(1));
        var tokens = new FakeEmailVerificationTokenRepository();
        await tokens.AddAsync(token, CancellationToken.None);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new VerifyEmailHandler(tokens, new FakeUserRepository(user), hasher, unitOfWork);

        //Act
        var result = await handler.Handle(new VerifyEmailCommand($"  {RawToken}  "), CancellationToken.None);

        //Assert
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(AccountState.Active, result.State);
        Assert.Equal(AccountState.Active, user.State);
        Assert.NotNull(token.UsedAtUtc);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    private static VerifyEmailHandler Create(
        FakeEmailVerificationTokenRepository tokens,
        FakeUserRepository users,
        FakeTokenHasher? hasher = null)
        => new(tokens, users, hasher ?? new FakeTokenHasher(), new FakeUnitOfWork());
}

using Identity.Application.Features.Authentication.Commands.ResendVerification;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.ResendVerification;

public sealed class ResendVerificationHandlerTests
{
    private const string RawToken = "verify+token";

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BlankEmail_SendsNothing(string email)
    {
        //Arrange
        var context = Create();

        //Act
        await context.Handler.Handle(new ResendVerificationCommand(email), CancellationToken.None);

        //Assert
        Assert.Empty(context.Emails.Sent);
    }

    [Fact]
    public async Task UnknownEmail_SendsNothing()
    {
        //Arrange
        var context = Create();

        //Act
        await context.Handler.Handle(new ResendVerificationCommand(TestUsers.Email), CancellationToken.None);

        //Assert
        Assert.Empty(context.Emails.Sent);
    }

    [Fact]
    public async Task ActiveUser_SendsNothing()
    {
        //Arrange
        var context = Create(TestUsers.Active());

        //Act
        await context.Handler.Handle(new ResendVerificationCommand(TestUsers.Email), CancellationToken.None);

        //Assert
        Assert.Empty(context.Emails.Sent);
    }

    [Fact]
    public async Task PendingUser_StoresTokenAndSendsLink()
    {
        //Arrange
        var context = Create(TestUsers.Pending());

        //Act
        await context.Handler.Handle(new ResendVerificationCommand(TestUsers.Email), CancellationToken.None);

        //Assert
        var token = Assert.Single(context.Tokens.Tokens);
        Assert.Equal(context.TokenHasher.Hash(RawToken), token.TokenHash);
        Assert.Null(token.UsedAtUtc);
        var email = Assert.Single(context.Emails.Sent);
        Assert.Equal(TestUsers.Email, email.To);
        Assert.Contains(
            $"{TestOptions.PublicBaseUrl}/verify-email?token=verify%2Btoken",
            email.HtmlBody);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    private static ResendContext Create(params Identity.Domain.User.Entities.User[] users)
    {
        var tokens = new FakeEmailVerificationTokenRepository();
        var unitOfWork = new FakeUnitOfWork();
        var emails = new FakeEmailSender(unitOfWork);
        var tokenHasher = new FakeTokenHasher();
        var handler = new ResendVerificationHandler(
            new FakeUserRepository(users),
            unitOfWork,
            new EmailVerificationIssuer(
                tokens,
                new FakeVerificationTokenGenerator { Token = RawToken },
                tokenHasher,
                emails,
                TestOptions.App()));

        return new ResendContext(tokens, emails, tokenHasher, unitOfWork, handler);
    }

    private sealed record ResendContext(
        FakeEmailVerificationTokenRepository Tokens,
        FakeEmailSender Emails,
        FakeTokenHasher TokenHasher,
        FakeUnitOfWork UnitOfWork,
        ResendVerificationHandler Handler);
}

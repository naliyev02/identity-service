using Identity.Application.Features.Authentication.Commands.ForgotPassword;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Xunit;

namespace Identity.Application.UnitTests.Features.Authentication.Commands.ForgotPassword;

public sealed class ForgotPasswordHandlerTests
{
    private const string ResetToken = "reset+token";

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BlankEmail_SendsNothing(string email)
    {
        //Arrange
        var context = Create();

        //Act
        await context.Handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

        //Assert
        Assert.Empty(context.Emails.Sent);
    }

    [Fact]
    public async Task UnknownEmail_SendsNothing()
    {
        //Arrange
        var context = Create();

        //Act
        await context.Handler.Handle(new ForgotPasswordCommand(TestUsers.Email), CancellationToken.None);

        //Assert
        Assert.Empty(context.Emails.Sent);
    }

    [Fact]
    public async Task KnownEmail_StoresResetTokenAndSendsLink()
    {
        //Arrange
        var context = Create(TestUsers.Active());

        //Act
        await context.Handler.Handle(new ForgotPasswordCommand(TestUsers.Email), CancellationToken.None);

        //Assert
        var token = Assert.Single(context.Tokens.Tokens);
        Assert.Equal(context.TokenHasher.Hash(ResetToken), token.TokenHash);
        Assert.Null(token.UsedAtUtc);
        var email = Assert.Single(context.Emails.Sent);
        Assert.Equal(TestUsers.Email, email.To);
        Assert.Contains(
            $"{TestOptions.PublicBaseUrl}/reset-password?token=reset%2Btoken",
            email.HtmlBody);
        Assert.Equal(1, context.UnitOfWork.SaveCount);
    }

    private static ForgotContext Create(params Identity.Domain.User.Entities.User[] users)
    {
        var tokens = new FakePasswordResetTokenRepository();
        var unitOfWork = new FakeUnitOfWork();
        var emails = new FakeEmailSender(unitOfWork);
        var tokenHasher = new FakeTokenHasher();
        var handler = new ForgotPasswordHandler(
            new FakeUserRepository(users),
            unitOfWork,
            new PasswordResetIssuer(
                tokens,
                new FakeVerificationTokenGenerator { Token = ResetToken },
                tokenHasher,
                emails,
                TestOptions.App()));

        return new ForgotContext(tokens, emails, tokenHasher, unitOfWork, handler);
    }

    private sealed record ForgotContext(
        FakePasswordResetTokenRepository Tokens,
        FakeEmailSender Emails,
        FakeTokenHasher TokenHasher,
        FakeUnitOfWork UnitOfWork,
        ForgotPasswordHandler Handler);
}

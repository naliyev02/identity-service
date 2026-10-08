using Identity.Application.Features.Users.Commands.Register;
using Identity.Application.Services;
using Identity.Application.UnitTests.Fakes;
using Identity.Application.UnitTests.TestData;
using Identity.Domain.Authorization;
using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Entities;
using Identity.Domain.User.Enums;
using Xunit;

namespace Identity.Application.UnitTests.Features.Users.Commands.Register;

public sealed class RegisterUserHandlerTests
{
    private const string PlainPassword = "plain-password";

    [Fact]
    public async Task ExistingEmail_ReturnsEmailAlreadyExists()
    {
        //Arrange
        var harness = Harness.WithUsers(TestUsers.Pending());
        var command = new RegisterUserCommand(
            $"  {TestUsers.Email.ToUpperInvariant()}  ",
            PlainPassword,
            TestUsers.FirstName,
            TestUsers.LastName);

        //Act
        var result = await harness.Handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result.Succeeded);
        Assert.Equal("EmailAlreadyExists", result.ErrorCode);
    }

    [Fact]
    public async Task MissingUserRole_ThrowsBeforeCreatingUser()
    {
        //Arrange
        var harness = Harness.WithRoles(TestRoles.Admin());

        //Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            harness.Handler.Handle(NewUser(), CancellationToken.None));

        //Assert
        Assert.Equal("User role is not seeded.", exception.Message);
    }

    [Fact]
    public async Task NewEmail_StoresPasswordHashAndSendsVerification()
    {
        //Arrange
        var userRole = TestRoles.User();
        var harness = Harness.WithRoles(userRole);
        harness.TokenGenerator.Token = "raw+token";

        //Act
        var result = await harness.Handler.Handle(NewUser(), CancellationToken.None);

        //Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.ErrorCode);
        Assert.Equal(TestUsers.Email, result.Email);
        Assert.Equal(TestUsers.FirstName, result.FirstName);
        Assert.Equal(TestUsers.LastName, result.LastName);
        Assert.Equal(AccountState.PendingVerification, result.State);

        var user = Assert.Single(await harness.Users.ListAsync(CancellationToken.None));
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(TestUsers.PasswordHash, user.Password.Hash);
        Assert.Equal(AccountState.PendingVerification, user.State);
        Assert.Equal(RoleNames.User, Assert.Single(user.Roles).Name);

        var verificationToken = Assert.Single(harness.VerificationTokens.Tokens);
        Assert.Equal(user.Id, verificationToken.UserId);
        Assert.Equal(harness.TokenHasher.Hash("raw+token"), verificationToken.TokenHash);
        Assert.Null(verificationToken.UsedAtUtc);

        Assert.Equal(1, harness.UnitOfWork.SaveCount);
        var email = Assert.Single(harness.Emails.Sent);
        Assert.Equal(TestUsers.Email, email.To);
        Assert.Equal(1, harness.Emails.SaveCountAtSend);
        Assert.Contains(
            $"{TestOptions.PublicBaseUrl}/verify-email?token=raw%2Btoken",
            email.HtmlBody);
    }

    private static RegisterUserCommand NewUser()
        => new(TestUsers.Email, PlainPassword, TestUsers.FirstName, TestUsers.LastName);

    private sealed class Harness
    {
        private Harness(
            FakeUserRepository users,
            FakeUnitOfWork unitOfWork,
            FakeEmailVerificationTokenRepository verificationTokens,
            FakeVerificationTokenGenerator tokenGenerator,
            FakeTokenHasher tokenHasher,
            FakeEmailSender emails,
            RegisterUserHandler handler)
        {
            Users = users;
            UnitOfWork = unitOfWork;
            VerificationTokens = verificationTokens;
            TokenGenerator = tokenGenerator;
            TokenHasher = tokenHasher;
            Emails = emails;
            Handler = handler;
        }

        public FakeUserRepository Users { get; }
        public FakeUnitOfWork UnitOfWork { get; }
        public FakeEmailVerificationTokenRepository VerificationTokens { get; }
        public FakeVerificationTokenGenerator TokenGenerator { get; }
        public FakeTokenHasher TokenHasher { get; }
        public FakeEmailSender Emails { get; }
        public RegisterUserHandler Handler { get; }

        public static Harness WithUsers(params User[] users) => Create(users, []);

        public static Harness WithRoles(params Role[] roles) => Create([], roles);

        private static Harness Create(User[] users, Role[] roles)
        {
            var userRepository = new FakeUserRepository(users);
            var passwords = new FakePasswordHasher();
            passwords.Use(PlainPassword, TestUsers.PasswordHash);
            var unitOfWork = new FakeUnitOfWork();
            var verificationTokens = new FakeEmailVerificationTokenRepository();
            var tokenGenerator = new FakeVerificationTokenGenerator();
            var tokenHasher = new FakeTokenHasher();
            var emails = new FakeEmailSender(unitOfWork);
            var handler = new RegisterUserHandler(
                userRepository,
                new FakeRoleRepository(roles),
                unitOfWork,
                passwords,
                new EmailVerificationIssuer(
                    verificationTokens,
                    tokenGenerator,
                    tokenHasher,
                    emails,
                    TestOptions.App()));

            return new Harness(
                userRepository,
                unitOfWork,
                verificationTokens,
                tokenGenerator,
                tokenHasher,
                emails,
                handler);
        }
    }
}

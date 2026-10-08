using Identity.Domain.Roles.Entities;
using Identity.Domain.User.Entities;
using Identity.Domain.User.ValueObjects;

namespace Identity.Application.UnitTests.TestData;

public static class TestUsers
{
    public const string Email = "test@identity.local";
    public const string FirstName = "testName";
    public const string LastName = "testSurname";
    public const string PasswordHash = "hashed-password";

    public static User Pending()
        => Pending(Email, FirstName, LastName);

    public static User Pending(string email, string firstName, string lastName)
        => new(new Email(email), new Password(PasswordHash), new FullName(firstName, lastName));

    public static User Active(params Role[] roles)
        => Active(Email, FirstName, LastName, roles);

    public static User Active(string email, string firstName, string lastName, params Role[] roles)
    {
        var user = Pending(email, firstName, lastName);
        user.VerifyEmail();
        if (roles.Length > 0)
            user.AssignRoles(roles);

        return user;
    }
}

namespace Identity.Application.Abstractions.Security;

public interface IVerificationTokenGenerator
{
    string Generate();
}

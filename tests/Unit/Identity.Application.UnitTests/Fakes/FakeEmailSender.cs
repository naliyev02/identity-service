using Identity.Application.Abstractions.Messaging;

namespace Identity.Application.UnitTests.Fakes;

public sealed record SentEmail(string To, string Subject, string HtmlBody);

public sealed class FakeEmailSender : IEmailSender
{
    private readonly FakeUnitOfWork _unitOfWork;

    public FakeEmailSender(FakeUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public List<SentEmail> Sent { get; } = [];

    public int SaveCountAtSend { get; private set; }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        SaveCountAtSend = _unitOfWork.SaveCount;
        Sent.Add(new SentEmail(to, subject, htmlBody));
        return Task.CompletedTask;
    }
}

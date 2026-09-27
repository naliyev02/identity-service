using Identity.Application.Abstractions.Persistence;
using Identity.Application.Abstractions.Security;
using MediatR;

namespace Identity.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly ISessionRepository _sessions;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutHandler(
        ISessionRepository sessions,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork)
    {
        _sessions = sessions;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var session = await _sessions.GetByHashAsync(
            _tokenHasher.Hash(request.RefreshToken),
            cancellationToken);
        if (session is null)
            return;

        session.Revoke(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

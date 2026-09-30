using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.Application.Abstractions.Security;
using Identity.Application.Options;
using Identity.Domain.Authorization;
using Identity.Domain.User.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly JwtOptions _options;

    public JwtAccessTokenIssuer(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");
    }

    public string Issue(User user, DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Name, user.Name.ToString())
        };

        foreach (var role in user.Roles.Select(role => role.Name).Distinct())
            claims.Add(new Claim(IdentityClaims.Role, role));

        foreach (var permission in user.Roles
                     .SelectMany(role => role.Permissions)
                     .Select(permission => permission.Name)
                     .Distinct())
            claims.Add(new Claim(IdentityClaims.Permission, permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

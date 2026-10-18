using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChugimCenter.Application.Interfaces.Security;
using ChugimCenter.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ChugimCenter.Infrastructure.Security;

/// <summary>
/// יוצר JWT חתום. ב-token נשמרים רק המזהה, השם והתפקיד – בלי אימייל ובלי פרטים אישיים.
/// החתימה במפתח הסודי היא מה שמונע זיוף: שינוי של התפקיד בתוך ה-token ישבור אותה.
/// </summary>
public class JwtTokenGenerator(IOptions<JwtSettings> options) : ITokenGenerator
{
    private readonly JwtSettings _settings = options.Value;

    public GeneratedToken Generate(User user)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_settings.ExpiryMinutes);

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString())
        ];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new GeneratedToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

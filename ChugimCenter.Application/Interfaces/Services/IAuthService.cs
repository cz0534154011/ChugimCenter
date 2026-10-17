using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Auth;

namespace ChugimCenter.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct);
}

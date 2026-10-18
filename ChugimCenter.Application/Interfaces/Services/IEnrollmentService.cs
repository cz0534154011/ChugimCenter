using ChugimCenter.Application.Common;
using ChugimCenter.Application.DTOs.Common;
using ChugimCenter.Application.DTOs.Enrollments;

namespace ChugimCenter.Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<Result<EnrollmentDto>> EnrollAsync(CreateEnrollmentDto dto, CurrentUser user, CancellationToken ct);
    Task<Result<EnrollmentDto>> GetByIdAsync(int enrollmentId, CurrentUser user, CancellationToken ct);
    Task<Result> CancelAsync(int enrollmentId, CurrentUser user, CancellationToken ct);

    Task<PagedResult<EnrollmentDto>> GetMineAsync(EnrollmentQueryDto query, int userId, CancellationToken ct);
    Task<PagedResult<EnrollmentDto>> GetPagedAsync(EnrollmentQueryDto query, CancellationToken ct);

    Task<Result<EnrollmentDto>> SetPaymentAsync(int enrollmentId, bool isPaid, int adminId, CancellationToken ct);
}

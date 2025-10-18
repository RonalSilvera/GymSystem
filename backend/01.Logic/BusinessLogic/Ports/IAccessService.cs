namespace BusinessLogic.Ports;

using Infrastructure.Dto;

public interface IAccessService
{
    Task<AccessValidationResult> ValidateAsync(string refid, string? deviceId, DateTime now);
    Task<PagedResult<AccessLogDto>> ListLogsAsync(DateTime? from, DateTime? to, int? clientId, bool? allowed, int page, int pageSize);
    Task<IReadOnlyList<AccessLogDto>> ExportLogsAsync(DateTime? from, DateTime? to, int? clientId, bool? allowed);
}

public record AccessValidationResult(bool Allowed, int? ClientId, int? MembershipId, string Message, string? Reason = null);

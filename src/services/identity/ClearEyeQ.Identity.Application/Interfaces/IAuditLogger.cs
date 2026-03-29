using ClearEyeQ.SharedKernel.Domain.ValueObjects;

namespace ClearEyeQ.Identity.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(string action, UserId userId, string detail, CancellationToken cancellationToken = default);
}

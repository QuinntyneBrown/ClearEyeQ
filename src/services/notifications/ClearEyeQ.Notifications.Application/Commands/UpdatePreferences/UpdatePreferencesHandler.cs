using ClearEyeQ.Notifications.Application.Interfaces;
using ClearEyeQ.Notifications.Domain.Entities;
using ClearEyeQ.Notifications.Domain.ValueObjects;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Notifications.Application.Commands.UpdatePreferences;

public sealed class UpdatePreferencesHandler : IRequestHandler<UpdatePreferencesCommand>
{
    private readonly IPreferenceRepository _repository;

    public UpdatePreferencesHandler(IPreferenceRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var preference = await _repository.GetAsync(userId, tenantId, request.Channel, cancellationToken);

        QuietHoursPolicy? quietHoursPolicy = null;
        if (request.QuietHoursStart.HasValue && request.QuietHoursEnd.HasValue && request.TimeZone is not null)
        {
            quietHoursPolicy = new QuietHoursPolicy(
                request.QuietHoursStart.Value,
                request.QuietHoursEnd.Value,
                request.TimeZone);
        }

        if (preference is null)
        {
            preference = NotificationPreference.Create(
                userId, tenantId, request.Channel, request.Enabled, quietHoursPolicy);
        }
        else
        {
            if (request.Enabled)
                preference.Enable();
            else
                preference.Disable();

            if (quietHoursPolicy is not null)
                preference.SetQuietHours(quietHoursPolicy);
            else
                preference.ClearQuietHours();
        }

        await _repository.SaveAsync(preference, cancellationToken);
    }
}

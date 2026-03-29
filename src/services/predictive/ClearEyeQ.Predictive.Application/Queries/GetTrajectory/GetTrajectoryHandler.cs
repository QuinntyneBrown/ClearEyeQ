using ClearEyeQ.Predictive.Application.Interfaces;
using ClearEyeQ.SharedKernel.Domain.ValueObjects;
using MediatR;

namespace ClearEyeQ.Predictive.Application.Queries.GetTrajectory;

public sealed class GetTrajectoryHandler : IRequestHandler<GetTrajectoryQuery, TrajectoryDto?>
{
    private readonly IPredictionRepository _repository;

    public GetTrajectoryHandler(IPredictionRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrajectoryDto?> Handle(GetTrajectoryQuery request, CancellationToken ct)
    {
        var userId = new UserId(request.UserId);
        var tenantId = new TenantId(request.TenantId);

        var prediction = await _repository.GetLatestByUserAsync(userId, tenantId, ct);

        if (prediction?.TrajectoryModel is null)
            return null;

        var trajectory = prediction.TrajectoryModel;

        return new TrajectoryDto(
            prediction.PredictionId,
            trajectory.HorizonMonths,
            trajectory.WithTreatment.Select(t => new TrajectoryPointDto(
                t.Date.ToString("yyyy-MM-dd"),
                t.ProjectedScore,
                t.ConfidenceLower,
                t.ConfidenceUpper)).ToList(),
            trajectory.WithoutTreatment.Select(t => new TrajectoryPointDto(
                t.Date.ToString("yyyy-MM-dd"),
                t.ProjectedScore,
                t.ConfidenceLower,
                t.ConfidenceUpper)).ToList());
    }
}

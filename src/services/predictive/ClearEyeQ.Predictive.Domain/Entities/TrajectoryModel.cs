using ClearEyeQ.Predictive.Domain.ValueObjects;

namespace ClearEyeQ.Predictive.Domain.Entities;

public sealed class TrajectoryModel
{
    public int HorizonMonths { get; private set; }
    public List<TrajectoryPoint> WithTreatment { get; private set; }
    public List<TrajectoryPoint> WithoutTreatment { get; private set; }

    private TrajectoryModel()
    {
        WithTreatment = [];
        WithoutTreatment = [];
    }

    public TrajectoryModel(
        int horizonMonths,
        List<TrajectoryPoint> withTreatment,
        List<TrajectoryPoint> withoutTreatment)
    {
        if (horizonMonths <= 0)
            throw new ArgumentOutOfRangeException(nameof(horizonMonths), "Horizon must be positive.");

        HorizonMonths = horizonMonths;
        WithTreatment = withTreatment ?? [];
        WithoutTreatment = withoutTreatment ?? [];
    }
}

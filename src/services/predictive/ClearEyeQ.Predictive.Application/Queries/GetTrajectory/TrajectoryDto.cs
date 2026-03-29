namespace ClearEyeQ.Predictive.Application.Queries.GetTrajectory;

public sealed record TrajectoryDto(
    Guid PredictionId,
    int HorizonMonths,
    List<TrajectoryPointDto> WithTreatment,
    List<TrajectoryPointDto> WithoutTreatment);

public sealed record TrajectoryPointDto(
    string Date,
    double ProjectedScore,
    double ConfidenceLower,
    double ConfidenceUpper);

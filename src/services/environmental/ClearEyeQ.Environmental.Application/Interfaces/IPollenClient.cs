namespace ClearEyeQ.Environmental.Application.Interfaces;

public record PollenData(int TreeCount, int GrassCount, int WeedCount);

public interface IPollenClient
{
    Task<PollenData?> GetPollenCountAsync(double latitude, double longitude, CancellationToken ct);
}

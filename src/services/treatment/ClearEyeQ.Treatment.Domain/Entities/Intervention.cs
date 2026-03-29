using ClearEyeQ.Treatment.Domain.Enums;

namespace ClearEyeQ.Treatment.Domain.Entities;

public abstract class Intervention
{
    public Guid InterventionId { get; protected set; } = Guid.NewGuid();
    public InterventionType InterventionType { get; protected set; }
    public string Description { get; protected set; } = string.Empty;
    public bool IsActive { get; protected set; } = true;

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}

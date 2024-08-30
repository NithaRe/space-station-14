using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Orienta.Antags.PossessedRuler;

[RegisterComponent]
public sealed partial class DefiledSoulComponent : Component
{
    [DataField]
    public TimeSpan LifeTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan InitTime = TimeSpan.Zero;
}

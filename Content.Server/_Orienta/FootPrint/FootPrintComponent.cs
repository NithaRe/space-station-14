using Content.Shared.Decals;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server._Orienta.FootPrint;
/// <summary>
///  Сохраняет тип следов и его настры.
/// </summary>
[RegisterComponent, Access(typeof(FootPrintSystem))]
public sealed partial class FootPrintComponent : Component
{
    [DataField]
    public ProtoId<DecalPrototype> SlediProto = "FootprintBoots";
    [DataField]
    public float FootPrintDistance = 1f;
    [DataField]
    public float DistanceFootPrint = 10f;
    [DataField]
    public Color FootPrintColor = Color.White;
    [DataField]
    public float DistanceTraveled = 0f;
    [DataField]
    public float Intensity = 0f;
    [DataField]
    public FixedPoint2 PickSolution = 1f;
    [DataField]
    public float DistanceIntensityCost = 0.05f;
}

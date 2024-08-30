using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Orienta.Antags.PossessedRuler;

[RegisterComponent]
public sealed partial class SoulLiberatorComponent : Component
{
    [DataField]
    public SoundSpecifier UseSound = new SoundCollectionSpecifier("possessing");

    [DataField]
    public float Delay = 3;

    [DataField("entityPrototype", required: true)]
    public EntProtoId EntityPrototype = default!;
}


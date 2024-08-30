using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Orienta.Antags.PossessedRuler;

[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    [DataField]
    public SoundSpecifier UseSound = new SoundCollectionSpecifier("possessing");

    [DataField("entityPrototype", required: true)]
    public EntProtoId EntityPrototype = default!;
}


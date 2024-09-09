using Robust.Shared.Audio;

namespace Content.Server._Orienta.PersonalSignature;

[RegisterComponent]
public sealed partial class OrientaPersonalSignatureComponent : Component
{
    [DataField]
    public SoundSpecifier? SignSound;
}
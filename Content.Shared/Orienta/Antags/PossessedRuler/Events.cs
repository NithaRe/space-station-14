using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Orienta.Antags.PossessedRuler;

[Serializable, NetSerializable]
public sealed partial class SoulLiberationDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}

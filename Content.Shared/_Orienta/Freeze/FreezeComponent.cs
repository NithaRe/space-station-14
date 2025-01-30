using Content.Shared.Chat.Prototypes;
using Content.Shared.Humanoid;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._Orienta.Freeze;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class FreezeComponent : Component
{
    [DataField("freezeId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [AutoNetworkedField]
    public string FreezeId = "Freeze";

    [DataField("freezeAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [AutoNetworkedField]
    public string FreezeAction = "ActionFreeze";

    [DataField("freezeActionEntity")]
    [AutoNetworkedField]
    public EntityUid? FreezeActionEntity;

    [DataField("targetEntity")]
    public EntityUid? TargetEntity;
}

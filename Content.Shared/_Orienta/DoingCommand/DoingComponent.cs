using Robust.Shared.GameStates;

namespace Content.Shared._Orienta.DoingCommand;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoingComponent : Component
{
    [DataField, AutoNetworkedField]
    [Access(typeof(DoingSystem), Friend = AccessPermissions.ReadWrite, Other = AccessPermissions.Read)]
    public bool Enabled = true;
}

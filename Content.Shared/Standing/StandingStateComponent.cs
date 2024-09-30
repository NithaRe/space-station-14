using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Standing;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StandingStateComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

    // Orienta: Laying start
    [DataField, AutoNetworkedField]
    public StandingState CurrentState { get; set; } = StandingState.Standing;
    // Orienta: Laying end

    [DataField, AutoNetworkedField]
    public bool Standing { get; set; } = true;

    /// <summary>
    ///     List of fixtures that had their collision mask changed when the entity was downed.
    ///     Required for re-adding the collision mask.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> ChangedFixtures = new();
}
// Orienta: Laying start
public enum StandingState
{
    Lying,
    GettingUp,
    Standing,
}
// Orienta: Laying end
using Content.Server.Actions;
using Content.Shared._Orienta.Freeze;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Orienta.Freeze;

public sealed class FreezeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FreezeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FreezeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FreezeComponent, FreezeActionEvent>(OnFreezeAction);
    }

    private void OnMapInit(EntityUid uid, FreezeComponent component, MapInitEvent args)
    {
        // try to add scream action when vocal comp added
        _actions.AddAction(uid, ref component.FreezeActionEntity, component.FreezeAction);
    }

    private void OnShutdown(EntityUid uid, FreezeComponent component, ComponentShutdown args)
    {
        // remove scream action when component removed
        if (component.FreezeActionEntity != null)
        {
            _actions.RemoveAction(uid, component.FreezeActionEntity);
        }
    }

    private void OnFreezeAction(EntityUid uid, FreezeComponent component, FreezeActionEvent args)
    {
        if (args.Handled)
            return;

        _stun.TryStun(uid, TimeSpan.FromSeconds(5f), false);
        args.Handled = true;
    }
}

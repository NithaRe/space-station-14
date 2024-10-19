using Content.Shared._Orienta.AntiPsychos.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Robust.Client.Player;

namespace Content.Client.Overlays;

public sealed class ShowAntiPsychosIconsSystem : EquipmentHudSystem<ShowAntiPsychosIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntitySystemManager _sysMan = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AntiPsychosComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, AntiPsychosComponent component, ref GetStatusIconsEvent ev)
    {
        //if (!IsActive)
          //  return;

        if (_prototype.TryIndex(component.MindShieldStatusIcon, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
    }
}

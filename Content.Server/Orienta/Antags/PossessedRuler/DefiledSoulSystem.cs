using Content.Server.Body.Systems;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Orienta.Antags.PossessedRuler;
using Content.Shared.Timing;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Popups;

namespace Content.Server.Orienta.Antags.PossessedRuler;

public sealed class DefiledSoulSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public const float MaxFeedDistance = 1.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DefiledSoulComponent, ComponentInit>(ComponentInit);
    }

    private void ComponentInit(EntityUid uid, DefiledSoulComponent comp, ComponentInit args)
    {
        comp.InitTime = _gameTiming.CurTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DefiledSoulComponent>();

        while (query.MoveNext(out var uid, out var component))
        {
            var currentTime = _gameTiming.CurTime;
            var elapsedTime = currentTime - component.InitTime; 

            if (elapsedTime >= component.LifeTime)
            {
                QueueDel(uid);
                continue; 
            }

            if ((elapsedTime.TotalSeconds + 1.0) % 20 < frameTime)
            {
                _popupSystem.PopupEntity(
                    $"До вашей смерти осталось {(component.LifeTime - elapsedTime).Minutes} мин., {(component.LifeTime - elapsedTime).Seconds} сек.",
                    uid,
                    PopupType.Large);
            }
        }
    }
}

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
using Robust.Shared.Utility;

namespace Content.Server.Orienta.Antags.PossessedRuler;

public sealed class PossessingSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly StationSystem _stations = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;

    [Dependency] private readonly ILogManager _logManager = default!;

    public const float MaxFeedDistance = 1.0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PossessedComponent, GetVerbsEvent<Verb>>(AddPossessVerb);
    }

    private void AddPossessVerb(Entity<PossessedComponent> entity, ref GetVerbsEvent<Verb> ev)
    {
        if (entity.Owner == ev.User ||
            !ev.CanInteract ||
            !ev.CanAccess)
            return;

        var user = ev.User;
        Verb verb = new()
        {
            Act = () =>
            {
                TryPossess(user, entity);
            },
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/in.svg.192dpi.png")),
            Text = "Вселиться",
            Priority = 1
        };

        ev.Verbs.Add(verb);
    }

    private bool TryPossess(EntityUid user, Entity<PossessedComponent> entity)
    {
        if (!_protoManager.TryIndex<EntityPrototype>(entity.Comp.EntityPrototype, out var entityProto) || !HasComp<DefiledSoulComponent>(user))
        {
            return false;
        }

        var coords = Transform(entity.Owner).Coordinates;
        var spawnedEntity = Spawn(entityProto.ID, coords);

        _mindSystem.ControlMob(user, spawnedEntity);

        QueueDel(user);
        QueueDel(entity.Owner);

        // log
        _adminLogger.Add(LogType.Ingestion, LogImpact.Low, $"{ToPrettyString(user):target} possessed into {ToPrettyString(spawnedEntity):Possessing}");

        _audio.PlayPvs(entity.Comp.UseSound, user, AudioParams.Default.WithVolume(-1f));

        return true;
    }
}

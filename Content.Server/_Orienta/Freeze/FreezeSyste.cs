using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Shared._Orienta.Freeze;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.IdentityManagement;

namespace Content.Server._Orienta.Freeze;

public sealed class FreezeSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FreezeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FreezeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FreezeComponent, FreezeActionEvent>(OnFreezeAction);
    }

    private void OnMapInit(EntityUid uid, FreezeComponent component, MapInitEvent args)
    {
        // Инициализация
        _actions.AddAction(uid, ref component.FreezeActionEntity, component.FreezeAction);
    }

    private void OnShutdown(EntityUid uid, FreezeComponent component, ComponentShutdown args)
    {
        // Удаление action
        if (component.FreezeActionEntity != null)
        {
            _actions.RemoveAction(uid, component.FreezeActionEntity);
        }
    }

    private void OnFreezeAction(EntityUid selfuid, FreezeComponent component, FreezeActionEvent args)
    {
        if (args.Handled)
            return;

        // Проверки
        if (!_mobState.IsAlive(args.Target))
        {
            _popup.PopupEntity("Данное действие можно применять только к живым существам", selfuid, selfuid, PopupType.MediumCaution);
            return;
        }

        if ((args.Target == selfuid))
        {
            _popup.PopupEntity("Вы не можете захватить самого себя", selfuid, selfuid, PopupType.MediumCaution);
            return;
        }

        // Проверка на корректные условия
        if (_mobState.IsAlive(args.Target))
        {
            // Формирование необходимых переменных
            var targetEntity = Identity.Entity(args.Target, EntityManager);
            var selfEntity = Identity.Entity(selfuid, EntityManager);

            var targetName = targetEntity != null ? Identity.Name(targetEntity, EntityManager) : "Unknown";
            var selfName = selfEntity != null ? Identity.Name(selfEntity, EntityManager) : "Unknown";

            // Сам action
            _stun.TryStun(selfuid, TimeSpan.FromSeconds(5f), false);
            _popup.PopupEntity("Вы остановили существо", selfuid, selfuid, PopupType.Medium);

            _stun.TryStun(args.Target, TimeSpan.FromSeconds(5f), false);
            _popup.PopupEntity("Вас остановили", args.Target, args.Target, PopupType.Medium);

            // Отправляем эмоцию о захвате
            var emoteMessage = $" берёт в захват {targetName}!";
            _chat.TrySendInGameICMessage(
            selfuid,
            emoteMessage,
            InGameICChatType.Emote,
            ChatTransmitRange.Normal,
            hideLog: false,
            player: null,
            nameOverride: selfName
            );

            // Сама активация
            args.Handled = true;
        }
    }
}

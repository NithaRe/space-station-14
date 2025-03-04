using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Content.Server._Orienta.Purchase;
using Content.Server.Actions;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.PDA.Ringer;
using Content.Server.Stack;
using Content.Server.Store.Components;
using Content.Shared.Actions;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Store.Systems;

public sealed partial class StoreSystem
{
    [Dependency] private readonly IAdminLogManager _admin = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly ActionUpgradeSystem _actionUpgrade = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly RequestForBuy _purchaseService = default!;

    private void InitializeUi()
    {
        SubscribeLocalEvent<StoreComponent, StoreRequestUpdateInterfaceMessage>(OnRequestUpdate);
        SubscribeLocalEvent<StoreComponent, StoreBuyListingMessage>(OnBuyRequest);
        SubscribeLocalEvent<StoreComponent, StoreRequestWithdrawMessage>(OnRequestWithdraw);
        SubscribeLocalEvent<StoreComponent, StoreRequestRefundMessage>(OnRequestRefund);
        SubscribeLocalEvent<StoreComponent, RefundEntityDeletedEvent>(OnRefundEntityDeleted);
    }

    private void OnRefundEntityDeleted(Entity<StoreComponent> ent, ref RefundEntityDeletedEvent args)
    {
        ent.Comp.BoughtEntities.Remove(args.Uid);
    }

    /// <summary>
    /// Toggles the store Ui open and closed
    /// </summary>
    /// <param name="user">the person doing the toggling</param>
    /// <param name="storeEnt">the store being toggled</param>
    /// <param name="component"></param>
    public void ToggleUi(EntityUid user, EntityUid storeEnt, StoreComponent? component = null)
    {
        if (!Resolve(storeEnt, ref component))
            return;

        if (!TryComp<ActorComponent>(user, out var actor))
            return;

        if (!_ui.TryToggleUi(storeEnt, StoreUiKey.Key, actor.PlayerSession))
            return;

        UpdateUserInterface(user, storeEnt, component);
    }

    /// <summary>
    /// Closes the store UI for everyone, if it's open
    /// </summary>
    public void CloseUi(EntityUid uid, StoreComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        _ui.CloseUi(uid, StoreUiKey.Key);
    }

    /// <summary>
    /// Updates the user interface for a store and refreshes the listings
    /// </summary>
    /// <param name="user">The person who if opening the store ui. Listings are filtered based on this.</param>
    /// <param name="store">The store entity itself</param>
    /// <param name="component">The store component being refreshed.</param>
    /// 
    /// PR 63 Покупка за ориентики. Модификация офовского метода
    public async void UpdateUserInterface(EntityUid? user, EntityUid store, StoreComponent? component = null)
    {
        if (!Resolve(store, ref component))
            return;

        if (user != null)
        {
            component.LastAvailableListings = GetAvailableListings(component.AccountOwner ?? user.Value, store, component)
                .ToHashSet();
        }

        Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> allCurrency = new();
        foreach (var supported in component.CurrencyWhitelist)
        {
            allCurrency.Add(supported, FixedPoint2.Zero);

            // Для обычных магазинов или валют, кроме "Orientik", берем баланс из component.Balance
            if (supported != "Orientik" || !HasComp<OrientaLinkComponent>(store))
            {
                if (component.Balance.TryGetValue(supported, out var value))
                    allCurrency[supported] = value;
            }
        }

        // Для Ориенталинка запрашиваем фиктивный баланс "Orientik" через API
        if (HasComp<OrientaLinkComponent>(store) && user != null && component.CurrencyWhitelist.Contains("Orientik") && TryComp<ActorComponent>(user.Value, out var actor))
        {
            var ckey = actor.PlayerSession.Name;
            var (success, _, balance) = await _purchaseService.GetBalance(ckey);
            allCurrency["Orientik"] = success ? FixedPoint2.New(balance) : FixedPoint2.Zero;
        }

        var showFooter = HasComp<RingerUplinkComponent>(store);
        var state = new StoreUpdateState(component.LastAvailableListings, allCurrency, showFooter, component.RefundAllowed);
        _ui.SetUiState(store, StoreUiKey.Key, state);
    }

    private void OnRequestUpdate(EntityUid uid, StoreComponent component, StoreRequestUpdateInterfaceMessage args)
    {
        UpdateUserInterface(args.Actor, GetEntity(args.Entity), component);
    }

    private void BeforeActivatableUiOpen(EntityUid uid, StoreComponent component, BeforeActivatableUIOpenEvent args)
    {
        UpdateUserInterface(args.User, uid, component);
    }

    /// <summary>
    /// Handles whenever a purchase was made.
    /// </summary>
    /// PR 63 Покупка за ориентики. Модификация офовского метода
    private void OnBuyRequest(EntityUid uid, StoreComponent component, StoreBuyListingMessage msg)
    {
        _ = ProcessPurchaseAsync(uid, component, msg);
    }

    // PR 63 Покупка за ориентики. Таск для покупок
    private async Task ProcessPurchaseAsync(EntityUid uid, StoreComponent component, StoreBuyListingMessage msg)
    {
        var listing = component.FullListingsCatalog.FirstOrDefault(x => x.ID.Equals(msg.Listing.Id));
        if (listing == null)
        {
            Log.Debug("listing does not exist");
            return;
        }

        if (listing.Cost.ContainsKey("Orientik"))
        {
            await HandleOrientikPurchaseAsync(uid, component, msg);
            return;
        }

        // Оригинальная синхронная логика для стандартных валют
        var buyer = msg.Actor;

        if (!ListingHasCategory(listing, component.Categories))
            return;

        if (listing.Conditions != null)
        {
            var args = new ListingConditionArgs(component.AccountOwner ?? buyer, uid, listing, EntityManager);
            var conditionsMet = listing.Conditions.All(condition => condition.Condition(args));
            if (!conditionsMet)
                return;
        }

        var cost = listing.Cost;
        foreach (var (currency, amount) in cost)
        {
            if (!component.Balance.TryGetValue(currency, out var balance) || balance < amount)
            {
                return;
            }
        }

        if (!IsOnStartingMap(uid, component))
            DisableRefund(uid, component);

        foreach (var (currency, amount) in cost)
        {
            component.Balance[currency] -= amount;
            component.BalanceSpent.TryAdd(currency, FixedPoint2.Zero);
            component.BalanceSpent[currency] += amount;
        }

        if (listing.ProductEntity != null)
        {
            var product = Spawn(listing.ProductEntity, Transform(buyer).Coordinates);
            _hands.PickupOrDrop(buyer, product);
            HandleRefundComp(uid, component, product);

            var xForm = Transform(product);
            if (xForm.ChildCount > 0)
            {
                var childEnumerator = xForm.ChildEnumerator;
                while (childEnumerator.MoveNext(out var child))
                {
                    component.BoughtEntities.Add(child);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(listing.ProductAction))
        {
            EntityUid? actionId;
            if (!_mind.TryGetMind(buyer, out var mind, out _))
                actionId = _actions.AddAction(buyer, listing.ProductAction);
            else
                actionId = _actionContainer.AddAction(mind, listing.ProductAction);

            if (actionId != null)
            {
                HandleRefundComp(uid, component, actionId.Value);
                if (listing.ProductUpgradeId != null)
                {
                    foreach (var upgradeListing in component.FullListingsCatalog)
                    {
                        if (upgradeListing.ID == listing.ProductUpgradeId)
                        {
                            upgradeListing.ProductActionEntity = actionId.Value;
                            break;
                        }
                    }
                }
            }
        }

        if (listing is { ProductUpgradeId: not null, ProductActionEntity: not null })
        {
            if (listing.ProductActionEntity != null)
            {
                component.BoughtEntities.Remove(listing.ProductActionEntity.Value);
            }
            if (!_actionUpgrade.TryUpgradeAction(listing.ProductActionEntity, out var upgradeActionId))
            {
                if (listing.ProductActionEntity != null)
                    HandleRefundComp(uid, component, listing.ProductActionEntity.Value);
                return;
            }
            listing.ProductActionEntity = upgradeActionId;
            if (upgradeActionId != null)
                HandleRefundComp(uid, component, upgradeActionId.Value);
        }

        if (listing.ProductEvent != null)
        {
            if (!listing.RaiseProductEventOnUser)
                RaiseLocalEvent(listing.ProductEvent);
            else
                RaiseLocalEvent(buyer, listing.ProductEvent);
        }

        if (listing.DisableRefund)
        {
            component.RefundAllowed = false;
        }

        _admin.Add(LogType.StorePurchase, LogImpact.Low,
            $"{ToPrettyString(buyer):player} purchased listing \"{ListingLocalisationHelpers.GetLocalisedNameOrEntityName(listing, _proto)}\" from {ToPrettyString(uid)}");

        listing.PurchaseAmount++;
        _audio.PlayEntity(component.BuySuccessSound, msg.Actor, uid);

        var buyFinished = new StoreBuyFinishedEvent
        {
            PurchasedItem = listing,
            StoreUid = uid
        };
        RaiseLocalEvent(ref buyFinished);

        UpdateUserInterface(buyer, uid, component);
    }

    // PR 63 Покупка за ориентики. Таск для покупок
    private async Task HandleOrientikPurchaseAsync(EntityUid uid, StoreComponent component, StoreBuyListingMessage msg)
    {
        var listing = component.FullListingsCatalog.FirstOrDefault(x => x.ID.Equals(msg.Listing.Id));
        if (listing == null || !listing.Cost.TryGetValue("Orientik", out var price))
            return;

        if (!TryComp<ActorComponent>(msg.Actor, out var actorComp))
        {
            // Log.Info($"[StoreSystem] Не удалось определить игрока {msg.Actor}");
            return;
        }

        var ckey = actorComp.PlayerSession.Name;
        var (success, message, _) = await _purchaseService.SendPurchaseRequest(ckey, (int)price);

        if (!success)
        {
            // Log.Warning($"[StoreSystem] Ошибка покупки для {msg.Actor}: {message}");
            return;
        }

        if (listing.ProductEntity != null)
        {
            var product = Spawn(listing.ProductEntity, Transform(msg.Actor).Coordinates);
            _hands.PickupOrDrop(msg.Actor, product);
        }

        _audio.PlayEntity(component.BuySuccessSound, msg.Actor, uid);
        UpdateUserInterface(msg.Actor, uid, component);
    }

    /// <summary>
    /// Handles dispensing the currency you requested to be withdrawn.
    /// </summary>
    /// <remarks>
    /// This would need to be done should a currency with decimal values need to use it.
    /// not quite sure how to handle that
    /// </remarks>
    private void OnRequestWithdraw(EntityUid uid, StoreComponent component, StoreRequestWithdrawMessage msg)
    {
        if (msg.Amount <= 0)
            return;

        //make sure we have enough cash in the bank and we actually support this currency
        if (!component.Balance.TryGetValue(msg.Currency, out var currentAmount) || currentAmount < msg.Amount)
            return;

        //make sure a malicious client didn't send us random shit
        if (!_proto.TryIndex<CurrencyPrototype>(msg.Currency, out var proto))
            return;

        //we need an actually valid entity to spawn. This check has been done earlier, but just in case.
        if (proto.Cash == null || !proto.CanWithdraw)
            return;

        var buyer = msg.Actor;

        FixedPoint2 amountRemaining = msg.Amount;
        var coordinates = Transform(buyer).Coordinates;

        var sortedCashValues = proto.Cash.Keys.OrderByDescending(x => x).ToList();
        foreach (var value in sortedCashValues)
        {
            var cashId = proto.Cash[value];
            var amountToSpawn = (int) MathF.Floor((float) (amountRemaining / value));
            var ents = _stack.SpawnMultiple(cashId, amountToSpawn, coordinates);
            if (ents.FirstOrDefault() is {} ent)
                _hands.PickupOrDrop(buyer, ent);
            amountRemaining -= value * amountToSpawn;
        }

        component.Balance[msg.Currency] -= msg.Amount;
        UpdateUserInterface(buyer, uid, component);
    }

    private void OnRequestRefund(EntityUid uid, StoreComponent component, StoreRequestRefundMessage args)
    {
        // TODO: Remove guardian/holopara

        if (args.Actor is not { Valid: true } buyer)
            return;

        if (!IsOnStartingMap(uid, component))
        {
            component.RefundAllowed = false;
            UpdateUserInterface(buyer, uid, component);
        }

        if (!component.RefundAllowed || component.BoughtEntities.Count == 0)
            return;

        _admin.Add(LogType.StoreRefund, LogImpact.Low, $"{ToPrettyString(buyer):player} has refunded their purchases from {ToPrettyString(uid):store}");

        for (var i = component.BoughtEntities.Count - 1; i >= 0; i--)
        {
            var purchase = component.BoughtEntities[i];

            if (!Exists(purchase))
                continue;

            component.BoughtEntities.RemoveAt(i);

            if (_actions.TryGetActionData(purchase, out var actionComponent, logError: false))
            {
                _actionContainer.RemoveAction(purchase, actionComponent);
            }

            EntityManager.DeleteEntity(purchase);
        }

        component.BoughtEntities.Clear();

        foreach (var (currency, value) in component.BalanceSpent)
        {
            component.Balance[currency] += value;
        }

        // Reset store back to its original state
        RefreshAllListings(component);
        component.BalanceSpent = new();
        UpdateUserInterface(buyer, uid, component);
    }

    private void HandleRefundComp(EntityUid uid, StoreComponent component, EntityUid purchase)
    {
        component.BoughtEntities.Add(purchase);
        var refundComp = EnsureComp<StoreRefundComponent>(purchase);
        refundComp.StoreEntity = uid;
    }

    private bool IsOnStartingMap(EntityUid store, StoreComponent component)
    {
        var xform = Transform(store);
        return component.StartingMap == xform.MapUid;
    }

    /// <summary>
    ///     Disables refunds for this store
    /// </summary>
    public void DisableRefund(EntityUid store, StoreComponent? component = null)
    {
        if (!Resolve(store, ref component))
            return;

        component.RefundAllowed = false;
    }
}

/// <summary>
/// Event of successfully finishing purchase in store (<see cref="StoreSystem"/>.
/// </summary>
/// <param name="StoreUid">EntityUid on which store is placed.</param>
/// <param name="PurchasedItem">ListingItem that was purchased.</param>
[ByRefEvent]
public readonly record struct StoreBuyFinishedEvent(
    EntityUid StoreUid,
    ListingDataWithCostModifiers PurchasedItem
);

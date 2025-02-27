using System.Threading.Tasks;
using Content.Shared.FixedPoint;
using Content.Shared._Orienta.Purchase;
using Content.Shared.PDA;
using Content.Shared.Store;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Audio;

namespace Content.Server._Orienta.Purchase
{
    public sealed partial class OrientikStoreSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly RequestForBuy _purchaseService = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly UserInterfaceSystem _ui = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PdaComponent, BoundUIOpenedEvent>(OnUiOpened);
            SubscribeLocalEvent<PdaComponent, OrientikBuyListingMessage>(OnBuyRequest);
        }

        private void OnUiOpened(EntityUid uid, PdaComponent component, BoundUIOpenedEvent args)
        {
            if (!TryComp<ActorComponent>(uid, out var actorComp) || actorComp.PlayerSession == null)
                return;

            Task.Run(async () =>
            {
                await HandleUiOpenedAsync(uid, component, actorComp.PlayerSession);
            });
        }

        private async Task HandleUiOpenedAsync(EntityUid uid, PdaComponent component, ICommonSession session)
        {
            var ckey = session.Name;
            var balanceResult = await _purchaseService.GetBalance(ckey);
            var (success, _, balance) = balanceResult;
            if (!success)
                return;

            var balanceDict = new Dictionary<string, FixedPoint2> { { "Orientik", FixedPoint2.New(balance) } };
            _ui.SetUiState(uid, OrientikStoreUiKey.Key, new OrientikStoreUiState(GetListings(), balanceDict));
        }

        private void OnBuyRequest(EntityUid uid, PdaComponent component, OrientikBuyListingMessage args)
        {
            if (!TryComp<ActorComponent>(uid, out var actorComp) || actorComp.PlayerSession == null)
                return;

            Task.Run(async () =>
            {
                await HandleBuyRequestAsync(uid, component, args, actorComp.PlayerSession);
            });
        }

        private async Task HandleBuyRequestAsync(EntityUid uid, PdaComponent component, OrientikBuyListingMessage args, ICommonSession session)
        {
            var listing = _proto.Index<ListingPrototype>(args.ListingId);
            if (listing == null || !listing.Cost.ContainsKey("Orientik"))
                return;

            var ckey = session.Name;
            var balanceResult = await _purchaseService.GetBalance(ckey);
            var (balanceSuccess, _, balance) = balanceResult;
            if (!balanceSuccess)
                return;

            var balanceDict = new Dictionary<string, FixedPoint2> { { "Orientik", FixedPoint2.New(balance) } };
            var price = listing.Cost["Orientik"].Int();
            if (balance < price)
            {
                _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Effects/error.ogg"), session);
                return;
            }

            var purchaseResult = await _purchaseService.SendPurchaseRequest(ckey, price);
            var (success, message, newBalance) = purchaseResult;
            if (!success)
            {
                Log.Info($"[OrientikStore] Ошибка покупки для {ckey}: {message}");
                _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Effects/error.ogg"), session);
                return;
            }

            balanceDict["Orientik"] = FixedPoint2.New(newBalance);
            if (listing.ProductEntity != null)
            {
                var product = Spawn(listing.ProductEntity, Transform(session.AttachedEntity!.Value).Coordinates);
                _hands.TryPickupAnyHand(session.AttachedEntity.Value, product);
            }

            _audio.PlayGlobal(new SoundPathSpecifier("/Audio/Effects/success.ogg"), session);
            _ui.SetUiState(uid, OrientikStoreUiKey.Key, new OrientikStoreUiState(GetListings(), balanceDict));
        }

        private IReadOnlyList<ListingData> GetListings()
        {
            var listings = new List<ListingData>();
            foreach (var listing in _proto.EnumeratePrototypes<ListingPrototype>())
            {
                if (listing.Cost.ContainsKey("Orientik"))
                    listings.Add(new ListingData(listing));
            }
            return listings;
        }
    }
}

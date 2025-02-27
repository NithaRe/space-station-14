using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared._Orienta.Purchase;
using Content.Shared.Store;
using Robust.Client.UserInterface;

namespace Content.Client._Orienta.Purchase
{
    public sealed class OrientikStoreBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        private OrientikStoreMenu? _menu;

        public OrientikStoreBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            IoCManager.InjectDependencies(this);
        }

        protected override void Open()
        {
            base.Open();
            _menu = this.CreateWindow<OrientikStoreMenu>(); // Используем CreateWindow из расширения
            _menu.OnClose += Close;
            _menu.OnListingButtonPressed += item =>
            {
                if (item.Metadata is ListingPrototype listingProto)
                    SendMessage(new OrientikBuyListingMessage(listingProto.ID));
            };
            _menu.OpenCentered();
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);
            if (state is not OrientikStoreUiState cast)
                return;

            _menu?.Populate(cast.Listings, cast.Balance);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            _menu?.Dispose();
        }
    }
}

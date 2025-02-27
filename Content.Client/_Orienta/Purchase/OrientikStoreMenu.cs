using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;
using Content.Shared.Store;
using Content.Shared.FixedPoint;
using System.Numerics;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._Orienta.Purchase
{
    public sealed class OrientikStoreMenu : BaseWindow
    {
        [Dependency] private readonly IPrototypeManager _proto = default!;
        private readonly ItemList _listings;
        private readonly Label _balanceLabel;

        public event Action<ItemList.Item>? OnListingButtonPressed;
        public event Action? OnClose;

        public OrientikStoreMenu() // Конструктор без аргументов
        {
            IoCManager.InjectDependencies(this);
            MinSize = new Vector2(300, 400);

            var container = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                VerticalExpand = true
            };
            AddChild(container);

            _balanceLabel = new Label { Text = "Баланс: 0 Orientik" };
            container.AddChild(_balanceLabel);

            _listings = new ItemList
            {
                VerticalExpand = true
            };
            container.AddChild(_listings);

            var closeButton = new Button { Text = "Закрыть" };
            closeButton.OnPressed += _ => OnClose?.Invoke();
            container.AddChild(closeButton);
        }

        public void Populate(IReadOnlyList<ListingData> listings, IReadOnlyDictionary<string, FixedPoint2> balance)
        {
            _listings.Clear();
            foreach (var listing in listings)
            {
                if (!_proto.TryIndex<ListingPrototype>(listing.ID, out var protoListing) ||
                    !protoListing.Cost.ContainsKey("Orientik"))
                    continue;

                var item = new ItemList.Item(_listings)
                {
                    Text = $"{protoListing.Name} ({protoListing.Cost["Orientik"]} Orientik)",
                    Metadata = protoListing
                };
                _listings.Add(item);
            }
            _listings.OnItemSelected += args =>
            {
                var selectedItem = _listings[args.ItemIndex];
                OnListingButtonPressed?.Invoke(selectedItem);
            };

            _balanceLabel.Text = $"Баланс: {balance["Orientik"]} Orientik";
        }
    }
}

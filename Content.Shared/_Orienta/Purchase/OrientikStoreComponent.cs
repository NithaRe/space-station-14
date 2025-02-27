using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Orienta.Purchase
{
    [Serializable, NetSerializable]
    public sealed class OrientikBuyListingMessage : BoundUserInterfaceMessage
    {
        public readonly string ListingId;
        public OrientikBuyListingMessage(string listingId)
        {
            ListingId = listingId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class OrientikStoreUiState : BoundUserInterfaceState
    {
        public IReadOnlyList<ListingData> Listings { get; }
        public IReadOnlyDictionary<string, FixedPoint2> Balance { get; }

        public OrientikStoreUiState(IReadOnlyList<ListingData> listings, IReadOnlyDictionary<string, FixedPoint2> balance)
        {
            Listings = listings;
            Balance = balance;
        }
    }

    [Serializable, NetSerializable]
    public enum OrientikStoreUiKey
    {
        Key
    }

    [Serializable, NetSerializable]
    public sealed class PdaShowOrientikStoreMessage : BoundUserInterfaceMessage
    {
        public readonly int ActorNetId; // Используем NetEntity вместо EntityUid

        public PdaShowOrientikStoreMessage(int actorNetId)
        {
            ActorNetId = actorNetId;
        }
    }
}

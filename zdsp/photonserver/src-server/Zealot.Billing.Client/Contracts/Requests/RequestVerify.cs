// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// receiver. They will always end up empty. Use List instead.
namespace Zealot.Billing.Contracts.Requests
{
    using Enums;
    using System;

    public class RequestVerify
    {
        public MerchantType merchantType { get; set; }

        public string transactionId { get; set; }

        public string productId { get; set; }

        public string receipt { get; set; }

        public Guid charId { get; set; }

        public string serverId { get; set; }

        // Used by WeGames.
        //public string auxiliaryId { get; set; }
        //public int guestId { get; set; }
        //public string charName { get; set; }
        //public string originServerId { get; set; }
    }
}

// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// receiver. They will always end up empty. Use List instead.
namespace Zealot.Billing.Contracts.Responses
{
    using Enums;
    using System;

    public class ResponseClaim
    {
        public Guid charId { get; set; }

        public Guid billingId { get; set; }

        public string serverId { get; set; }

        public string productId { get; set; }

        public ClaimStatus status { get; set; }

        public int currGame { get; set; }
    }
}

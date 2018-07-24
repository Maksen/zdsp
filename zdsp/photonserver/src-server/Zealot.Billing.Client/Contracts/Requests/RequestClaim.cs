// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// receiver. They will always end up empty. Use List instead.
namespace Zealot.Billing.Contracts.Requests
{
    using System;

    public class RequestClaim
    {
        public Guid charId { get; set; }

        public Guid billingId { get; set; }

        public string serverId { get; set; }
    }
}

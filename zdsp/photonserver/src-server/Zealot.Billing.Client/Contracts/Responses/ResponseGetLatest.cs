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
    using System.Collections.Generic;

    public class ResponseGetLatest
    {
        public Guid charId { get; set; }

        public string serverId { get; set; }

        public GetLatestStatus status { get; set; }

        public List<BillingRecord> billingRecords { get; set; }

        public class BillingRecord
        {
            public string productId { get; set; }

            public DateTime purchasedDate { get; set; }
        }
    }
}

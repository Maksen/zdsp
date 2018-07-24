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

    public class ResponseVerify
    {
        public Guid charId { get; set; }

        public string serverId { get; set; }

        public string transactionId { get; set; }

        public MerchantType merchantType { get; set; }

        public VerifyStatus status { get; set; }

        public Guid billingId { get; set; }
    }
}
// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.
namespace Zealot.Billing.Enums
{
    public enum VerifyStatus : byte
    {
        Pass,
        InvalidId,
        AlreadyExists,
        MerchantInvalid,
        MerchantUnreachable,
        DBError,
        Offline
    }
}

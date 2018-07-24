// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.
namespace Zealot.Billing.Enums
{
    // IMPORTANT!
    // This Enum is used in both GameServer and GameClient.
    // GameClient declared at TopUpController.cs.
    public enum MerchantType : byte
    {
        Apple,
        Google
    }
}

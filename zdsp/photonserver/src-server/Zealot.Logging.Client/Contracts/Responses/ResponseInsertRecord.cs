// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// logging server. They will always end up empty. Use List instead.
namespace Zealot.Logging.Contracts.Requests
{
    public class ResponseInsertRecord
    {
        public bool isSuccessful { get; set; }
    }
}
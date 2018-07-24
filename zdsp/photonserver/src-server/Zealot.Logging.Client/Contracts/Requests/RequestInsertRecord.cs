// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// logging server. They will always end up empty. Use List instead.
namespace Zealot.Logging.Contracts.Requests
{
    using System;
    using System.Collections.Generic;

    public class RequestInsertRecord
    {
        // base_logs
        public DateTime dateTime { get; set; }

        public string serverId { get; set; }

        //public string userId { get; set; }
        public Guid userId { get; set; }

        //public string charId { get; set; }
        public Guid charId { get; set; }

        public string logName { get; set; }

        public string message { get; set; }

        // custom_logs
        public List<TypeCode> logFieldTypes { get; set; }
        
        public List<object> logFieldValues { get; set; }
    }
}

// WARNING!
// Synchronization required. Both server and client/s are to have 
// identical copies of this contract to serialize/deserialize.

// WARNING!
// Dictionary data types do not get deserialized properly at the
// logging server. They will always end up empty. Use List instead.
namespace Zealot.Logging.Contracts.Requests
{
    using System.Collections.Generic;

    public class RequestUpdateTables
    {
        public string serverId { get; set; }

        public List<TableData> tables { get; set; }
        
        public class TableData
        {
            public string tableName { get; set; }

            public List<string> columnName { get; set; }

            public List<string> columnType { get; set; }
        }
    }
}

namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class Redemption : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string redemptionId { get; set; }

        public string mailName { get; set; }

        public int itemId1 { get; set; }

        public int itemStackCount1 { get; set; }

        public int itemId2 { get; set; }

        public int itemStackCount2 { get; set; }

        public int itemId3 { get; set; }

        public int itemStackCount3 { get; set; }

        public int itemId4 { get; set; }

        public int itemStackCount4 { get; set; }

        public int itemId5 { get; set; }

        public int itemStackCount5 { get; set; }

        public int itemId6 { get; set; }

        public int itemStackCount6 { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            // Reminder: Change the class type identical to this class.
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptions and/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                redemptionId.GetTypeCode(),
                mailName.GetTypeCode(),
                itemId1.GetTypeCode(),
                itemStackCount1.GetTypeCode(),
                itemId2.GetTypeCode(),
                itemStackCount2.GetTypeCode(),
                itemId3.GetTypeCode(),
                itemStackCount3.GetTypeCode(),
                itemId4.GetTypeCode(),
                itemStackCount4.GetTypeCode(),
                itemId5.GetTypeCode(),
                itemStackCount5.GetTypeCode(),
                itemId6.GetTypeCode(),
                itemStackCount6.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                redemptionId,
                mailName,
                itemId1,
                itemStackCount1,
                itemId2,
                itemStackCount2,
                itemId3,
                itemStackCount3,
                itemId4,
                itemStackCount4,
                itemId5,
                itemStackCount5,
                itemId6,
                itemStackCount6
            };

            return requestInsertRecord;
        }
    }
}

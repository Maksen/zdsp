namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class RandomBoxReward : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int BoxId { get; set; }

        public int ItemId1 { get; set; }

        public int ItemCount1 { get; set; }

        public int ItemId2 { get; set; }

        public int ItemCount2 { get; set; }

        public int ItemId3 { get; set; }

        public int ItemCount3 { get; set; }

        public int ItemId4 { get; set; }

        public int ItemCount4 { get; set; }

        public int ItemId5 { get; set; }

        public int ItemCount5 { get; set; }

        public int ItemId6 { get; set; }

        public int ItemCount6 { get; set; }

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
                BoxId.GetTypeCode(),
                ItemId1.GetTypeCode(),
                ItemCount1.GetTypeCode(),
                ItemId2.GetTypeCode(),
                ItemCount2.GetTypeCode(),
                ItemId3.GetTypeCode(),
                ItemCount3.GetTypeCode(),
                ItemId4.GetTypeCode(),
                ItemCount4.GetTypeCode(),
                ItemId5.GetTypeCode(),
                ItemCount5.GetTypeCode(),
                ItemId6.GetTypeCode(),
                ItemCount6.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                BoxId,
                ItemId1,
                ItemCount1,
                ItemId2,
                ItemCount2,
                ItemId3,
                ItemCount3,
                ItemId4,
                ItemCount4,
                ItemId5,
                ItemCount5,
                ItemId6,
                ItemCount6
            };

            return requestInsertRecord;
        }
    }

    public class RandomBoxReward2 : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int BoxId { get; set; }

        public int ItemId { get; set; }

        public int ItemCount { get; set; }

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
                BoxId.GetTypeCode(),
                ItemId.GetTypeCode(),
                ItemCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                BoxId,
                ItemId,
                ItemCount
            };

            return requestInsertRecord;
        }
    }
}
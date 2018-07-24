namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class AuctionItem : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }
        public int itemId { get; set; }
        public int itemCount { get; set; }
        public string playerName { get; set; }

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
                actionType.GetTypeCode(),
                itemId.GetTypeCode(),
                itemCount.GetTypeCode(),
                playerName.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                itemId,
                itemCount,
                playerName
            };

            return requestInsertRecord;
        }
    }

    public class AuctionCurrency : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }

        public int itemId { get; set; }

        public int lockGold { get; set; }

        public int gold { get; set; }

        public int lockGoldBefore { get; set; }

        public int lockGoldAfter { get; set; }

        public int goldBefore { get; set; }

        public int goldAfter { get; set; }

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
                actionType.GetTypeCode(),
                itemId.GetTypeCode(),
                lockGold.GetTypeCode(),
                gold.GetTypeCode(),
                lockGoldBefore.GetTypeCode(),
                lockGoldAfter.GetTypeCode(),
                goldBefore.GetTypeCode(),
                goldAfter.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                itemId,
                lockGold,
                gold,
                lockGoldBefore,
                lockGoldAfter,
                goldBefore,
                goldAfter
            };

            return requestInsertRecord;
        }
    }
}
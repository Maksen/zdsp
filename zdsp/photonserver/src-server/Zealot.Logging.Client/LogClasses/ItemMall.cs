namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class ItemMall : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string itemCategory { get; set; }
        public int itemId { get; set; }
        public int quantityBefore { get; set; }
        public int quantityPurchased { get; set; }
        public int quantityAfter { get; set; }
        public int bindGoldBefore { get; set; }
        public int bindGoldAfter { get; set; }
        public int goldBefore { get; set; }
        public int goldAfter { get; set; }
        public int totalCost { get; set; }

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
                itemCategory.GetTypeCode(),
                itemId.GetTypeCode(),
                quantityBefore.GetTypeCode(),
                quantityPurchased.GetTypeCode(),
                quantityAfter.GetTypeCode(),
                bindGoldBefore.GetTypeCode(),
                bindGoldAfter.GetTypeCode(),
                goldBefore.GetTypeCode(),
                goldAfter.GetTypeCode(),
                totalCost.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemCategory,
                itemId,
                quantityBefore,
                quantityPurchased,
                quantityAfter,
                bindGoldBefore,
                bindGoldAfter,
                goldBefore,
                goldAfter,
                totalCost,
            };

            return requestInsertRecord;
        }
    }
}
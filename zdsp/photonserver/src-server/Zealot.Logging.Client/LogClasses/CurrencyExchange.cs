namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class CurrencyExchange : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        
        public int goldBefore { get; set; }
        public int goldAfter { get; set; }
        public int bindGoldBefore { get; set; }
        public int bindGoldAfter { get; set; }
        public int moneyBefore { get; set; }
        public int moneyAfter { get; set; }
        public int moneyGained { get; set; }
        public int moneyOriginal { get; set; }
        public int multiplier { get; set; }
        public int reqGold { get; set; }

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
                goldBefore.GetTypeCode(),
                goldAfter.GetTypeCode(),
                bindGoldBefore.GetTypeCode(),
                bindGoldAfter.GetTypeCode(),
                moneyBefore.GetTypeCode(),
                moneyAfter.GetTypeCode(),
                moneyGained.GetTypeCode(),
                moneyOriginal.GetTypeCode(),
                multiplier.GetTypeCode(),
                reqGold.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
               goldBefore,
               goldAfter,
               bindGoldBefore,
               bindGoldAfter,
               moneyBefore,
               moneyAfter,
               moneyGained,
               moneyOriginal,
               multiplier,
               reqGold,
            };

            return requestInsertRecord;
        }
    }
}
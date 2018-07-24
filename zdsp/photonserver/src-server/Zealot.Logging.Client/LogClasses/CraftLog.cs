using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Logging.Contracts.Requests;

namespace Zealot.Logging.Client.LogClasses
{
    public class CraftLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int CraftedItemId { get; set; }
        public int CraftedItemCount { get; set; }
        public int BeforeCraftedItemCount { get; set; }
        public int AfterCraftedItemCount { get; set; }
        public string AllUsedItemId { get; set; }
        public string AllUsedItemCount { get; set; }
        public string BeforeAllUsedItemCount { get; set; }
        public string AfterAllUsedItemCount { get; set; }
        public int MoneyUsed { get; set; }
        public int BeforeCraftedMoney { get; set; }
        public int AfterCraftedMoney { get; set; }
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
                CraftedItemId.GetTypeCode(),
                CraftedItemCount.GetTypeCode(),
                BeforeCraftedItemCount.GetTypeCode(),
                AfterCraftedItemCount.GetTypeCode(),
                AllUsedItemId.GetTypeCode(),
                AllUsedItemCount.GetTypeCode(),
                BeforeAllUsedItemCount.GetTypeCode(),
                AfterAllUsedItemCount.GetTypeCode(),
                MoneyUsed.GetTypeCode(),
                BeforeCraftedMoney.GetTypeCode(),
                AfterCraftedMoney.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                CraftedItemId,
                CraftedItemCount,
                BeforeCraftedItemCount,
                AfterCraftedItemCount,
                AllUsedItemId,
                AllUsedItemCount,
                BeforeAllUsedItemCount,
                AfterAllUsedItemCount,
                MoneyUsed,
                BeforeCraftedMoney,
                AfterCraftedMoney
            };

            return requestInsertRecord;
        }
    }


}

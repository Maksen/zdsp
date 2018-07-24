namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class InventorySlot : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string inventoryType { get; set; }

        public int diamondCost { get; set; }

        public int oldOpenSlots { get; set; }

        public int newOpenSlots { get; set; }

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
                inventoryType.GetTypeCode(),
                diamondCost.GetTypeCode(),
                oldOpenSlots.GetTypeCode(),
                newOpenSlots.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                inventoryType,
                diamondCost,
                oldOpenSlots,
                newOpenSlots
            };

            return requestInsertRecord;
        }
    }
}
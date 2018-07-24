namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class CurrencyChange : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string source { get; set; }
        public byte currency { get; set; } //currency type
        public int amt { get; set; } // amt > 0 increment, amt < 0 deduction
        public int after { get; set; } // value after changes.

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
                source.GetTypeCode(),
                currency.GetTypeCode(),
                amt.GetTypeCode(),
                after.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                source,
                currency,
                amt,
                after
            };

            return requestInsertRecord;
        }
    }
}
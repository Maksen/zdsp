namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class TongbaoBuffCost : LogClass
    {
        public int tbid { get; set; }

        public int addGold { get; set; }

        public int totalGold { get; set; }

        public bool isArrived { get; set; }


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
                tbid.GetTypeCode(),
                addGold.GetTypeCode(),
                totalGold.GetTypeCode(),
                isArrived.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                tbid,
                addGold,
                totalGold,
                isArrived
            };

            return requestInsertRecord;
        }
    }
}
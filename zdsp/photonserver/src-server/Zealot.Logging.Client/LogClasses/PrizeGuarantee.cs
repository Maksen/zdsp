namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class PrizeGuaranteeItemUse : LogClass
    {
        public int countID { get; set; }

        public int type { get; set; }

        public int itemID { get; set; }

        public int beforeCount { get; set; }

        public int afterCount { get; set; }

         public override RequestInsertRecord GetRequestInsertRecord()
        {
            // Reminder: Change the class type identical to this class.
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptionsand/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                countID.GetTypeCode(),
                type.GetTypeCode(),
                itemID.GetTypeCode(),
                beforeCount.GetTypeCode(),
                afterCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                countID,
                type,
                itemID,
                beforeCount,
                afterCount,
            };
            return requestInsertRecord;
        }
    }

    public class PrizeGuaranteeCount : LogClass
    {
        public int countID { get; set; }
        public int count { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            // Reminder: Change the class type identical to this class.
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptionsand/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                countID.GetTypeCode(),
                count.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                countID,
                count,
            };
            return requestInsertRecord;
        }
    }

    public class PrizeGuaranteeGiveAward : LogClass
    {
        public int countID { get; set; }
        public int beforeCount { get; set; }
        public int afterCount { get; set; }

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
                countID.GetTypeCode(),
                beforeCount.GetTypeCode(),
                afterCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                countID,
                beforeCount,
                afterCount,
            };
            return requestInsertRecord;
        }
    }
}

   

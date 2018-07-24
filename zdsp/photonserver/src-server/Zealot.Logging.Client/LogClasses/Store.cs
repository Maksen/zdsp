namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class StoreBuy : LogClass
    {
        public DateTime recvDate { get; set; }
        public int storeOrder { get; set; }
        public int shelveNo { get; set; }
        public int itemID { get; set; }
        public int itemCount { get; set; }
        public string currencyType { get; set; }
        public int itemPrice { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptions and/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                recvDate.GetTypeCode(),
                storeOrder.GetTypeCode(),
                shelveNo.GetTypeCode(),
                itemID.GetTypeCode(),
                itemCount.GetTypeCode(),
                currencyType.GetTypeCode(),
                itemPrice.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                recvDate,
                storeOrder,
                shelveNo,
                itemID,
                itemCount,
                currencyType,
                itemPrice
            };

            return requestInsertRecord;
        }
    }

    public class StoreRefresh : LogClass
    {
        public DateTime recvDate { get; set; }
        public int storeOrder { get; set; }
        public int storeRefreshCount { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                recvDate.GetTypeCode(),
                storeOrder.GetTypeCode(),
                storeRefreshCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                recvDate,
                storeOrder,
                storeRefreshCount
            };

            return requestInsertRecord;
        }
    }
}

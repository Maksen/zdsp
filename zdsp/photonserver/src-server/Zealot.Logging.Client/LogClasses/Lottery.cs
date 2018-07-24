namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class LotteryUseFreeTicket : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }
        public int useFreeCount { get; set; }

        public int oldFreeCount { get; set; }

        public int newFreeCount { get; set; }

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
                lotteryId.GetTypeCode(),
                useFreeCount.GetTypeCode(),
                oldFreeCount.GetTypeCode(),
                newFreeCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                useFreeCount,
                oldFreeCount,
                newFreeCount
            };

            return requestInsertRecord;
        }
    }

    public class LotteryUseExtraTicket : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int itemId { get; set; }

        public int useExtraCount { get; set; }

        public int newExtraCount { get; set; }

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
                lotteryId.GetTypeCode(),
                itemId.GetTypeCode(),
                useExtraCount.GetTypeCode(),
                newExtraCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                itemId,
                useExtraCount,
                newExtraCount
            };

            return requestInsertRecord;
        }
    }

    public class LotteryUseGold : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int useGold { get; set; }

        public int oldGold { get; set; }

        public int newGold { get; set; }

        public int useBindGold { get; set; }

        public int oldBindGold { get; set; }

        public int newBindGold { get; set; }

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
                lotteryId.GetTypeCode(),
                useGold.GetTypeCode(),
                oldGold.GetTypeCode(),
                newGold.GetTypeCode(),
                useBindGold.GetTypeCode(),
                oldBindGold.GetTypeCode(),
                newBindGold.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                useGold,
                oldGold,
                newGold,
                useBindGold,
                oldBindGold,
                newBindGold
            };

            return requestInsertRecord;
        }
    }

    public class LotteryGetItem : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int itemId { get; set; }

        public int count { get; set; }

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
                lotteryId.GetTypeCode(),
                itemId.GetTypeCode(),
                count.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                itemId,
                count
            };

            return requestInsertRecord;
        }
    }

    public class LotteryGetPoint : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int getPoint { get; set; }

        public int oldPoint { get; set; }

        public int newPoint { get; set; }

        public int rollTimes { get; set; }

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
                lotteryId.GetTypeCode(),
                getPoint.GetTypeCode(),
                oldPoint.GetTypeCode(),
                newPoint.GetTypeCode(),
                rollTimes.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                getPoint,
                oldPoint,
                newPoint,
                rollTimes
            };

            return requestInsertRecord;
        }
    }

    public class LotteryGetPointReward : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int point { get; set; }

        public int itemId1 { get; set; }

        public int count1 { get; set; }

        public int itemId2 { get; set; }

        public int count2 { get; set; }

        public int itemId3 { get; set; }

        public int count3 { get; set; }

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
                lotteryId.GetTypeCode(),
                point.GetTypeCode(),
                itemId1.GetTypeCode(),
                count1.GetTypeCode(),
                itemId2.GetTypeCode(),
                count2.GetTypeCode(),
                itemId3.GetTypeCode(),
                count3.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                point,
                itemId1,
                count1,
                itemId2,
                count2,
                itemId3,
                count3
            };

            return requestInsertRecord;
        }
    }

    public class LotteryUsePointItem : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int itemId { get; set; }

        public int count { get; set; }

        public int getPoint { get; set; }

        public int oldPoint { get; set; }

        public int newPoint { get; set; }

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
                lotteryId.GetTypeCode(),
                itemId.GetTypeCode(),
                count.GetTypeCode(),
                getPoint.GetTypeCode(),
                oldPoint.GetTypeCode(),
                newPoint.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                itemId,
                count,
                getPoint,
                oldPoint,
                newPoint
            };

            return requestInsertRecord;
        }
    }

    public class LotteryUseFreeAndGetPoint : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int lotteryId { get; set; }

        public int useFreeCount { get; set; }

        public int lastFreeCount { get; set; }

        public int getPoint { get; set; }

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
                lotteryId.GetTypeCode(),
                useFreeCount.GetTypeCode(),
                lastFreeCount.GetTypeCode(),
                getPoint.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                lotteryId,
                useFreeCount,
                lastFreeCount,
                getPoint
            };

            return requestInsertRecord;
        }
    }
}
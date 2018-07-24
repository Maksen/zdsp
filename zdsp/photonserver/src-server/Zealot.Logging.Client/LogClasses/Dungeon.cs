namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class DungeonComplete : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int realmId { get; set; }
        public int difficulty { get; set; }
        public int reqLvl { get; set; }
        public int entryBefore { get; set; }
        public int entryAfter { get; set; }
        public bool isSuccess { get; set; }
        public int rewardGrp { get; set; }
        public string stars { get; set; }

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
                realmId.GetTypeCode(),
                difficulty.GetTypeCode(),
                reqLvl.GetTypeCode(),
                entryBefore.GetTypeCode(),
                entryAfter.GetTypeCode(),
                isSuccess.GetTypeCode(),
                rewardGrp.GetTypeCode(),
                stars.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                realmId,
                difficulty,
                reqLvl,
                entryBefore,
                entryAfter,
                isSuccess,
                rewardGrp,
                stars
            };

            return requestInsertRecord;
        }
    }

    public class DungeonCollectStarReward : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int seq { get; set; }
        public int starCount { get; set; }

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
                seq.GetTypeCode(),
                starCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                seq,
                starCount
            };

            return requestInsertRecord;
        }
    }

    public class DungeonAddExtraEntry : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int realmId { get; set; }
        public int dailyExtraEntry { get; set; }
        public int extraEntryBefore { get; set; }
        public int extraEntryAfter { get; set; }

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
                realmId.GetTypeCode(),
                dailyExtraEntry.GetTypeCode(),
                extraEntryBefore.GetTypeCode(),
                extraEntryAfter.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                realmId,
                dailyExtraEntry,
                extraEntryBefore,
                extraEntryAfter
            };

            return requestInsertRecord;
        }
    }
}
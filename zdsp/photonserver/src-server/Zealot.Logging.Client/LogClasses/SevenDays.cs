namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class SevenDaysTaskReward : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string system { get; set; }

        public int newServerActivityId { get; set; }

        public int rewardListId { get; set; }

        public int rewardItemId { get; set; }

        public int rewardAmount { get; set; }

        public int rewardItemCountBef { get; set; }

        public int rewardItemCountAft { get; set; }

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
                system.GetTypeCode(),
                newServerActivityId.GetTypeCode(),
                rewardListId.GetTypeCode(),
                rewardItemId.GetTypeCode(),
                rewardAmount.GetTypeCode(),
                rewardItemCountBef.GetTypeCode(),
                rewardItemCountAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                system,
                newServerActivityId,
                rewardListId,
                rewardItemId,
                rewardAmount,
                rewardItemCountBef,
                rewardItemCountAft
            };

            return requestInsertRecord;
        }
    }

    public class RestrictionBuyLimit : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string system { get; set; }

        public int restrictionId { get; set; }

        public int buyAmount { get; set; }

        public int buyLimitBef { get; set; }

        public int buyLimitAft { get; set; }

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
                system.GetTypeCode(),
                restrictionId.GetTypeCode(),
                buyAmount.GetTypeCode(),
                buyLimitBef.GetTypeCode(),
                buyLimitAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                system,
                restrictionId,
                buyAmount,
                buyLimitBef,
                buyLimitAft
            };

            return requestInsertRecord;
        }
    }

    public class RestrictionLockGoldUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string system { get; set; }

        public int restrictionId { get; set; }

        public int useAmount { get; set; }

        public int lockGoldBef { get; set; }

        public int lockGoldAft { get; set; }

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
                system.GetTypeCode(),
                restrictionId.GetTypeCode(),
                useAmount.GetTypeCode(),
                lockGoldBef.GetTypeCode(),
                lockGoldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                system,
                restrictionId,
                useAmount,
                lockGoldBef,
                lockGoldAft
            };

            return requestInsertRecord;
        }
    }

    public class RestrictionGoldUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string system { get; set; }

        public int restrictionId { get; set; }

        public int useAmount { get; set; }

        public int goldBef { get; set; }

        public int goldAft { get; set; }

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
                system.GetTypeCode(),
                restrictionId.GetTypeCode(),
                useAmount.GetTypeCode(),
                goldBef.GetTypeCode(),
                goldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                system,
                restrictionId,
                useAmount,
                goldBef,
                goldAft
            };

            return requestInsertRecord;
        }
    }

    public class RestrictionGetItem : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string system { get; set; }

        public int restrictionId { get; set; }

        public int itemId { get; set; }

        public int itemAmount { get; set; }

        public int itemCountBef { get; set; }

        public int itemCountAft { get; set; }

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
                system.GetTypeCode(),
                restrictionId.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode(),
                itemCountBef.GetTypeCode(),
                itemCountAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                system,
                restrictionId,
                itemId,
                itemAmount,
                itemCountBef,
                itemCountAft
            };

            return requestInsertRecord;
        }
    }
}

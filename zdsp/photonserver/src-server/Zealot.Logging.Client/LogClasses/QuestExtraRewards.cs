namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class QERBoxRewardGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int currActivePts { get; set; }

        public int boxNum { get; set; }

        //public int rewardItemId { get; set; }

        //public int rewardAmount { get; set; }

        //public int rewardItemCountBef { get; set; }

        //public int rewardItemCountAft { get; set; }

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
                type.GetTypeCode(),
                currActivePts.GetTypeCode(),
                boxNum.GetTypeCode()
                //rewardItemId.GetTypeCode(),
                //rewardAmount.GetTypeCode(),
                //rewardItemCountBef.GetTypeCode(),
                //rewardItemCountAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                currActivePts,
                boxNum
                //rewardItemId,
                //rewardAmount,
                //rewardItemCountBef,
                //rewardItemCountAft
            };

            return requestInsertRecord;
        }
    }

    public class QERTaskFinish : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int questId { get; set; }

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
                type.GetTypeCode(),
                questId.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type.GetTypeCode(),
                questId
            };

            return requestInsertRecord;
        }
    }

    public class QERTaskFinishAll : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public string questIds { get; set; }

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
                type.GetTypeCode(),
                questIds.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type.GetTypeCode(),
                questIds
            };

            return requestInsertRecord;
        }
    }

    public class QERActivePtsGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string getType { get; set; }

        //public int questId { get; set; }        

        public int activePtsAmount { get; set; }

        //public int activePtsBef { get; set; }

        public int activePtsAft { get; set; }

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
                //questId.GetTypeCode(),
                getType.GetTypeCode(),
                activePtsAmount.GetTypeCode(),
                //activePtsBef.GetTypeCode(),
                activePtsAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                //questId,
                getType,
                activePtsAmount,
                //activePtsBef,
                activePtsAft
            };

            return requestInsertRecord;
        }
    }

    public class QERActivePtsGetQuestID : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int questId { get; set; }

        public string getType { get; set; }

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
                type.GetTypeCode(),
                questId.GetTypeCode(),
                getType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                questId,
                getType
            };

            return requestInsertRecord;
        }
    }

    public class QERMoneyGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questId { get; set; }

        public string getType { get; set; }

        public int moneyAmount { get; set; }

        public int moneyBef { get; set; }

        public int moneyAft { get; set; }

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
                questId.GetTypeCode(),
                getType.GetTypeCode(),
                moneyAmount.GetTypeCode(),
                moneyBef.GetTypeCode(),
                moneyAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questId,
                getType,
                moneyAmount,
                moneyBef,
                moneyAft
            };

            return requestInsertRecord;
        }
    }

    public class QERVIPXPGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int questId { get; set; }

        public string getType { get; set; }

        public int vipXPAmount { get; set; }

        //public int vipXPBef { get; set; }

        //public int vipXPAft { get; set; }

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
                type.GetTypeCode(),
                questId.GetTypeCode(),
                getType.GetTypeCode(),
                vipXPAmount.GetTypeCode()
                //vipXPBef.GetTypeCode(),
                //vipXPAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                questId,
                getType,
                vipXPAmount
                //vipXPBef,
                //vipXPAft
            };

            return requestInsertRecord;
        }
    }

    public class QERLockGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questId { get; set; }

        public string getType { get; set; }

        public int lockGoldAmount { get; set; }

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
                questId.GetTypeCode(),
                getType.GetTypeCode(),
                lockGoldAmount.GetTypeCode(),
                lockGoldBef.GetTypeCode(),
                lockGoldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questId,
                getType,
                lockGoldAmount,
                lockGoldBef,
                lockGoldAft
            };

            return requestInsertRecord;
        }
    }

    public class QERItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questId { get; set; }

        public string getType { get; set; }

        public int itemAmount { get; set; }

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
                questId.GetTypeCode(),
                getType.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questId,
                getType,
                itemAmount
            };

            return requestInsertRecord;
        }
    }
}

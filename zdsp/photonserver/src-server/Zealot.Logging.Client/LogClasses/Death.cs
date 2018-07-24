namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class DeathRespawnType : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string respawnMethod { get; set; }

        public int mapId { get; set; }

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
                respawnMethod.GetTypeCode(),
                mapId.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                respawnMethod,
                mapId
            };

            return requestInsertRecord;
        }
    }

    public class DeathRespawnFree : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }

        public int useAmount { get; set; }

        public int freeReviveBef { get; set; }

        public int freeReviveAft { get; set; }

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
                actionType.GetTypeCode(),
                useAmount.GetTypeCode(),
                freeReviveBef.GetTypeCode(),
                freeReviveAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                useAmount,
                freeReviveBef,
                freeReviveAft
            };

            return requestInsertRecord;
        }
    }

    public class DeathRespawnItem : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }

        public int itemId { get; set; }

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
                actionType.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    public class DeathRespawnLockGold : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }

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
                actionType.GetTypeCode(),
                useAmount.GetTypeCode(),
                lockGoldBef.GetTypeCode(),
                lockGoldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                useAmount,
                lockGoldBef,
                lockGoldAft
            };

            return requestInsertRecord;
        }
    }

    public class DeathRespawnGold : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string actionType { get; set; }

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
                actionType.GetTypeCode(),
                useAmount.GetTypeCode(),
                goldBef.GetTypeCode(),
                goldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                actionType,
                useAmount,
                goldBef,
                goldAft
            };

            return requestInsertRecord;
        }
    }

    public class PlayerKillPlayer :LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string player { get; set; }
        public string killer { get; set; }

        public int realmID { get; set; } 
        public string realmName { get; set; }
         

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
                player.GetTypeCode(),
                killer.GetTypeCode(),
                realmID.GetTypeCode(),
                realmName.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                player,
                killer,
                realmID,
                realmName
            };

            return requestInsertRecord;
        }
    }
}

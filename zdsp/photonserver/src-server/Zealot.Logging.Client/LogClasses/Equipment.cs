namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class EquipmentUpgrade : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public string equipSlot { get; set; }

        public int upgradeLvlAft { get; set; }

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
                equipSlot.GetTypeCode(),
                upgradeLvlAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                equipSlot,
                upgradeLvlAft
            };

            return requestInsertRecord;
        }
    }

    public class EquipmentMoneyUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int amount { get; set; }

        public int moneyBef { get; set; }

        public int moneyAft { get; set; }

        public string system { get; set; }

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
                amount.GetTypeCode(),
                moneyBef.GetTypeCode(),
                moneyAft.GetTypeCode(),
                system.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                amount,
                moneyBef,
                moneyAft,
                system
            };

            return requestInsertRecord;
        }
    }

    public class EquipmentGemSlot : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public string equipSlot { get; set; }

        //public int gemSlot1IDBef { get; set; }

        public int gemID { get; set; }

        //public int gemSlot2IDBef { get; set; }

        //public int gemSlot2IDAft { get; set; }

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
                equipSlot.GetTypeCode(),
                //gemSlot1IDBef.GetTypeCode(),
                gemID.GetTypeCode(),
                //gemSlot2IDBef.GetTypeCode(),
                //gemSlot2IDAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                equipSlot,
                //gemSlot1IDBef,
                gemID,
                //gemSlot2IDBef,
                //gemSlot2IDAft
            };

            return requestInsertRecord;
        }
    }

    public class EquipmentItemUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public int count { get; set; }

        public string system { get; set; }

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
                itemId.GetTypeCode(),
                count.GetTypeCode(),
                system.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                count,
                system
            };

            return requestInsertRecord;
        }
    }

    public class EquipmentItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public int count { get; set; }

        public string system { get; set; }

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
                itemId.GetTypeCode(),
                count.GetTypeCode(),
                system.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                count,
                system
            };

            return requestInsertRecord;
        }
    }
}
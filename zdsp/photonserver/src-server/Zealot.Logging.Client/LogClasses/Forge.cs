namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class ForgeItemCreate : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public string equipmentType { get; set; }

        public string rarityType { get; set; }

        public int brilliantAttribute1 { get; set; }

        public int brilliantAttribute2 { get; set; }

        public int brilliantAttribute3 { get; set; }

        public int brilliantAttribute4 { get; set; }

        public int brilliantAttribute5 { get; set; }

        public bool superAbility { get; set; }
        
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
                itemUId.GetTypeCode(),
                equipmentType.GetTypeCode(),
                rarityType.GetTypeCode(),
                brilliantAttribute1.GetTypeCode(),
                brilliantAttribute2.GetTypeCode(),
                brilliantAttribute3.GetTypeCode(),
                brilliantAttribute4.GetTypeCode(),
                brilliantAttribute5.GetTypeCode(),
                superAbility.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                equipmentType,
                rarityType,
                brilliantAttribute1,
                brilliantAttribute2,
                brilliantAttribute3,
                brilliantAttribute4,
                brilliantAttribute5,
                superAbility
            };

            return requestInsertRecord;
        }
    }

    public class ForgeMaterialUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
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
                itemId.GetTypeCode(),
                count.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                count
            };

            return requestInsertRecord;
        }
    }

    public class ForgeCurrencyUse : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string currency { get; set; }

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
                currency.GetTypeCode(),
                count.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                currency,
                count
            };

            return requestInsertRecord;
        }
    }

    public class ForgeUpgrade : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public int upgradeLevel { get; set; }

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
                itemUId.GetTypeCode(),
                upgradeLevel.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                upgradeLevel
            };

            return requestInsertRecord;
        }
    }

    public class ForgePlus : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public int plusId { get; set; }

        public int plusLevel { get; set; }

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
                itemUId.GetTypeCode(),
                plusId.GetTypeCode(),
                plusLevel.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                plusLevel
            };

            return requestInsertRecord;
        }
    }

    public class ForgeFoster : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public int fosterId { get; set; }

        public int fosterA { get; set; }

        public int fosterB { get; set; }

        public int fosterC { get; set; }

        public int fosterD { get; set; }

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
                itemUId.GetTypeCode(),
                fosterId.GetTypeCode(),
                fosterA.GetTypeCode(),
                fosterB.GetTypeCode(),
                fosterC.GetTypeCode(),
                fosterD.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                fosterId,
                fosterA,
                fosterB,
                fosterC,
                fosterD
            };

            return requestInsertRecord;
        }
    }

    public class ForgeUpTier : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public string rarityType { get; set; }

        public int upgradeLevel { get; set; }

        public int plusId { get; set; }

        public int plusLevel { get; set; }

        public int fosterId { get; set; }

        public int fosterA { get; set; }

        public int fosterB { get; set; }

        public int fosterC { get; set; }

        public int fosterD { get; set; }

        public int brilliantAttribute1 { get; set; }

        public int brilliantAttribute2 { get; set; }

        public int brilliantAttribute3 { get; set; }

        public int brilliantAttribute4 { get; set; }

        public int brilliantAttribute5 { get; set; }

        public bool superAbility { get; set; }

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
                itemUId.GetTypeCode(),
                rarityType.GetTypeCode(),
                upgradeLevel.GetTypeCode(),
                plusId.GetTypeCode(),
                plusLevel.GetTypeCode(),
                fosterId.GetTypeCode(),
                fosterA.GetTypeCode(),
                fosterB.GetTypeCode(),
                fosterC.GetTypeCode(),
                fosterD.GetTypeCode(),
                brilliantAttribute1.GetTypeCode(),
                brilliantAttribute2.GetTypeCode(),
                brilliantAttribute3.GetTypeCode(),
                brilliantAttribute4.GetTypeCode(),
                brilliantAttribute5.GetTypeCode(),
                superAbility.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                rarityType,
                upgradeLevel,
                plusId,
                plusLevel,
                fosterId,
                fosterA,
                fosterB,
                fosterC,
                fosterD,
                brilliantAttribute1,
                brilliantAttribute2,
                brilliantAttribute3,
                brilliantAttribute4,
                brilliantAttribute5,
                superAbility
            };

            return requestInsertRecord;
        }
    }

    public class ForgeTransfer : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int itemId { get; set; }

        public string itemUId { get; set; }

        public string rarityType { get; set; }

        public int upgradeLevel { get; set; }

        public int plusId { get; set; }

        public int plusLevel { get; set; }

        public int fosterId { get; set; }

        public int fosterA { get; set; }

        public int fosterB { get; set; }

        public int fosterC { get; set; }

        public int fosterD { get; set; }

        public int brilliantAttribute1 { get; set; }

        public int brilliantAttribute2 { get; set; }

        public int brilliantAttribute3 { get; set; }

        public int brilliantAttribute4 { get; set; }

        public int brilliantAttribute5 { get; set; }

        public bool superAbility { get; set; }

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
                itemUId.GetTypeCode(),
                rarityType.GetTypeCode(),
                upgradeLevel.GetTypeCode(),
                plusId.GetTypeCode(),
                plusLevel.GetTypeCode(),
                fosterId.GetTypeCode(),
                fosterA.GetTypeCode(),
                fosterB.GetTypeCode(),
                fosterC.GetTypeCode(),
                fosterD.GetTypeCode(),
                brilliantAttribute1.GetTypeCode(),
                brilliantAttribute2.GetTypeCode(),
                brilliantAttribute3.GetTypeCode(),
                brilliantAttribute4.GetTypeCode(),
                brilliantAttribute5.GetTypeCode(),
                superAbility.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemUId,
                rarityType,
                upgradeLevel,
                plusId,
                plusLevel,
                fosterId,
                fosterA,
                fosterB,
                fosterC,
                fosterD,
                brilliantAttribute1,
                brilliantAttribute2,
                brilliantAttribute3,
                brilliantAttribute4,
                brilliantAttribute5,
                superAbility
            };

            return requestInsertRecord;
        }
    }
}
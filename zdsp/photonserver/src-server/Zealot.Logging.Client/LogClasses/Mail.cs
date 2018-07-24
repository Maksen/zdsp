namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class MailSent : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public DateTime sentDate { get; set; }
        public string mailName { get; set; }
        public string rcvName { get; set; }
        public string attachment { get; set; }
        public string currency { get; set; }

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
                sentDate.GetTypeCode(),
                mailName.GetTypeCode(),
                rcvName.GetTypeCode(),
                attachment.GetTypeCode(),
                currency.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                sentDate,
                mailName,
                rcvName,
                attachment,
                currency
            };

            return requestInsertRecord;
        }
    }

    public class MailTaken : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public DateTime sentDate { get; set; }
        public string mailName { get; set; }

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
                sentDate.GetTypeCode(),
                mailName.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                sentDate,
                mailName
            };

            return requestInsertRecord;
        }
    }

    public class MailRemove : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public DateTime sentDate { get; set; }
        public string mailName { get; set; }

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
                sentDate.GetTypeCode(),
                mailName.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                sentDate,
                mailName,
            };

            return requestInsertRecord;
        }
    }
}

using System;
using System.Collections.Generic;
using Zealot.Logging.Contracts.Requests;

namespace Zealot.Logging.Client.LogClasses
{
    public class RequestDonateLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int itemId { get; set; }

        public int requestItemCount { get; set; }

        public int deductRequestCount { get; set; }

        public int requestCount { get; set; }

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
                action.GetTypeCode(),
                itemId.GetTypeCode(),
                requestItemCount.GetTypeCode(),
                deductRequestCount.GetTypeCode(),
                requestCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                itemId,
                requestItemCount,
                deductRequestCount,
                requestCount,
            };

            return requestInsertRecord;
        }
    }

    public class RequestDonateItemLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int itemId { get; set; }

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
                action.GetTypeCode(),
                itemId.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                itemId,
            };

            return requestInsertRecord;
        }
    }

    public class RequestDonateCountLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int deductRequestCount { get; set; }

        public int requestCount { get; set; }

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
                action.GetTypeCode(),
                deductRequestCount.GetTypeCode(),
                requestCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                deductRequestCount,
                requestCount,
            };

            return requestInsertRecord;
        }
    }

    public class ResponseDonateLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int itemId { get; set; }

        public int deductItemCount { get; set; }

        public int remainingItemCount { get; set; }

        public int getGuildContribution { get; set; }

        public int GuildContributionCount { get; set; }

        public int responseCount { get; set; }

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
                action.GetTypeCode(),
                itemId.GetTypeCode(),
                deductItemCount.GetTypeCode(),
                remainingItemCount.GetTypeCode(),
                getGuildContribution.GetTypeCode(),
                GuildContributionCount.GetTypeCode(),
                responseCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                itemId,
                deductItemCount,
                remainingItemCount,
                getGuildContribution,
                GuildContributionCount,
                responseCount,
            };

            return requestInsertRecord;
        }
    }

    public class ResponseDonateItemLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int itemId { get; set; }

        public int responseCount { get; set; }

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
                action.GetTypeCode(),
                itemId.GetTypeCode(),
                responseCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                itemId,
                responseCount,
            };

            return requestInsertRecord;
        }
    }

    public class GetDonateLog : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.operties.
        public string action { get; set; }

        public int itemId { get; set; }

        public int getDonateCount { get; set; }

        public int itemCount { get; set; }

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
                action.GetTypeCode(),
                itemId.GetTypeCode(),
                getDonateCount.GetTypeCode(),
                itemCount.GetTypeCode(),
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                action,
                itemId,
                getDonateCount,
                itemCount,

            };

            return requestInsertRecord;
        }
    }


}

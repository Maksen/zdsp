namespace Zealot.Logging.Client.LogClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Zealot.Logging.Contracts.Requests;


    public class GuildQuestRefresh : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questid1 { get; set; }  
        public int questid2 { get; set; }  
        public int questid3 { get; set; }  
        public int freerefreshtimesleft { get; set; }  

        
        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptions and/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                questid1.GetTypeCode(),
                questid2.GetTypeCode(),
                questid3.GetTypeCode(),
                freerefreshtimesleft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questid1,
                questid2,
                questid3,
                freerefreshtimesleft
            };

            return requestInsertRecord;
        }
    }

    public class GuildQuestDrop : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questid1 { get; set; }
        
        public int dailytimesleft { get; set; }


        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptions and/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                questid1.GetTypeCode(),                
                dailytimesleft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questid1,
                dailytimesleft
            };

            return requestInsertRecord;
        }
    }

    public class GuildQuestFinish: LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public int questid1 { get; set; }

        public int dailytimesleft { get; set; }

        public int wealthAmount { get; set; }
        public int wealthBefore { get; set; }

        public int wealthAfter { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            // WARNING!
            // Code all properties in the logFieldTypes and logFieldValues in the 
            // EXACT order in which they are declared above or risk potential SQL
            // exceptions and/or data corruption.
            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                questid1.GetTypeCode(),
                dailytimesleft.GetTypeCode(),
                wealthAmount.GetTypeCode(),
                wealthBefore.GetTypeCode(),
                wealthAfter.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                questid1,
                dailytimesleft,
                wealthAmount,
                wealthBefore,
                wealthAfter
            };

            return requestInsertRecord;
        }
    }

}

namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class PlayerTrade : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string targetUserId { get; set; }

        public string targetCharId { get; set; }

        public int playerItemId1 { get; set; }

        public int playerItemId2 { get; set; }

        public int playerItemId3 { get; set; }

        public int playerItemId4 { get; set; }

        public int playerItemId5 { get; set; }

        public int playerItemId6 { get; set; }

        public int playerGold { get; set; }

        public int playerDiamond { get; set; }

        public int targetItemId1 { get; set; }

        public int targetItemId2 { get; set; }

        public int targetItemId3 { get; set; }

        public int targetItemId4 { get; set; }

        public int targetItemId5 { get; set; }

        public int targetItemId6 { get; set; }

        public int targetGold { get; set; }

        public int targetDiamond { get; set; }

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
                targetUserId.GetTypeCode(),
                targetCharId.GetTypeCode(),
                playerItemId1.GetTypeCode(),
                playerItemId2.GetTypeCode(),
                playerItemId3.GetTypeCode(),
                playerItemId4.GetTypeCode(),
                playerItemId5.GetTypeCode(),
                playerItemId6.GetTypeCode(),
                playerGold.GetTypeCode(),
                playerDiamond.GetTypeCode(),
                targetItemId1.GetTypeCode(),
                targetItemId2.GetTypeCode(),
                targetItemId3.GetTypeCode(),
                targetItemId4.GetTypeCode(),
                targetItemId5.GetTypeCode(),
                targetItemId6.GetTypeCode(),
                targetGold.GetTypeCode(),
                targetDiamond.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                targetUserId,
                targetCharId,
                playerItemId1,
                playerItemId2,
                playerItemId3,
                playerItemId4,
                playerItemId5,
                playerItemId6,
                playerGold,
                playerDiamond,
                targetItemId1,
                targetItemId2,
                targetItemId3,
                targetItemId4,
                targetItemId5,
                targetItemId6,
                targetGold,
                targetDiamond
            };

            return requestInsertRecord;
        }
    }
}

namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;

    public abstract class LogClass
    {
        private DateTime dateTime { get; set; }

        public static string serverId { get; set; }

        public string userId { get; set; }
        //public Guid userId { get; set; }

        public string charId { get; set; }
        //public Guid charId { get; set; }

        public string message { get; set; }

        protected RequestInsertRecord CreateRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = new RequestInsertRecord();
            requestInsertRecord.dateTime = DateTime.Now;
            requestInsertRecord.serverId = serverId;
            //requestInsertRecord.userId = userId;
            //requestInsertRecord.charId = charId;

            if (!string.IsNullOrEmpty(userId))
            {
                Guid _userId;
                if (Guid.TryParse(userId, out _userId))
                    requestInsertRecord.userId = _userId;
            }
            
            if (!string.IsNullOrEmpty(charId))
            {
                Guid _charId;
                if (Guid.TryParse(charId, out _charId))
                    requestInsertRecord.charId = _charId;
            }

            requestInsertRecord.logName = this.GetType().Name;// logName;
            requestInsertRecord.message = message;

            return requestInsertRecord;
        }

        public abstract RequestInsertRecord GetRequestInsertRecord();
    }
}

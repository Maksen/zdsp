namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    public class OfflineExp_ChooseReward : LogClass
    {
        public DateTime recvDate { get; set; }
        public int charLevel { get; set; }
        public int chosenTime { get; set; }
        public int baseExpReward { get; set; }

        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                recvDate.GetTypeCode(),
                charLevel.GetTypeCode(),
                chosenTime.GetTypeCode(),
                baseExpReward.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                recvDate,
                charLevel,
                chosenTime,
                baseExpReward
            };

            return requestInsertRecord;
        }
    }

    public class OfflineExp_ClaimReward : LogClass
    {
        public DateTime recvDate { get; set; }
        public int beforeCharLevel { get; set; }
        public int afterCharLevel { get; set; }
        public string chosenBonus { get; set; }
        public int baseExpReward { get; set; }  //Experience points before any bonuses
        public int finalExpReward { get; set; }  //Claimed exp point
        public int beforeCharExp { get; set; }
        public int afterCharExp { get; set; }


        public override RequestInsertRecord GetRequestInsertRecord()
        {
            RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

            requestInsertRecord.logFieldTypes = new List<TypeCode>()
            {
                recvDate.GetTypeCode(),
                beforeCharLevel.GetTypeCode(),
                afterCharLevel.GetTypeCode(),
                chosenBonus.GetTypeCode(),
                baseExpReward.GetTypeCode(),
                finalExpReward.GetTypeCode(),
                beforeCharExp.GetTypeCode(),
                afterCharExp.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                recvDate,
                beforeCharLevel,
                afterCharLevel,
                chosenBonus,
                baseExpReward,
                finalExpReward,
                beforeCharExp,
                afterCharExp
            };

            return requestInsertRecord;
        }
    }
}

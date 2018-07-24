namespace Zealot.Logging.Client.LogClasses
{
    using Contracts.Requests;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// <Updated>Dropped stuff</Updated>
    /// </summary>
    public class WelfareSignInPrizeGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
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
                getType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                getType
            };

            return requestInsertRecord;
        }
    }

    /// <summary>
    /// <Update>Dropped stuff</Update>
    /// </summary>
    public class WelfareSignInPrizeItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string getType { get; set; }

        public int vipLvl { get; set; }

        public int playerVIPLvl { get; set; }

        public int vipStackBonus { get; set; }

        public int actualStackBonus { get; set; }

        public int rewardItemID { get; set; }

        public int rewardItemCount { get; set; }



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
                getType.GetTypeCode(),
                vipLvl.GetTypeCode(),
                playerVIPLvl.GetTypeCode(),
                vipStackBonus.GetTypeCode(),
                actualStackBonus.GetTypeCode(),
                rewardItemID.GetTypeCode(),
                rewardItemCount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                getType,
                vipLvl,
                playerVIPLvl,
                vipStackBonus,
                actualStackBonus,
                rewardItemID,
                rewardItemCount
            };

            return requestInsertRecord;
        }
    }

    public class WelfareSignInPrizeReGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int year { get; set; }

        public int month { get; set; }

        public int day { get; set; }

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
                year.GetTypeCode(),
                month.GetTypeCode(),
                day.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                year,
                month,
                day
            };

            return requestInsertRecord;
        }
    }

    ////Check google Doc agian
    //public class WelfareSignInPrizeReGetLockGoldUse : LogClass
    //{
    //    // WARNING!
    //    // Never re-arrange the order of the properties after initial creation.
    //    // Logging server uses SQL Prepared Statements for performance.
    //    // Altering the order will cause SQL exceptions and/or data corruption on
    //    // current/subsequent insertion of records. Always append new properties. 
    //    // Do not insert between existing properties.

    //    public int lockGoldAmount { get; set; }

    //    public int lockGoldBef { get; set; }

    //    public int lockGoldAft { get; set; }

    //    public override RequestInsertRecord GetRequestInsertRecord()
    //    {
    //        // Reminder: Change the class type identical to this class.
    //        RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

    //        // WARNING!
    //        // Code all properties in the logFieldTypes and logFieldValues in the 
    //        // EXACT order in which they are declared above or risk potential SQL
    //        // exceptions and/or data corruption.
    //        requestInsertRecord.logFieldTypes = new List<TypeCode>()
    //        {
    //            lockGoldAmount.GetTypeCode(),
    //            lockGoldBef.GetTypeCode(),
    //            lockGoldAft.GetTypeCode()
    //        };

    //        requestInsertRecord.logFieldValues = new List<object>()
    //        {
    //            lockGoldAmount,
    //            lockGoldBef,
    //            lockGoldAft
    //        };

    //        return requestInsertRecord;
    //    }
    //}

    //public class WelfareSignInPrizeReGetGoldUse : LogClass
    //{
    //    // WARNING!
    //    // Never re-arrange the order of the properties after initial creation.
    //    // Logging server uses SQL Prepared Statements for performance.
    //    // Altering the order will cause SQL exceptions and/or data corruption on
    //    // current/subsequent insertion of records. Always append new properties. 
    //    // Do not insert between existing properties.

    //    public int goldAmount { get; set; }

    //    public int goldBef { get; set; }

    //    public int goldAft { get; set; }

    //    public override RequestInsertRecord GetRequestInsertRecord()
    //    {
    //        // Reminder: Change the class type identical to this class.
    //        RequestInsertRecord requestInsertRecord = CreateRequestInsertRecord();

    //        // WARNING!
    //        // Code all properties in the logFieldTypes and logFieldValues in the 
    //        // EXACT order in which they are declared above or risk potential SQL
    //        // exceptions and/or data corruption.
    //        requestInsertRecord.logFieldTypes = new List<TypeCode>()
    //        {
    //            goldAmount.GetTypeCode(),
    //            goldBef.GetTypeCode(),
    //            goldAft.GetTypeCode()
    //        };

    //        requestInsertRecord.logFieldValues = new List<object>()
    //        {
    //            goldAmount,
    //            goldBef,
    //            goldAft
    //        };

    //        return requestInsertRecord;
    //    }
    //}

    public class WelfareOnlinePrizeGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int serial { get; set; }

        public double duration { get; set; }

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
                serial.GetTypeCode(),
                duration.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                serial,
                duration
            };

            return requestInsertRecord;
        }
    }

    public class WelfareOnlinePrizeItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.

        public int itemId { get; set; }

        public long itemAmount { get; set; }

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
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    public class WelfareDailyGoldMCardBuy : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string buyType { get; set; }

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
                buyType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                buyType
            };

            return requestInsertRecord;
        }
    }

    public class WelfareDailyGoldPCardBuy : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string buyType { get; set; }

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
                buyType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                buyType
            };

            return requestInsertRecord;
        }
    }

    /// <summary>
    /// <Update>Dropped stuff</Update>
    /// </summary>
    public class WelfareDailyGoldMCardLockGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
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
                getType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                getType
            };

            return requestInsertRecord;
        }
    }

    /// <summary>
    /// <Update>Dropped stuff</Update>
    /// </summary>
    public class WelfareDailyGoldPCardLockGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
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
                getType.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                getType
            };

            return requestInsertRecord;
        }
    }

    //Removed From Commit for now
    public class WelfareFirstTopUp : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int topUpAmount { get; set; }

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
                topUpAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                topUpAmount
            };

            return requestInsertRecord;
        }
    }

    // Removed From Commit for now
    public class WelfareFirstTopUpItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public string itemId { get; set; }

        public string itemAmount { get; set; }

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
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    public class WelfareTotalCredit : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int creditAmount { get; set; }

        public int creditBef { get; set; }

        public int creditAft { get; set; }

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
                creditAmount.GetTypeCode(),
                creditBef.GetTypeCode(),
                creditAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                creditAmount,
                creditBef,
                creditAft
            };

            return requestInsertRecord;
        }
    }

    public class WelfareTotalCreditItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int serial { get; set; }

        public string itemId { get; set; }

        public string itemAmount { get; set; }


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
                serial.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                serial,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    /// <summary>
    /// <Add> back in
    /// </summary>
    public class WelfareTotalCreditLockGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int serial { get; set; }

        public int lockGold { get; set; }

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
                serial.GetTypeCode(),
                lockGold.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                serial,
                lockGold
            };

            return requestInsertRecord;
        }
    }

    //Removed From Commit for now
    public class WelfareOpenServiceFundsBuy : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

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
                type.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type
            };

            return requestInsertRecord;
        }
    }

    /// <summary>
    /// <Update>Dropped Stuff</Update>
    /// </summary>
    public class WelfareOpenServiceFundsLockGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int playerLvl { get; set; }

        public int lockGoldAmount { get; set; }

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
                playerLvl.GetTypeCode(),
                lockGoldAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                playerLvl,
                lockGoldAmount
            };

            return requestInsertRecord;
        }
    }

    public class WelfareOpenServiceFundsItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int joinMemberNum { get; set; }

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
                type.GetTypeCode(),
                joinMemberNum.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                joinMemberNum,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    public class WelfareTotalSpend : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int spendAmount { get; set; }

        public int spendBef { get; set; }

        public int spendAft { get; set; }

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
                spendAmount.GetTypeCode(),
                spendBef.GetTypeCode(),
                spendAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                spendAmount,
                spendBef,
                spendAft
            };

            return requestInsertRecord;
        }
    }

    // Removed From Commit for now
    public class WelfareTotalSpendItemGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int serial { get; set; }

        public string itemId { get; set; }

        public string itemAmount { get; set; }

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
                serial.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                serial,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }

    /*<Update>Dropped Class</Update>
    public class WelfareTotalSpendGoldGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int version { get; set; }

        public int serial { get; set; }

        public int goldAmount { get; set; }

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
                version.GetTypeCode(),
                serial.GetTypeCode(),
                goldAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                version,
                serial,
                goldAmount
            };

            return requestInsertRecord;
        }
    }
    */

    /*<Update>Dropped Class</Update>
    public class WelfareGoldJackpotSpend : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int tier { get; set; }

        public int goldBef { get; set; }

        public int goldAft { get; set; }

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
                type.GetTypeCode(),
                tier.GetTypeCode(),
                goldBef.GetTypeCode(),
                goldAft.GetTypeCode(),
                lockGoldBef.GetTypeCode(),
                lockGoldAft.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                tier,
                goldBef,
                goldAft,
                lockGoldBef,
                lockGoldAft
            };

            return requestInsertRecord;
        }
    }
    */

    public class WelfareGoldJackpotRewardGet : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

        public int tier { get; set; }

        public int jackpotAmount { get; set; }

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
                tier.GetTypeCode(),
                jackpotAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                tier,
                jackpotAmount
            };

            return requestInsertRecord;
        }
    }

    // Removed From Commit for now
    public class WelfareContinuousLogin : LogClass
    {
        // WARNING!
        // Never re-arrange the order of the properties after initial creation.
        // Logging server uses SQL Prepared Statements for performance.
        // Altering the order will cause SQL exceptions and/or data corruption on
        // current/subsequent insertion of records. Always append new properties. 
        // Do not insert between existing properties.
        public string type { get; set; }

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
                type.GetTypeCode(),
                itemId.GetTypeCode(),
                itemAmount.GetTypeCode()
            };

            requestInsertRecord.logFieldValues = new List<object>()
            {
                type,
                itemId,
                itemAmount
            };

            return requestInsertRecord;
        }
    }
}

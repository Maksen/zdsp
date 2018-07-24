using ExitGames.Diagnostics.Counter;
using ExitGames.Diagnostics.Monitoring;

namespace Zealot.Server.Counters
{
    //These counters appear in the dashboard
    public static class GameCounters
    {
        [PublishCounter("_ProcessMemory")]
        public static readonly NumericCounter ProcessMemory = new NumericCounter();        

        [PublishCounter("_TotalRealms")]
        public static readonly NumericCounter TotalRealms = new NumericCounter();

        [PublishCounter("_1)CheckGamesDuration(msec)")]
        public static readonly NumericCounter CheckGamesDuration = new NumericCounter();

        [PublishCounter("_AverageRealmUpdateRate")]
        public static readonly AverageCounter AverageRealmUpdateRate = new AverageCounter();

        [PublishCounter("_TotalPlayers")]
        public static readonly NumericCounter TotalPlayers = new NumericCounter();

        [PublishCounter("_TotalEntities")]
        public static readonly NumericCounter TotalEntities = new NumericCounter();

        [PublishCounter("_RPCSentPerSec")]
        public static readonly CountsPerSecondCounter RPCSentPerSec = new CountsPerSecondCounter();

        [PublishCounter("_RPCReceivedPerSec")]
        public static readonly CountsPerSecondCounter RPCReceivedPerSec = new CountsPerSecondCounter();

        [PublishCounter("_AttackCountsPerSec")]
        public static readonly CountsPerSecondCounter AttackCountsPerSec = new CountsPerSecondCounter();
        
        //[PublishCounter("_ShuangFeiEntSysDuration(msec)")]
        public static readonly NumericCounter ShuangFeiEntSysDuration = new NumericCounter();

        //[PublishCounter("_ShuangFeiNetSlotUpdateDuration(msec)")]
        public static readonly NumericCounter ShuangFeiNetSlotUpdateDuration = new NumericCounter();                

        //[PublishCounter("_ShuangFeiNetUpdateRelevantObj(msec)")]
        public static readonly NumericCounter ShuangFeiNetUpdateRelevantObj = new NumericCounter();

        //[PublishCounter("_ShuangFeiNetUpdateSnapShot(msec)")]
        public static readonly NumericCounter ShuangFeiNetUpdateSnapShot = new NumericCounter();

        //[PublishCounter("_ShuangFeiNetSyncRelevantStats(msec)")]
        public static readonly NumericCounter ShuangFeiNetSyncRelevantStats = new NumericCounter();

        //[PublishCounter("_ShuangFeiNetDispatchChat(msec)")]
        public static readonly NumericCounter ShuangFeiNetDispatchChat = new NumericCounter();        

        [PublishCounter("_1_1)TotalEntSysDuration(msec)")]
        public static readonly NumericCounter TotalEntSysDuration = new NumericCounter();

        [PublishCounter("_1_2)TotalNetSlotUpdateDuration(msec)")]
        public static readonly NumericCounter TotalNetSlotUpdateDuration = new NumericCounter();

        [PublishCounter("_1_2_1)TotalNetUpdateRelevantObj(msec)")]
        public static readonly NumericCounter TotalNetUpdateRelevantObj = new NumericCounter();

        [PublishCounter("_1_2_2)TotalNetUpdateSnapShot(msec)")]
        public static readonly NumericCounter TotalNetUpdateSnapShot = new NumericCounter();

        [PublishCounter("_1_2_3)TotalNetSyncRelevantStats(msec)")]
        public static readonly NumericCounter TotalNetSyncRelevantStats = new NumericCounter();

        [PublishCounter("_1_2_4)TotalNetDispatchChat(microsec)")]
        public static readonly NumericCounter TotalNetDispatchChat = new NumericCounter();        

        [PublishCounter("_1_3)TotalResetSyncStats(microsec)")]
        public static readonly NumericCounter TotalResetSyncStats = new NumericCounter();

        [PublishCounter("_ExecutionFiberQueue")]
        public static readonly NumericCounter ExecutionFiberQueue = new NumericCounter();

        [PublishCounter("_ProxyMethod")]
        public static readonly NumericCounter ProxyMethod = new NumericCounter();

        //[PublishCounter("_ShuangFeiDamageResultsUpdate(microsec)")]
        public static readonly NumericCounter ShuangFeiDamageResultsUpdate = new NumericCounter();
        
        //[PublishCounter("_ShuangFeiUpdateEntitySyncStats(microsec)")]
        public static readonly NumericCounter ShuangFeiUpdateEntitySyncStats = new NumericCounter();

        [PublishCounter("_1_2_3_1)TotalDamageResultsUpdate(microsec)")]
        public static readonly NumericCounter TotalDamageResultsUpdate = new NumericCounter();

        [PublishCounter("_1_2_3_2)TotalUpdateEntitySyncStats(microsec)")]
        public static readonly NumericCounter TotalUpdateEntitySyncStats = new NumericCounter();


        //[PublishCounter("_SnapShotPrepareTime(microsec)")]
        public static readonly NumericCounter ShuangFeiSnapShotPrepareTime = new NumericCounter();

        //[PublishCounter("_SnapShotSendTime(microsec)")]
        public static readonly NumericCounter ShuangFeiSnapShotSendTime = new NumericCounter();

        [PublishCounter("_1_2_2_1)TotalSnapShotPrepareTime(microsec)")]
        public static readonly NumericCounter TotalSnapShotPrepareTime = new NumericCounter();

        [PublishCounter("_1_2_2_2)TotalSnapShotSendTime(microsec)")]
        public static readonly NumericCounter TotalSnapShotSendTime = new NumericCounter();        
    }
}
using System;
using System.Collections.Generic;
using System.Timers;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer.RandomBoxReward
{
    class RandomBoxRewardData
    {
        private int id = 0;
        private DateTime expritTime;
        private DateTime latestActiveTime;
        private int rewardGroupId = 0;
        private List<ItemInfo> rewardList;
        private Dictionary<CurrencyType, int> currency;

        #region property
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public DateTime ExpritTime {
            get { return expritTime; }
            set { expritTime = value; }
        }

        public DateTime LatestActiveTime
        {
            get { return latestActiveTime; }
            set { latestActiveTime = value; }
        }

        public int RewardGroupId
        {
            get { return rewardGroupId; }
            set { rewardGroupId = value; }
        }

        /// <summary>
        /// for write Log easy, get all reward items from kopio data (contain id:0 ro count: 0).
        /// </summary>
        public List<ItemInfo> RewardList
        {
            get { return rewardList; }
        }

        public bool HasReward
        {
            get { return (rewardList.Count > 0); }
        }
        #endregion

        public RandomBoxRewardData ()
        {
            
        }

        /// <summary>
        /// Get inventory ItemInfo list to multi add item
        /// </summary>
        /// <returns></returns>
        public void GetRewardItems()
        {
            rewardList = new List<ItemInfo>();
            Dictionary<CurrencyType, int> currency = new Dictionary<CurrencyType, int>();
            GameRules.GenerateRewardGrp(rewardGroupId, rewardList, currency);
        }
    }

    class RandomBoxRewardManager
    {
        private static volatile RandomBoxRewardManager instance;
        private static object syncRoot = new object();

        Timer timer = null;
        RandomBoxRewardData rewardData;

        public static RandomBoxRewardManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new RandomBoxRewardManager();
                        }
                    }
                }

                return instance;
            }
        }

        public void Init()
        {
            // add json data
            var infos = RandomBoxRewardRepo.GetRandomBoxRewardInfos();
            if (infos.Length > 0)
            {
                rewardData = new RandomBoxRewardData();
                RandomBoxRewardJson info = infos[0];
                rewardData.Id = info.id;
                rewardData.RewardGroupId = info.rewardgroupid;

                // start timer
                SetNextActive();
            }
        }

        void SetNextActive()
        {
            if (rewardData != null)
            {
                SetNextActiveTime();
                rewardData.GetRewardItems();
                SetNextActiveTimer();
            }
        }

        void SetNextActiveTime()
        {
            lock (syncRoot)
            {
                var now = DateTime.Now;
                var info = RandomBoxRewardRepo.GetRandomBoxRewardInfoById(rewardData.Id);
                if (info != null)
                {
                    Random rand = GameUtils.GetRandomGenerator();
                    var diff = info.boxtimemax - info.boxtimemin;
                    var nextMinutes = rand.Next(0, diff) + info.boxtimemin;
                    rewardData.LatestActiveTime = now.AddMinutes(nextMinutes);
                    rewardData.ExpritTime = now.AddMinutes(nextMinutes + info.boxcontinued);
                }
            }    
        }

        long GetLastActiveTime()
        {
            return rewardData.LatestActiveTime.Ticks;
        }

        public void SendCurrentStateToClient(GameClientPeer peer)
        {
            if (CheckInActive() == true)
            {
                if (peer.mPlayer.SecondaryStats.RandomBoxTimeTick < rewardData.LatestActiveTime.Ticks)
                {
                    if (SystemSwitch.mSysSwitch.IsOpen(SysSwitchType.RandomBox))
                        peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.RandomBoxRewardActive, GetBoxStringInfo(rewardData.Id.ToString(), rewardData.ExpritTime.Ticks.ToString()), peer);
                }
            }
            else
                peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.RandomBoxRewardExprie, GetBoxStringInfo(rewardData.Id.ToString(), rewardData.LatestActiveTime.Ticks.ToString()), peer);
        }

        bool CheckInActive()
        {
            var now = DateTime.Now;
            if (now < rewardData.LatestActiveTime)
                return false;

            if (now >= rewardData.ExpritTime)
                return false;

            return true;
        }

        bool CheckRewards()
        {
            return rewardData.HasReward;
        }

        bool CheckCanReceive(Player player)
        {
            var laTicks = rewardData.LatestActiveTime.Ticks;
            if (player.SecondaryStats.RandomBoxTimeTick >= laTicks)
                return false;

            return true;
        }

        public void SetActive(bool active) {
            if (active && !CheckInActive())
            {
                if (rewardData != null)
                {
                    var info = RandomBoxRewardRepo.GetRandomBoxRewardInfoById(rewardData.Id);
                    if (info != null) {
                        var now = DateTime.Now;
                        rewardData.LatestActiveTime = now;
                        rewardData.ExpritTime = now.AddMinutes(info.boxcontinued);
                        rewardData.GetRewardItems();
                        OnActived(null, null);
                    }
                }
            }
            else if (!active && CheckInActive())
            {
                OnExpried(null, null);
            }
        }

        bool AddRewards(Player player)
        {
            var rewards = rewardData.RewardList;
            if (rewards.Count == 0)
                return false;

            ItemInventoryController inventory = player.Slot.mInventory;
            if (inventory.GetEmptySlotCount() < rewards.Count)
                return false;

            foreach (ItemInfo itemInfo in rewards)
            {
                IInventoryItem item = GameRules.GenerateItem(itemInfo.itemId, player.Slot, itemInfo.stackCount);
                InvRetval res = inventory.AddItemsIntoInventory(item, true, "RandomBox");
                if (res.retCode != InvReturnCode.AddSuccess)
                    return false;

                // set last get time tick
                player.SecondaryStats.RandomBoxTimeTick = DateTime.Now.Ticks;
                
                Zealot.Logging.Client.LogClasses.RandomBoxReward2 randomBoxRewardLog = new Zealot.Logging.Client.LogClasses.RandomBoxReward2();
                randomBoxRewardLog.userId = player.Slot.mUserId;
                randomBoxRewardLog.charId = player.Slot.GetCharId();
                randomBoxRewardLog.message = string.Format("RandomBoxReward id = {0}, item id = {1}, count = {2}", rewardData.Id, itemInfo.itemId, itemInfo.stackCount);
                randomBoxRewardLog.BoxId = rewardData.Id;
                randomBoxRewardLog.ItemId = itemInfo.itemId;
                randomBoxRewardLog.ItemCount = itemInfo.stackCount;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(randomBoxRewardLog);
            }

            player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.RandomBoxTimes);

            return true;
        }

        /// <summary>
        /// Check passable and backpack enough and get items
        /// </summary>
        /// <param name="reward_id"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public int GetRewards(int reward_id, Player player)
        {
            if (CheckInActive() == false)
                return 2;   // expired

            if (CheckRewards() == false)
                return 0;   // no rewards

            if (CheckCanReceive(player) == false)
            {
                return 4;   // already get
            }

            if (AddRewards(player) == false)
                return 3;   // full
            else
                return 1;   // success
        }

        /// <summary>
        /// for broadcast message to player
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        string GetBoxStringInfo(params string[] param)
        {
            string strInfo = "";
            if (param.Length > 0)
            {
                strInfo = String.Join(";", param);
            }

            return strInfo;
        }

        void SetNextActiveTimer()
        {
            if (timer != null)
                timer.Stop();

            var t = new TimeSpan(rewardData.LatestActiveTime.Ticks - DateTime.Now.Ticks);
            timer = new Timer(t.TotalMilliseconds);
            timer.Elapsed += OnActived;
            timer.AutoReset = false;
            timer.Start();

        }

        // Box Reward Actived
        void OnActived(object sender, ElapsedEventArgs e)
        {
            // set exprie timer
            SetNextOnExprieTimer();

            // broadcast exprie time to client
            if (SystemSwitch.mSysSwitch.IsOpen(SysSwitchType.RandomBox))
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.RandomBoxRewardActive, GetBoxStringInfo(rewardData.Id.ToString(), rewardData.ExpritTime.Ticks.ToString()));
        }

        void SetNextOnExprieTimer()
        {
            if (timer != null)
                timer.Stop();

            var t = new TimeSpan(rewardData.ExpritTime.Ticks - DateTime.Now.Ticks);
            timer = new Timer(t.TotalMilliseconds);
            timer.Elapsed += OnExpried;
            timer.AutoReset = false;
            timer.Start();

        }

        // Box Reward Expried
        void OnExpried(object sender, ElapsedEventArgs e)
        {
            SetNextActive();

            // broadcast next active time to client
            GameApplication.Instance.BroadcastMessage(BroadcastMessageType.RandomBoxRewardExprie, GetBoxStringInfo(rewardData.Id.ToString(), rewardData.LatestActiveTime.Ticks.ToString())); 
        }
    }
}

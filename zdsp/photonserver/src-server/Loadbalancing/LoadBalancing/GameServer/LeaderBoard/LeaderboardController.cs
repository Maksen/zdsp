//#define TEST_LEADERBOARD

using System;
using System.Collections.Generic;
using ExitGames.Concurrency.Fibers;
using Zealot.Common;
using ExitGames.Logging;
using System.Threading.Tasks;
using Zealot.Server.Rules;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using Newtonsoft.Json;

namespace Photon.LoadBalancing.GameServer
{
    public abstract class BaseLeaderBoard
    {
        public int MinimumScore { get; protected set; }

        public readonly int rankingCount;
        protected readonly string storedProc;
        protected DateTime nextUpdate;

        private LeaderboardType lbType;
        public LeaderboardType LBType { get { return lbType; } }

        public delegate void OnProcessAvatar(string charname, string characterdata, LeaderboardType type);
        public OnProcessAvatar ProcessAvatar;

        public LeaderBoardData lbData;
        protected string mSerializedData = "";

        public BaseLeaderBoard(LeaderboardType lbType, int count, string storedProc)
        {
            this.lbType = lbType;
            this.rankingCount = count;
            this.storedProc = storedProc;

            lbData = new LeaderBoardData();
            lbData.lbType = lbType;
        }

        public virtual void LoadFromDB()
        {
            var results = GameApplication.dbRepository.Ladder.GetLeaderBoard((int)LBType);
            if (results != null)
            {
                DateTime dtupdated = (DateTime)results["dtupdate"];
                string data = (string)results["data"];
                lbData = LeaderBoardData.Deserialize(data);
                mSerializedData = data;
            }
            UpdateNextUpdate();
        }

        public void ClearData()
        {
            lbData.lbRanking.Clear();
            mSerializedData = "";
        }

        protected virtual void UpdateNextUpdate()
        {
            //default 5 am everyday.
            DateTime lastupdate = (lbData.lastupdate == DateTime.MinValue) ? DateTime.Now : lbData.lastupdate;
            if (lastupdate.Hour >= 5 || (lastupdate.Hour == 4 && lastupdate.Minute >= 57))
                nextUpdate = lastupdate.Date.AddDays(1).AddHours(5);
            else
                nextUpdate = lastupdate.Date.AddHours(5);
        }

        public virtual string GetSerializedData()
        {
            if (string.IsNullOrEmpty(mSerializedData))
                mSerializedData = lbData.GetSerialized();
            return mSerializedData;
        }

        protected bool CanUpdate()
        {
#if TEST_LEADERBOARD
            return true;
#else
            return DateTime.Now >= nextUpdate;
#endif
        }

        public virtual async Task ProcessUpdate(bool force)
        {
            if (force || CanUpdate())
            {
                List<Dictionary<string, object>> tempResults = await GameApplication.dbRepository.Ladder.GetLeaderBoardAsync(storedProc, rankingCount);
                if (tempResults != null)
                {
                    GameApplication.Instance.executionFiber.Enqueue(
                        async () =>
                        {
                            lbData.lbRanking.Clear();
                            mSerializedData = "";
                            RebuildData(tempResults);
                            DateTime old = lbData.lastupdate;
                            lbData.lastupdate = DateTime.Now;
                            await SaveToDB(old);
                        });
                }
            }
        }

        protected virtual async Task SaveToDB(DateTime old)
        {
            GetSerializedData();
            bool success = await GameApplication.dbRepository.Ladder.SaveLeaderBoardAsync((int)LBType, mSerializedData);
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                if (success)
                    UpdateNextUpdate();
                else
                {
                    lbData.lastupdate = old;
                    LeaderboardController.Log.Info(string.Format("Failed Saving LeaderBoard [{0}]", LBType.ToString()));
                }
            });
        }

        protected abstract void RebuildData(List<Dictionary<string, object>> tempResults);
    }

    public class LeaderBoardAvatar
    {
        public LeaderboardType lbType;
        private string strAvatarData { get; set; } //data which is really to send to client
        private string characterData; //raw data

        public LeaderBoardAvatar(string chardata, LeaderboardType lbType)
        {
            this.lbType = lbType;
            SetCharacterData(chardata);
        }

        public void SetCharacterData(string chardata)
        {
            characterData = chardata;
            strAvatarData = null;
        }

        public string GetClientSerializedEquippedInv()
        {
            if (strAvatarData == null)
            {
                var charData = CharacterData.DeserializeFromDB(characterData);
                switch (lbType)
                {
                    case LeaderboardType.Pet:
                        //PetRules.PetInfo petInfo = charData.PetDataInv.PetList.Find(x => x.IsUsing == true);
                        strAvatarData = ""; //(petInfo == null) ? "" : JsonConvert.SerializeObject(petInfo);
                        break;
                    default:
                        CharacterInspectData avatarData = new CharacterInspectData();
                        avatarData.Name = charData.Name;
                        avatarData.JobSect = charData.JobSect;
                        avatarData.EquipmentInventory = charData.EquipmentInventory;
                        avatarData.ProgressLevel = charData.ProgressLevel;
                        avatarData.Faction = charData.Faction;
                        if (charData.GuildId > 0)
                            avatarData.Guild = GuildRules.GetGuildNameById(charData.GuildId);
                        avatarData.VIP = charData.CurrencyInventory.VIPLevel;
                        avatarData.CombatScore = charData.EquipScore;
                        avatarData.InspectCombatStats = charData.InspectCombatStats;
                        strAvatarData = avatarData.SerializeForCharCreation();
                        break;
                }
            }
            return strAvatarData;
        }
    }

    #region Controller
    public class LeaderboardController
    {
        private readonly PoolFiber executionFiber;
        public static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private Dictionary<LeaderboardType, BaseLeaderBoard> leaderboards;
        private Dictionary<string, Dictionary<LeaderboardType, LeaderBoardAvatar>> avatarList = new Dictionary<string, Dictionary<LeaderboardType, LeaderBoardAvatar>>();

        IDisposable regularlyUpdate;

        public LeaderboardController()
        {
            CreateLeaderBoards();            
            this.executionFiber = GameApplication.Instance.executionFiber;
        }

        public void Init()
        {
            LoadFromDB();           
            InitRefreshTimes();
        }

        private void LoadFromDB()
        {
            //load leaderboard data from database
            //run synchronously, always load gearscore first
            foreach (var leaderboard in leaderboards.Values)
            {
                leaderboard.LoadFromDB();
            }
            InitArenaLeaderboard();
            OnRegularUpdate();
        }

        private void InitRefreshTimes()
        {
            var timeOfDay = DateTime.Now.TimeOfDay;
            var nextHour = TimeSpan.FromHours(Math.Ceiling(timeOfDay.TotalHours));
            var nextHourDuration = (nextHour - timeOfDay).TotalMilliseconds + 500;
            if (nextHourDuration < 1000)
                nextHourDuration += 180000;

            //check every 30 minutes.
#if TEST_LEADERBOARD
            regularlyUpdate = this.executionFiber.ScheduleOnInterval(OnRegularUpdate, 5000, 180000);
#else
            regularlyUpdate = this.executionFiber.ScheduleOnInterval(OnRegularUpdate, (long)nextHourDuration, 180000);
#endif          
        }

        /// <summary>
        /// saves leaderboards synchronously, will block current thread
        /// </summary>
        public void Stop()
        {
            if (regularlyUpdate != null)
            {
                regularlyUpdate.Dispose();
                regularlyUpdate = null;
            }
        }

        public void Dispose()
        {
        }

        private void CreateLeaderBoards()
        {
            leaderboards = new Dictionary<LeaderboardType, BaseLeaderBoard>();
            
            leaderboards.Add(LeaderboardType.Guild, new GuildLeaderBoard());
            leaderboards.Add(LeaderboardType.HeroHouse, new GuildHeroHouseLeaderBoard());
            //leaderboards.Add(LeaderboardType.FactionWar, new FactionWarLeaderBoard());           
            leaderboards.Add(LeaderboardType.GearScore, new GearScoreLeaderBoard());
            leaderboards.Add(LeaderboardType.HeroBook, new HeroBookLeaderBoard());
            leaderboards.Add(LeaderboardType.Gold, new GoldLeaderBoard());
            leaderboards.Add(LeaderboardType.Pet, new PetLeaderBoard());
            leaderboards.Add(LeaderboardType.Arena, new ArenaLeaderBoard());
            leaderboards.Add(LeaderboardType.FactionKill, new FactionKillLeaderBoard());
            leaderboards.Add(LeaderboardType.FactionDeath, new FactionDeathLeaderBoard());
            leaderboards.Add(LeaderboardType.GuildRankAll, new GuildRankLeaderBoard());
            leaderboards.Add(LeaderboardType.FactionRecommend, new FactionRecommendLeaderBoard());
            leaderboards.Add(LeaderboardType.WorldLevel, new WorldLevelLeaderBoard());
            leaderboards.Add(LeaderboardType.WorldBossRecord, new WorldBossRecordLeaderBoard());

            leaderboards[LeaderboardType.GearScore].ProcessAvatar = ProcessAvatar;
            leaderboards[LeaderboardType.Pet].ProcessAvatar = ProcessAvatar;
        }

        #region Scheduled Update
        private void OnRegularUpdate()
        {
            executionFiber.Enqueue(async () => await ProcessUpdate(false).ConfigureAwait(false));
        }

        private async Task ProcessUpdate(bool force)
        {
            Log.Info("LeaderBoard Start ProcessUpdate");
            foreach (var leaderboard in leaderboards.Values)
            {
                if (leaderboard.LBType != LeaderboardType.Guild && leaderboard.LBType != LeaderboardType.Arena
                    && leaderboard.LBType != LeaderboardType.WorldBossRecord)
                    await leaderboard.ProcessUpdate(force).ConfigureAwait(false);
            }
            Log.Info("LeaderBoard Finish ProcessUpdate");
        }
        #endregion

        #region Avatar
        public void ProcessAvatar(string charname, string characterdata, LeaderboardType type)
        {
            if (string.IsNullOrEmpty(charname))
            {
                Log.Error("ProcessAvatar: invalid char name");
                return;
            }

            if (!avatarList.ContainsKey(charname))
                avatarList.Add(charname, new Dictionary<LeaderboardType, LeaderBoardAvatar>());
            if (avatarList[charname].ContainsKey(type))
                avatarList[charname][type].SetCharacterData(characterdata);
            else
            {
                switch (type)
                {
                    case LeaderboardType.Pet:
                        avatarList[charname].Add(type, new LeaderBoardAvatar(characterdata, type));
                        break;
                    default:
                        avatarList[charname].Add(LeaderboardType.GearScore, new LeaderBoardAvatar(characterdata, LeaderboardType.GearScore));
                        break;
                }
            }
        }

        public LeaderBoardAvatar GetLeaderBoardAvatar(string charname, LeaderboardType lbType)
        {
            if (avatarList.ContainsKey(charname))
            {
                if (avatarList[charname].ContainsKey(lbType))
                    return avatarList[charname][lbType];
            }
            return null;
        }

        public void ForceRefreshLeaderBoard()
        {
            var task = ProcessUpdate(true);
        }

        public BaseLeaderBoard GetLeaderBoard(LeaderboardType lbType)
        {
            if (leaderboards.ContainsKey(lbType))
                return leaderboards[lbType];
            return null;
        }

        public FactionType GetFactionRecommend()
        {
            FactionRecommendLeaderBoard leaderBoard = leaderboards[LeaderboardType.FactionRecommend] as FactionRecommendLeaderBoard;
            return leaderBoard.mFactionRecommend;
        }

        public int GetWorldLevel()
        {
            WorldLevelLeaderBoard leaderBoard = leaderboards[LeaderboardType.WorldLevel] as WorldLevelLeaderBoard;
            return leaderBoard.mWorldLevel;
        }
        #endregion

        #region PlayerRPCs
        public void PlayerRequestLeaderboard(GameClientPeer peer, byte lbType)
        {
            if (!Enum.IsDefined(typeof(LeaderboardType), lbType))
                return;
            LeaderboardType type = (LeaderboardType)lbType;
            if (leaderboards.ContainsKey(type))
            {
                var leaderboard = leaderboards[type];
                string lbData = leaderboard.GetSerializedData();
                peer.ZRPC.NonCombatRPC.Ret_GetLeaderBoard(lbType, lbData, peer);
            }
            else
                peer.ZRPC.NonCombatRPC.Ret_GetLeaderBoard(lbType, "", peer);
        }

        public void PlayerRequestAvatar(GameClientPeer peer, byte lbType, string charName)
        {
            if(string.IsNullOrEmpty(charName))
                peer.ZRPC.NonCombatRPC.Ret_GetLeaderBoardAvatar(lbType, "", peer);
            else
            {
                LeaderboardType type = (LeaderboardType)lbType;
                var avatarData = GetLeaderBoardAvatar(charName, type);              
                if (avatarData != null)
                {
                    string strData = avatarData.GetClientSerializedEquippedInv();
                    peer.ZRPC.NonCombatRPC.Ret_GetLeaderBoardAvatar(lbType, strData, peer);
                }
            }
        }
        #endregion

        #region Arena Ranking
        public void InitArenaLeaderboard()
        {
            List<ArenaPlayerRecord> records = LadderRules.mLadderArenaRecord.PlayerRecords;
            var leaderboard = leaderboards[LeaderboardType.Arena] as ArenaLeaderBoard;
            leaderboard.InitArenaRank(records);
        }

        public void OnArenaRankChange(int newrank, ArenaPlayerRecord record)
        {
            var leaderboard = leaderboards[LeaderboardType.Arena] as ArenaLeaderBoard;
            leaderboard.UpdateArenaRank(newrank, record);
        }
        #endregion

        #region World Boss Leader

        public async void SetWorldBossRecordData(KeyValuePair<string, int>[] data)
        {
            var leaderboard = leaderboards[LeaderboardType.WorldBossRecord] as WorldBossRecordLeaderBoard;
            await leaderboard.ManualUpdate(data);
        }

        #endregion
    }
    #endregion
}

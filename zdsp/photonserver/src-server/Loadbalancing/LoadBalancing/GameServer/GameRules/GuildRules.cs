using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using Newtonsoft.Json;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.Datablock;
using Zealot.Server.Entities;
using Zealot.Repository;
using UnityEngine;

namespace Zealot.Server.Rules
{
    public class GuildMemberRequestRecord
    {
        public int guildId;
        public DateTime requestDT;

        public GuildMemberRequestRecord(int guildId, DateTime requestDT)
        {
            this.guildId = guildId;
            this.requestDT = requestDT;
        }
    }

    public static class GuildRules
    {
        public static Dictionary<int, GuildStatsServer> GuildList = null;
        public static Dictionary<string, int> GuildNameToIdMap = new Dictionary<string, int>();
        public static Dictionary<string, List<GuildMemberRequestRecord>> GuildMemberRequestMap = new Dictionary<string, List<GuildMemberRequestRecord>>();
        public static Dictionary<string, int> GuildIdByPlayer = new Dictionary<string, int>(); 
        public static DateTime bossPrevResetDT;
        public static GuildData defaultGuildData = new GuildData();
        public static string defaultGuildDataStr = "";

		public static readonly int saveDBInterval = 10000; // DB save interval in ms
        public static readonly int UpdateInterval = 500;

        private static int serverId;
        private static readonly PoolFiber executionFiber;
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private static IList<int> saveDBGuildIdList = null;
        private static int currSaveDbIdx = 0;     

        static GuildRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
            defaultGuildDataStr = defaultGuildData.SerializeForDB();
        }

        public static void Init() 
        {
            serverId = GameApplication.Instance.GetMyServerline();

            // Initialization done only once on startup
            GuildList = new Dictionary<int, GuildStatsServer>();
            saveDBGuildIdList = new List<int>();

            // Init boss reset time
            DateTime now = DateTime.Today;
            int diffFrmMonday = (int)DayOfWeek.Monday - (int)now.DayOfWeek;
            diffFrmMonday = (diffFrmMonday < 0) ? diffFrmMonday : -diffFrmMonday;
            bossPrevResetDT = now.AddDays(diffFrmMonday); // prev boss reset datetime

            InitGuildListFromDB();            
        }

        public static GuildStatsServer GetGuildById(int id)
        {
            if (GuildList.ContainsKey(id))
                return GuildList[id];
            return null;
        }

        public static int GetIdByGuildName(string guildName)
        {
            if (GuildNameToIdMap.ContainsKey(guildName))
                return GuildNameToIdMap[guildName];
            return 0;
        }

        public static string GetGuildNameById(int id)
        {
            if (GuildList.ContainsKey(id))
                return GuildList[id].name;
            return "";
        }

        public static int GetGuildIdByPlayer(string playerName)
        {
            if (GuildIdByPlayer.ContainsKey(playerName))
                return GuildIdByPlayer[playerName];
            return 0;
        }

        public static string GetGuildNameByPlayer(string playerName)
        {
            if (GuildIdByPlayer.ContainsKey(playerName))
                return GetGuildNameById(GuildIdByPlayer[playerName]);
            return "";
        }

        public static bool AddToGuildStatsList(int id, string gName, int guildIcon, string leader, byte faction, GuildMemberStats gLeaderStats)
        {
            if(!GuildList.ContainsKey(id))
            {
                GuildStatsServer gInfo = new GuildStatsServer();
                gInfo.guildId = id;
                gInfo.name = gName;
                gInfo.guildIcon = guildIcon;
                gInfo.guildIconFree = true;
                gInfo.guildLevel = 1;
                gInfo.guildGold = 0;
                gInfo.faction = faction;
                gInfo.totalCombatScore = 0;
                gInfo.guildNotice = "";               
                gInfo.SMBossLevel = 1;
                gInfo.SMBossDmgDone = 0;
                gInfo.SMBossAttacker = "";
                gInfo.SMBossRoomGuid = "";
                gInfo.DreamHouseFavourability = 0;
                gInfo.guildBossLastResetTick = bossPrevResetDT.Ticks;
                gInfo.guildSecretShopTick = 0;
                AddMemberToGuildStats(gInfo, gLeaderStats, false);
                gInfo.mGuildDataStr = defaultGuildDataStr;
                gInfo.requestSetting = gInfo.mRequestSetting.ToString();
                gInfo.mGuildTechDict[GuildTechType.Quest] = 1;
                gInfo.mGuildTechDict[GuildTechType.Love] = 1;
                gInfo.UpdateTechString();
                gInfo.saveToDB = true;
                GuildList[id] = gInfo;
                GuildNameToIdMap[gName] = id;
                saveDBGuildIdList.Add(id); // Add to save to db list
                return true;
            }
            return false;
        }

        public static void AddMemberToGuildStats(GuildStatsServer guildStats, GuildMemberStats memberStats, bool join)
        {
            string playerName = memberStats.name;
            guildStats.GetMemberStatsDict()[playerName] = memberStats;
            guildStats.members[memberStats.localObjIdx] = memberStats.ToString();
            guildStats.totalCombatScore += memberStats.combatScore;
            if (join)
            {
                guildStats.AddHistory(GuildHistoryType.JoinGuild, playerName);

                //log
                string message = string.Format("id:{0}|name:{1}", guildStats.guildId, playerName);
                Zealot.Logging.Client.LogClasses.GuildJoin guildJoinLog = new Zealot.Logging.Client.LogClasses.GuildJoin();
                guildJoinLog.message = message;
                guildJoinLog.guildid = guildStats.guildId;
                guildJoinLog.charName = playerName;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildJoinLog);
            }
            GuildIdByPlayer[playerName] = guildStats.guildId;
        }

        public static void RemoveMemberFromGuildStats(GuildStatsServer guildStats, GuildMemberStats memberStats, bool kicked)
        {
            string playerName = memberStats.name;
            guildStats.GetMemberStatsDict().Remove(playerName);
            guildStats.members[memberStats.localObjIdx] = null;
            guildStats.totalCombatScore -= memberStats.combatScore;
            if (kicked)
                guildStats.AddHistory(GuildHistoryType.KickGuild, playerName);
            else
                guildStats.AddHistory(GuildHistoryType.LeaveGuild, playerName);
            GuildIdByPlayer.Remove(playerName);
        }

        public static void AddMemberRequestToGuildStats(GuildStatsServer guildStats, GuildMemberStatsRequest requestStats)
        {
            string playerName = requestStats.name;
            guildStats.GetMemberRequestsDict()[playerName] = requestStats;
            guildStats.memberRequests[requestStats.localObjIdx] = requestStats.ToString();
            if (!GuildMemberRequestMap.ContainsKey(playerName))
                GuildMemberRequestMap.Add(playerName, new List<GuildMemberRequestRecord>());
            GuildMemberRequestMap[playerName].Add(new GuildMemberRequestRecord(guildStats.guildId, requestStats.requestDT));
        }

        public static void InitGuildListFromDB()
        {
            DateTime today = DateTime.Today;
            List<Dictionary<string, object>> guildListDB = GameApplication.dbRepository.Guild.GetAllGuilds(serverId).Result;
            foreach(var guildInfo in guildListDB)
            {
                GuildStatsServer newGuildStats = new GuildStatsServer();
                newGuildStats.guildId = (int)guildInfo["guildid"];
                newGuildStats.name = (string)guildInfo["guildname"];
                newGuildStats.guildIcon = (int)guildInfo["guildicon"];
                newGuildStats.guildLevel = (byte)guildInfo["guildlevel"];
                newGuildStats.guildGold = (long)guildInfo["guildgold"];
                newGuildStats.faction = (byte)guildInfo["faction"];
                newGuildStats.guildNotice = (string)guildInfo["guildnotice"];
                newGuildStats.mGuildHistory = new GuildHistory();
                newGuildStats.mGuildHistory.Init((string)guildInfo["guildhistory"]);
                newGuildStats.techs = (string)guildInfo["techs"];
                newGuildStats.ParseTech();
                newGuildStats.SMBossLevel = (int)guildInfo["smbosslvl"];
                newGuildStats.SMBossDmgDone = (int)guildInfo["smbossdmgdone"];
                newGuildStats.DreamHouseFavourability = (int)guildInfo["dhfavourability"];
                newGuildStats.guildBossLastResetTick = (guildInfo["dtbosslastreset"] != DBNull.Value) 
                                                        ? ((DateTime)guildInfo["dtbosslastreset"]).Ticks : 0;
                newGuildStats.guildSecretShopTick = (guildInfo["dtsecretshop"] != DBNull.Value)
                                                    ? ((DateTime)guildInfo["dtsecretshop"]).Ticks : 0;

                int guildId = newGuildStats.guildId;
                // Member
                List<Dictionary<string, object>> dbMembersInfo = GameApplication.dbRepository.Character.GetByGuildId(guildId).Result;
                if (dbMembersInfo.Count == 0)
                {
                    Log.InfoFormat("InitGuild remove dirty guild = {0}, name = {1}", guildId, newGuildStats.name);
                    var _removeDirtyGuld = GameApplication.dbRepository.Guild.DeleteGuild(guildId);
                    continue;
                }
                Dictionary<string, GuildMemberStats> MemberStatsDict = newGuildStats.GetMemberStatsDict();
                for (int i = 0; i < dbMembersInfo.Count; i++)
                {
                    Dictionary<string, object> memberInfo = dbMembersInfo[i];
                    int fundtoday = (int)memberInfo["fundtoday"];
                    if ((DateTime)memberInfo["dtupdated"] < today)
                        fundtoday = 0;
                    AddMemberToGuildStats(newGuildStats,
                        new GuildMemberStats((byte)memberInfo["guildrank"], (string)memberInfo["charname"], fundtoday, (long)memberInfo["fundtotal"], (int)memberInfo["combatscore"], false, i),
                        false);
                }

                // Member request
                string guildDataStr = (string)guildInfo["guilddata"];                
                GuildData guildData = GuildData.DeserializeFromDB(guildDataStr);
                if (guildData == null)
                {
                    guildData = new GuildData();
                    guildDataStr = defaultGuildDataStr;
                }
                else
                {
                    //cleanup for those dirty data.
                    List<string> _donateRequestCDNameList = guildData.donateRequestCDTime.Keys.ToList();
                    for (int index = 0; index < _donateRequestCDNameList.Count; index++)
                    {
                        string _name = _donateRequestCDNameList[index];
                        if (!MemberStatsDict.ContainsKey(_name))
                            guildData.donateRequestCDTime.Remove(_name);
                    }
                    List<string> _donateRequestList = guildData.DonateInvData.donateRequestDic.Keys.ToList();
                    for (int index = 0; index < _donateRequestList.Count; index++)
                    {
                        string _name = _donateRequestList[index];
                        if (!MemberStatsDict.ContainsKey(_name))
                            guildData.DonateInvData.donateRequestDic.Remove(_name);
                    }
                    List<string> _donateRequestRemainingList = guildData.DonateInvData.donateRemainingTimes.Keys.ToList();
                    for (int index = 0; index < _donateRequestRemainingList.Count; index++)
                    {
                        string _name = _donateRequestRemainingList[index];
                        if (!MemberStatsDict.ContainsKey(_name))
                            guildData.DonateInvData.donateRemainingTimes.Remove(_name);
                    }
                }

                List<MemberRequestElement> mRequestList = guildData.MemberRequestInvData.memberRequestList;
                for(int i = 0; i < mRequestList.Count; i++)
                {
                    MemberRequestElement mRequest = mRequestList[i];
                    AddMemberRequestToGuildStats(newGuildStats, new GuildMemberStatsRequest(mRequest.Name, mRequest.Portrait, mRequest.VIPLevel, mRequest.ProgressLevel, mRequest.CombatScore, mRequest.RequestDT, i));
                }              
                newGuildStats.mGuildDataStr = guildDataStr;
                newGuildStats.guildIconFree = guildData.GuildIconFree;
                newGuildStats.mRequestSetting = guildData.MemberRequestInvData.requestSetting;
                newGuildStats.requestSetting = newGuildStats.mRequestSetting.ToString();
                newGuildStats.donateRequestCDTime = guildData.donateRequestCDTime;
                newGuildStats.memberDonateInv = guildData.DonateInvData;

                GuildList[newGuildStats.guildId] = newGuildStats; // Add to guildlist
                GuildNameToIdMap[newGuildStats.name] = newGuildStats.guildId;
                saveDBGuildIdList.Add(guildId);  // Add to save to db list
                // Reset new week if is outdated
                if(newGuildStats.guildBossLastResetTick != bossPrevResetDT.Ticks)
                    ResetOnNewWeek(newGuildStats);
            }
            foreach (string charName in GuildMemberRequestMap.Keys.ToList()) //reorder request base on requestdt asc.
                GuildMemberRequestMap[charName] = GuildMemberRequestMap[charName].OrderBy(x => x.requestDT).ToList();
        }

        public static void SendGuildListToClient(byte searchFilter, int guildId, string guildname, Player player)
        {                      
            List<GuildInfo> listToSend = new List<GuildInfo>();
            GameClientPeer peer = player.Slot;
            if (searchFilter == (byte)GuildSearchFilter.All)
            {
                byte faction = player.PlayerSynStats.faction;
                int combatscore = 0;
                int progresslvl = player.PlayerSynStats.Level;
                int viplvl = 0;
                List<GuildStatsServer> sortedGuildStats = GuildList.Values.OrderByDescending(x => x.totalCombatScore).ThenByDescending(x => x.guildLevel).
                    Where(x => x.faction == faction && x.mRequestSetting.CombatScore <= combatscore && x.mRequestSetting.ProgressLvl <= progresslvl &&
                    x.mRequestSetting.VipLvl <= viplvl && !x.IsMemberFull()).ToList();
                for (int i = 0; i < sortedGuildStats.Count; i++)
                {
                    GuildStatsServer guild = sortedGuildStats[i];
                    int maxMemberCount = (int)GuildRepo.GetGuildTechByTypeAndLevel(GuildTechType.Level, guild.guildLevel).stats;
                    GuildInfo currGuild = new GuildInfo(guild.guildId, guild.name, guild.faction, guild.guildLevel, guild.totalCombatScore,
                                          guild.GetMemberStatsDict().Count, maxMemberCount, guild.rank);
                    listToSend.Add(currGuild);
                }
            }
            else if (searchFilter == (byte)GuildSearchFilter.GuildId)  // search by guild id
            {         
                GuildStatsServer guild = GuildList.Values.FirstOrDefault(x => x.guildId == guildId);
                if (guild != null)
                {
                    int maxMemberCount = (int)GuildRepo.GetGuildTechByTypeAndLevel(GuildTechType.Level, guild.guildLevel).stats;
                    GuildInfo currGuild = new GuildInfo(guild.guildId, guild.name, guild.faction, guild.guildLevel, guild.totalCombatScore,
                                           guild.GetMemberStatsDict().Count, maxMemberCount, guild.rank);
                    listToSend.Add(currGuild);
                }
            }
            else if (searchFilter == (byte)GuildSearchFilter.GuildName)  // search by guild name
            {
                List<GuildStatsServer> list = GuildList.Values.OrderByDescending(x => x.totalCombatScore).ThenByDescending(x => x.guildLevel).Where(x => x.name.IndexOf(guildname, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    GuildStatsServer guild = list[i];
                    int maxMemberCount = (int)GuildRepo.GetGuildTechByTypeAndLevel(GuildTechType.Level, guild.guildLevel).stats;
                    GuildInfo currGuild = new GuildInfo(guild.guildId, guild.name, guild.faction, guild.guildLevel, guild.totalCombatScore,
                                           guild.GetMemberStatsDict().Count, maxMemberCount, guild.rank);
                    listToSend.Add(currGuild);
                }
            }

            string stringData = JsonConvert.SerializeObject(listToSend);
            peer.ZRPC.CombatRPC.Ret_GuildGetGuildInfo(stringData, peer);
        }

        public static async Task CheckGuildName(string guildName, GameClientPeer peer)
        {
            GuildReturnCode checkNameRetCode = await IsGuildNameValid(guildName);
            executionFiber.Enqueue(() => 
            {
                if (peer != null)
                    peer.ZRPC.CombatRPC.Ret_GuildCheckName((byte)checkNameRetCode, peer);
            });   
        }

        private static async Task<GuildReturnCode> IsGuildNameValid(string guildName)
        {
            // Check for special characters
            Regex regex = new Regex(@"^[\p{IsCJKUnifiedIdeographs}0-9A-Za-z]+$");
            Match match = regex.Match(guildName);
            if (!match.Success)
                return GuildReturnCode.NameHasInvalidCharacter;

            // Check for forbidden words
            string filteredTxt = "";
            if (WordFilterRepo.FilterString(guildName, '*', WordFilterType.Naming, out filteredTxt))
                return GuildReturnCode.NameHasForbiddenWord;

            // Check guild name length
            string tmpStr = guildName.Normalize(NormalizationForm.FormKC);
            if (tmpStr.Length > 6 || tmpStr.Length < 2)
                return GuildReturnCode.NameCharacterLimit;

            if (GuildNameToIdMap.ContainsKey(guildName))
                return GuildReturnCode.NameAlreadyExist;

            // Check whether name already exist across all servers
            var guildList = await GameApplication.dbRepository.Guild.GetGuildByGuildName(guildName);
            if (guildList.Count > 0)
                return GuildReturnCode.NameAlreadyExist;

            return GuildReturnCode.Success;
        }

        public static async Task CreateGuild(string guildName, int guildIcon, GameClientPeer peer)
        {
            string playerName = peer.mPlayer.Name;
            GuildReturnCode checkNameRetCode = await IsGuildNameValid(guildName);
            executionFiber.Enqueue(async () =>
            {
                peer = GameApplication.Instance.GetCharPeer(playerName);
                if (peer == null)
                    return;
                Player player = peer.mPlayer;
                if (player == null)
                    return;
                if (checkNameRetCode != GuildReturnCode.Success)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildAdd((byte)checkNameRetCode, peer);
                    return;
                }
             
                PlayerSynStats pSynStats = player.PlayerSynStats;
                byte faction = pSynStats.faction;
                if (pSynStats.Level < GuildRepo.CreateGuildMinLevel)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildAdd((byte)GuildReturnCode.InsufficientPlayerLevel, peer);
                    return;
                }

                int cost = GuildRepo.GetValue("CreateGuildCost");
                if (!player.IsCurrencySufficient(CurrencyType.Gold, cost))
                {
                    peer.ZRPC.CombatRPC.Ret_GuildAdd((byte)GuildReturnCode.InsufficientGold, peer);
                    return;
                }

                if (DateTime.Now.Ticks < player.SecondaryStats.guildLeaveGuildCDEnd)  // has cooldown
                {
                    peer.ZRPC.CombatRPC.Ret_GuildAdd((byte)GuildReturnCode.LeaveGuildCooldown, peer);
                    return;
                }
               
                int combatscore = 0;
                Tuple<int, bool> result = await GameApplication.dbRepository.Guild.InsertNewGuild(serverId, guildName, guildIcon,
                                                                                            peer.mChar, faction, bossPrevResetDT,
                                                                                            defaultGuildDataStr);
                executionFiber.Enqueue(() =>
                {
                    GameClientPeer leaderPeer = GameApplication.Instance.GetCharPeer(playerName); //properly at this time peer is null.
                    if (result.Item2) // Insert to dictionary if db success
                    {
                        int guildId = result.Item1;
                        Player leaderPlayer = leaderPeer != null ? leaderPeer.mPlayer : null;
                        bool success = false;
                        if (leaderPlayer != null && leaderPlayer.IsCurrencySufficient(CurrencyType.Gold, cost))
                        {
                            GuildMemberStats gMemberStats = new GuildMemberStats((byte)GuildRankType.Leader, playerName, 0, 0, combatscore, true, 0);
                            if (AddToGuildStatsList(guildId, guildName, guildIcon, playerName, faction, gMemberStats))
                            {
                                success = true;
                                int goldbefore = leaderPlayer.SecondaryStats.Gold;
                                int bindgoldbefore = leaderPlayer.SecondaryStats.bindgold;
                                RemoveAllRequest(playerName);
                                leaderPlayer.OnCreateGuild(GuildList[guildId]);

                                //log
                                string message = string.Format("id:{0}|name:{1}|creator:{2}|icon:{3}", guildId, guildName, playerName, guildIcon);
                                Zealot.Logging.Client.LogClasses.GuildCreate guildCreateLog = new Zealot.Logging.Client.LogClasses.GuildCreate();
                                guildCreateLog.userId = leaderPeer.mUserId;
                                guildCreateLog.charId = leaderPeer.GetCharId();
                                guildCreateLog.message = message;
                                guildCreateLog.guildid = guildId;
                                guildCreateLog.guildName = guildName;
                                guildCreateLog.charName = playerName;
                                guildCreateLog.icon = guildIcon;
                                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildCreateLog);
                            }
                        }
                        if (!success)
                        {
                            Log.InfoFormat("CreatGuild remove dirty guild = {0}, name = {1}", guildId, guildName);
                            var _removeDirtyGuld = GameApplication.dbRepository.Guild.DeleteGuild(guildId);
                        }
                    }
                    else if (leaderPeer != null)
                        leaderPeer.ZRPC.CombatRPC.Ret_GuildAdd((byte)GuildReturnCode.Failed, leaderPeer);
                });
            });     
        }

        public static async Task DeleteGuild(int id)
        {
            bool result = await GameApplication.dbRepository.Guild.DeleteGuild(id);
            executionFiber.Enqueue(() => 
            {
                if(result && GuildList.ContainsKey(id))
                {
                    string guildName = GuildList[id].name;
                    RemoveMemberRequestAll(GuildList[id]);                       
                    GuildList.Remove(id);
                    GuildNameToIdMap.Remove(guildName);
                    int removedIdx = saveDBGuildIdList.IndexOf(id);
                    saveDBGuildIdList.RemoveAt(removedIdx);
                    if(currSaveDbIdx <= removedIdx && currSaveDbIdx > 0)
                        --currSaveDbIdx;
                }
            });
        }       
        
        public static void JoinGuildCheck(int guildId, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            string playerName = player.Name;
            SecondaryStats secStats = player.SecondaryStats;
            if(secStats.guildId != 0)
                return;

            if (player.PlayerSynStats.Level < GuildRepo.CreateGuildMinLevel)
            {
                peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.InsufficientPlayerLevel, peer);
                return;
            }
            if (DateTime.Now.Ticks < player.SecondaryStats.guildLeaveGuildCDEnd) // check for cooldown
            {
                peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.LeaveGuildCooldown, peer);
                return;
            }
            if (GuildList.ContainsKey(guildId))
            {
                GuildStatsServer guildStats = GuildList[guildId];
                if (guildStats.faction != peer.mPlayer.PlayerSynStats.faction)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.IncompatibleFaction, peer);
                    return;
                }
                GuildRequestSetting requestSetting = guildStats.mRequestSetting;
                //if (player.LocalCombatStats.CombatScore < requestSetting.CombatScore)
                //{
                //    peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.CombatScoreTooLow, peer);
                //    return;
                //}
                if (player.PlayerSynStats.Level < requestSetting.ProgressLvl)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.InsufficientPlayerLevel, peer);
                    return;
                }
                //if (player.PlayerSynStats.vipLvl < requestSetting.VipLvl)
                //{
                //    peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.VIPLevelTooLow, peer);
                //    return;
                //}

                if (requestSetting.AutoAccept && !guildStats.IsMemberFull())
                {
                    var result = JoinGuild(guildId, peer);            
                }
                else // Member request to join guild
                {
                    Dictionary<string, GuildMemberStatsRequest> requestMembers = guildStats.GetMemberRequestsDict();
                    if (requestMembers.ContainsKey(playerName)) // check whether have already applied
                    {
                        peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.MemberRequestExist, peer);
                        return;
                    }
                    if (requestMembers.Count >= GuildRepo.MAX_REQUEST) // check whether guild's request list is max
                    {
                        peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.MemberRequestLimit, peer);
                        return;
                    }
                    PlayerSynStats pSynStats = player.PlayerSynStats;
                    DateTime timeRequested = DateTime.Now;
                    int slotidx = guildStats.Request_GetEmptySlot();
                    AddMemberRequestToGuildStats(guildStats, new GuildMemberStatsRequest(playerName, player.PlayerSynStats.PortraitID, 0, pSynStats.Level, 0, timeRequested, slotidx));
                    guildStats.mGuildDataDirty = true;
                    guildStats.saveToDB = true;                    
                    if (RemoveOldRequest(playerName))
                        peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.MemberRequestSuccessRemoveOld, peer);
                    else
                        peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.MemberRequestSuccess, peer);
                }
            }
            else  // guild to join no longer exist
                peer.ZRPC.CombatRPC.Ret_GuildJoin((byte)GuildReturnCode.Failed, peer);
        }

        public static void CheckMemberRequest(string charName, bool isAccept, Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return;
            if (secStats.guildRank == (byte)GuildRankType.Member)
                return;
            if(isAccept)
            {
                GameClientPeer peer = GameApplication.Instance.GetCharPeer(charName);
                var task = (peer != null) ? JoinGuild(guildId, peer) : JoinGuild(guildId, null, charName);
            }
            else
            {
                GuildStatsServer guildStats = GuildList[guildId];
                RemoveMemberRequest(guildStats, charName);
                RemoveRequestRecord(charName, guildId);
            }
        }

        public static void CheckMemberRequestAll(bool isAccept, Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return;
            if (secStats.guildRank == (byte)GuildRankType.Member)
                return;
            GuildStatsServer guildStats = GuildList[guildId];
            if (isAccept)
            {
                Dictionary<string, GuildMemberStatsRequest> mRequestsDict = guildStats.GetMemberRequestsDict();
                List<string> charNames = mRequestsDict.OrderBy(kvp => kvp.Value.requestDT).Select(kvp => kvp.Value.name).ToList();
                for (int index = 0; index < charNames.Count; index++)
                {
                    if (guildStats.IsMemberFull())
                        break;
                    string charName = charNames[index];
                    GameClientPeer peer = GameApplication.Instance.GetCharPeer(charName);
                    var task = (peer != null) ? JoinGuild(guildId, peer) : JoinGuild(guildId, null, charName);
                }
            }
            else
                RemoveMemberRequestAll(guildStats);
        }

        #region request record
        private static void RemoveRequestRecord(string charName, int guildId)
        {
            if (GuildMemberRequestMap.ContainsKey(charName))
            {
                for (int index = 0; index < GuildMemberRequestMap[charName].Count; index++)
                {
                    if (GuildMemberRequestMap[charName][index].guildId == guildId)
                    {
                        GuildMemberRequestMap[charName].RemoveAt(index);
                        if (GuildMemberRequestMap[charName].Count == 0)
                            GuildMemberRequestMap.Remove(charName);
                        break;
                    }
                }
            }
        }

        private static bool RemoveOldRequest(string charName)
        {
            int exceedCount = GuildMemberRequestMap[charName].Count - GuildRepo.MAX_APPLY;
            if (exceedCount > 0)
            {
                for (int index = 0; index < exceedCount; index++)
                {
                    GuildMemberRequestRecord record = GuildMemberRequestMap[charName][0];
                    int prev_request_guildId = record.guildId;
                    if (GuildList.ContainsKey(prev_request_guildId))
                        RemoveMemberRequest(GuildList[prev_request_guildId], charName);
                    GuildMemberRequestMap[charName].RemoveAt(0);
                }
                return true;
            }
            return false;
        }

        private static void RemoveAllRequest(string charName)
        {
            if (GuildMemberRequestMap.ContainsKey(charName))
            {
                for (int index = 0; index < GuildMemberRequestMap[charName].Count; index++)
                {
                    GuildMemberRequestRecord record = GuildMemberRequestMap[charName][index];
                    if (GuildList.ContainsKey(record.guildId))
                        RemoveMemberRequest(GuildList[record.guildId], charName);
                }
                GuildMemberRequestMap.Remove(charName);
            }
        }
        #endregion

        public static void RemoveMemberRequest(GuildStatsServer guildStats, string charName)
        {
            Dictionary<string, GuildMemberStatsRequest> mRequestsDict = guildStats.GetMemberRequestsDict();
            if(mRequestsDict.ContainsKey(charName))
            {
                GuildMemberStatsRequest memberStatsRequest = mRequestsDict[charName];  
                CollectionHandler<object> requestList = guildStats.memberRequests;
                int memberIdx = memberStatsRequest.localObjIdx;
                requestList[memberIdx] = null;
                mRequestsDict.Remove(charName);
                guildStats.mGuildDataDirty = true;
                guildStats.saveToDB = true;
            }
        }

        public static void RemoveMemberRequestAll(GuildStatsServer guildStats)
        {
            Dictionary<string, GuildMemberStatsRequest> mRequestsDict = guildStats.GetMemberRequestsDict();
            if (mRequestsDict.Count > 0)
            {
                int guildId = guildStats.guildId;
                CollectionHandler<object> requestList = guildStats.memberRequests;
                foreach (var memberStatsRequest in mRequestsDict.Values)
                {
                    requestList[memberStatsRequest.localObjIdx] = null;
                    RemoveRequestRecord(memberStatsRequest.name, guildId);
                }
                mRequestsDict.Clear();
                guildStats.mGuildDataDirty = true;
                guildStats.saveToDB = true;
            }
        }

        public static async Task<bool> JoinGuild(int id, GameClientPeer peer, string charName = "")
        {
            Player player = null;
            SecondaryStats secStats = null;
            PlayerSynStats pSynStats = null;
            bool isOnline = false;
            if(string.IsNullOrEmpty(charName)) // Means is online
            {
                player = peer.mPlayer;
                secStats = player.SecondaryStats;
                if(secStats.guildId != 0)
                    return false;
                pSynStats = player.PlayerSynStats;
                isOnline = true;
                charName = peer.mChar;
            }

            GuildStatsServer guildStats = GuildList[id];
            if(guildStats != null)
            {
                Dictionary<string, GuildMemberStats> mStatsDict = guildStats.GetMemberStatsDict();
                if (mStatsDict.ContainsKey(charName))
                    return false;
                if(guildStats.IsMemberFull())
                    return false;
                if(isOnline)
                {                              
                    int slotidx = guildStats.Member_GetEmptySlot();
                    AddMemberToGuildStats(guildStats, 
                        new GuildMemberStats(secStats.guildRank, peer.mChar, 0, 0, 0, true, slotidx), 
                        true);
                    player.OnJoinGuild(guildStats);
                    RemoveAllRequest(charName);
                }
                else
                {
                    byte rank = (byte)GuildRankType.Member;
                    int combatscore = await GameApplication.dbRepository.Character.JoinGuild(charName, id, rank);
                    executionFiber.Enqueue(() =>
                    {
                        if (combatscore != -1) //join guild success
                        {
                            int slotidx = guildStats.Member_GetEmptySlot();
                            AddMemberToGuildStats(GuildList[id], new GuildMemberStats(rank, charName, 0, 0, combatscore, false, slotidx), true);
                            RemoveAllRequest(charName);
                        }
                    });
                }
                CreateDonateCDTime(guildStats, charName);
            }      
            return true;
        }

        public static bool LeaveGuild(string playerName, Player senderPlayer)
        {
            GameClientPeer senderPeer = senderPlayer.Slot;
            bool iskicked = senderPlayer.Name != playerName;
            bool isSuccess = false, isRemoveFrmDB = false, isRemoveGuild = false;
            int guildId = senderPlayer.SecondaryStats.guildId;
            if (!GuildList.ContainsKey(guildId))
            {
                senderPeer.ZRPC.CombatRPC.Ret_GuildLeave((byte)GuildReturnCode.GuildNotFound, senderPeer);
                return false;
            }
            byte senderRank = senderPlayer.SecondaryStats.guildRank;
            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> MemberStatsDict = guildStats.GetMemberStatsDict();
            if(guildStats != null && MemberStatsDict.ContainsKey(playerName))
            {
                GuildMemberStats memberStats = MemberStatsDict[playerName];
                if (iskicked)
                {
                    if (senderRank == (byte)GuildRankType.Member || (senderRank == (byte)GuildRankType.Officer && memberStats.rank != (byte)GuildRankType.Member))
                    {
                        senderPeer.ZRPC.CombatRPC.Ret_GuildLeave((byte)GuildReturnCode.KickFailRankTooLow, senderPeer);
                        return false;
                    }
                }
                else if (MemberStatsDict.Count > 1 && senderRank == (byte)GuildRankType.Leader)
                {
                    senderPeer.ZRPC.CombatRPC.Ret_GuildLeave((byte)GuildReturnCode.LeaderLeaveGuildFailed, senderPeer);
                    return false;
                }
                if (MemberStatsDict.Count == 1) // If player is last person
                    isRemoveGuild = true;

                RemoveMemberFromGuildStats(guildStats, memberStats, iskicked);
                GameClientPeer targetPeer = null;
                if (iskicked)
                {
                    targetPeer = GameApplication.Instance.GetCharPeer(playerName);
                    if (targetPeer != null) // Player is Online
                    {
                        Player player = targetPeer.mPlayer;
                        if (player != null && !player.Destroyed)
                        {
                            player.OnLeaveGuild(guildStats, true);
                            GameRules.SendMail(playerName, "Guild_Kicked");
                            isSuccess = true;
                        }
                    }
                    else // Player is Offline
                        isRemoveFrmDB = true;
                }
                else
                {
                    targetPeer = senderPeer;
                    senderPlayer.OnLeaveGuild(guildStats, false);
                    isSuccess = true;
                }

                //log
                string message = string.Format("id:{0}|executor:{1}|target:{2}|kick:{3}", guildId, senderPeer.mChar, playerName, iskicked);
                Zealot.Logging.Client.LogClasses.GuildLeave guildLeaveLog = new Zealot.Logging.Client.LogClasses.GuildLeave();
                if (targetPeer != null)
                {
                    guildLeaveLog.userId = targetPeer.mUserId;
                    guildLeaveLog.charId = targetPeer.GetCharId();
                }
                guildLeaveLog.message = message;
                guildLeaveLog.guildid = guildId;
                guildLeaveLog.executor = senderPeer.mChar;
                guildLeaveLog.target = playerName;
                guildLeaveLog.kick = iskicked;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildLeaveLog);
            }
            if(isRemoveFrmDB)
            {
                DateTime leaveGuildCDEnd = DateTime.Now.AddSeconds(GuildRepo.GetValue("LeaveGuildCooldownTime"));
                var result = GameApplication.dbRepository.Character.QuitGuild(playerName, leaveGuildCDEnd);
                GameRules.SendMail(playerName, "Guild_Kicked");
                isSuccess = true;
            }
            if(isSuccess)
            {
                if(isRemoveGuild)
                {
                    var result = DeleteGuild(guildId);
                }
                DonateRules.LeaveGuildSendDonateRewardMail(playerName, guildId); //Guild Donate  , Send did not take away donate
                RemoveDonateCDTime(guildStats, playerName);
                return true;
            }
            return false;
        }

        public static int KickFromGuildByGM(string playerName)
        {
            bool isSuccess = false, isRemoveFrmDB = false, isRemoveGuild = false;
            int guildId = GetGuildIdByPlayer(playerName);
            if (!GuildList.ContainsKey(guildId))
                return -1;
            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> MemberStatsDict = guildStats.GetMemberStatsDict();
            if (MemberStatsDict.ContainsKey(playerName))
            {
                GuildMemberStats memberStats = MemberStatsDict[playerName];
                if (memberStats.rank == (byte)GuildRankType.Leader && MemberStatsDict.Count > 1)
                    return -2;

                if (MemberStatsDict.Count == 1) // If player is last person
                    isRemoveGuild = true;

                RemoveMemberFromGuildStats(guildStats, memberStats, true);
                GameClientPeer targetPeer = GameApplication.Instance.GetCharPeer(playerName);
                if (targetPeer != null) // Player is Online
                {
                    Player player = targetPeer.mPlayer;
                    if (player != null && !player.Destroyed)
                    {
                        player.OnLeaveGuild(guildStats, true);
                        GameRules.SendMail(playerName, "Guild_Kicked");
                        isSuccess = true;
                    }
                }
                else // Player is Offline
                    isRemoveFrmDB = true;

                //log
                string message = string.Format("id:{0}|executor:{1}|target:{2}|kick:{3}", guildId, "GM", playerName, true);
                Zealot.Logging.Client.LogClasses.GuildLeave guildLeaveLog = new Zealot.Logging.Client.LogClasses.GuildLeave();
                if (targetPeer != null)
                {
                    guildLeaveLog.userId = targetPeer.mUserId;
                    guildLeaveLog.charId = targetPeer.GetCharId();
                }
                guildLeaveLog.message = message;
                guildLeaveLog.guildid = guildId;
                guildLeaveLog.executor = "GM";
                guildLeaveLog.target = playerName;
                guildLeaveLog.kick = true;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildLeaveLog);
            }
            else
                return -3;
            if (isRemoveFrmDB)
            {
                DateTime leaveGuildCDEnd = DateTime.Now.AddSeconds(GuildRepo.GetValue("LeaveGuildCooldownTime"));
                var result = GameApplication.dbRepository.Character.QuitGuild(playerName, leaveGuildCDEnd);
                GameRules.SendMail(playerName, "Guild_Kicked");
                isSuccess = true;
            }
            if (isSuccess)
            {
                if (isRemoveGuild)
                {
                    var result = DeleteGuild(guildId);
                }
                DonateRules.LeaveGuildSendDonateRewardMail(playerName, guildId); //Guild Donate  , Send did not take away donate
                RemoveDonateCDTime(guildStats, playerName);
                return 1;
            }
            return -4;
        }

        public static void SetRequestSetting(int combatscore, int level, byte viplevel, bool autoAccept, Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return;
            if (secStats.guildRank == (byte)GuildRankType.Member)
                return;
            GuildStatsServer guildStats = GuildList[guildId];
            GuildRequestSetting requestSetting = guildStats.mRequestSetting;
            if (requestSetting.CombatScore == combatscore && requestSetting.ProgressLvl == level &&
                requestSetting.VipLvl == viplevel && requestSetting.AutoAccept == autoAccept)
                return; //not changes
            requestSetting.CombatScore = combatscore;
            requestSetting.ProgressLvl = level;
            requestSetting.VipLvl = viplevel;
            requestSetting.AutoAccept = autoAccept;
            guildStats.requestSetting = requestSetting.ToString();
            guildStats.mGuildDataDirty = true;
            guildStats.saveToDB = true;
        }

        public static void SetGuildIcon(int icon, bool free, Player player)
        {
            if (!GuildRepo.IsIconValid(icon))
                return;
            int guildId = player.SecondaryStats.guildId;
            if (!IsGuildLeader(player))
                return;

            bool bLog = false;
            GuildStatsServer guildStats = GuildList[guildId];
            if (guildStats.guildIconFree)
            {
                guildStats.guildIcon = icon;
                guildStats.guildIconFree = false;
                guildStats.saveToDB = true;
                guildStats.mGuildDataDirty = true;
                bLog = true;
            }
            else
            {
                if (!free && player.DeductGold(GuildRepo.GetValue("ChangeGuildIconCost"), true, true, "Guild_Icon"))
                {
                    guildStats.guildIcon = icon;
                    guildStats.saveToDB = true;
                    guildStats.mGuildDataDirty = true;
                    bLog = true;
                }
                else
                    player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("sys_InsufficientGold"), "", false, player.Slot);
            }
            if (bLog)
            {
                //log
                string message = string.Format("id:{0}", guildId);
                Zealot.Logging.Client.LogClasses.GuildIcon guildIconLog = new Zealot.Logging.Client.LogClasses.GuildIcon();
                GameClientPeer peer = player.Slot;
                guildIconLog.userId = peer.mUserId;
                guildIconLog.charId = peer.GetCharId();
                guildIconLog.message = message;
                guildIconLog.guildid = guildId;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildIconLog);
            }
        }

        public static void SetGuildNotice(string noticeStr, Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return;         
            if (secStats.guildRank != (byte)GuildRankType.Leader && secStats.guildRank != (byte)GuildRankType.Officer)
                return; 
            GuildList[guildId].guildNotice = noticeStr;
            GuildList[guildId].saveToDB = true;
        }

        public static void AddGuildGold(int guildId, int amt, string charName)
        {
            if(GuildList.ContainsKey(guildId) && amt > 0)
            {
                GuildStatsServer guildstats = GuildList[guildId];
                Dictionary<string, GuildMemberStats> memberStatsDict = guildstats.GetMemberStatsDict();
                if (memberStatsDict.ContainsKey(charName))
                {
                    guildstats.guildGold += amt;
                    guildstats.saveToDB = true;

                    GuildMemberStats gMemberStats = memberStatsDict[charName];
                    gMemberStats.fundToday += amt;
                    gMemberStats.fundTotal += amt;
                    guildstats.members[gMemberStats.localObjIdx] = gMemberStats.ToString();
                }
            }
        }

        public static float GetGuildTechStats(Player player, GuildTechType type)
        {
            GameClientPeer peer = player.Slot;
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (GuildList.ContainsKey(guildId))
                return GuildList[guildId].GetGuildTechStats(type);
            return 0;
        }

        public static void GuildLvlUpTech(GuildTechType type, Player player)
        {
            GameClientPeer peer = player.Slot;
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
            {
                peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.GuildNotFound, "", peer);
                return;
            }
            if (secStats.guildRank != (byte)GuildRankType.Leader)
            {
                peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.GuildLeaderOnly, "", peer);
                return;
            }
            GuildStatsServer guildStats = GuildList[guildId];
            int guildLevel = guildStats.guildLevel;
            Dictionary<GuildTechType, int> guildTechDict = guildStats.mGuildTechDict;
            int currentLvl = 0;
            if (type == GuildTechType.Level)
                currentLvl = guildLevel;
            else if (guildTechDict.ContainsKey(type))
                currentLvl = guildTechDict[type];
            int nextLvl = currentLvl + 1;
            GuildTechLevelJson guildTechLevelJson = GuildRepo.GetGuildTechByTypeAndLevel(type, nextLvl);
            if (guildTechLevelJson == null)
            {
                peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.TechAlreadyMax, "", peer);
                return;
            }
            else
            {
                if (guildLevel < guildTechLevelJson.requirelv)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.GuildLevelTooLow, "", peer);
                    return;
                }
                if (guildStats.guildGold < guildTechLevelJson.fund)
                {
                    peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.InsufficientGold, "", peer);
                    return;
                }
                guildStats.guildGold -= guildTechLevelJson.fund;
                if (type == GuildTechType.Level)
                    guildStats.guildLevel++;
                else
                {
                    guildTechDict[type] = nextLvl;
                    guildStats.UpdateTechString();
                }

                guildStats.AddHistory(GuildHistoryType.TechLevelUp, string.Format("{0}|{1}", (byte)type, nextLvl));
                Dictionary<string, string> _paramters = new Dictionary<string, string>();
                _paramters.Add("type", GuildRepo.GetGuildTechClassByType(type).localizedname);
                _paramters.Add("level", nextLvl.ToString());
                SendGuildMessage(guildStats.guildId, GUILocalizationRepo.GetLocalizedString("guild_History_TechLevelUp", _paramters));

                if (type == GuildTechType.Health || type == GuildTechType.Attack || type == GuildTechType.Armor 
                    || type == GuildTechType.Accuracy || type == GuildTechType.Evasion || type == GuildTechType.Critical
                    || type == GuildTechType.CoCritical || type == GuildTechType.CriticalDamage || type == GuildTechType.CoCriticalDamage
                    || type == GuildTechType.Shop)
                {
                    string peerName = player.Name;
                    Dictionary<string, GuildMemberStats> mStatsDict = guildStats.GetMemberStatsDict();
                    foreach (KeyValuePair<string, GuildMemberStats> kvp in mStatsDict)
                    {
                        if (kvp.Key != peerName)
                        {
                            GameClientPeer currPeer = GameApplication.Instance.GetCharPeer(kvp.Key);
                            if (currPeer != null && currPeer.mPlayer != null)
                                currPeer.mPlayer.OnGuildTechLevelUp(guildTechLevelJson);
                        }
                    }
                    peer.mPlayer.OnGuildTechLevelUp(guildTechLevelJson);
                }
                else if (type == GuildTechType.Love)
                {
                    GuildSMBossJson guildSMBossJson = GuildRepo.GetGuildSMBossByLvl(guildStats.SMBossLevel);
                    if (guildSMBossJson != null)
                        guildStats.UpdateSMBossLevel(guildStats.SMBossDmgDone, guildSMBossJson.healthmax);
                }

                //log
                string message = string.Format("id:{0}|type:{1}|cost:{2}|now:{3}", guildId, (byte)type, guildTechLevelJson.fund, guildStats.guildGold);
                Zealot.Logging.Client.LogClasses.GuildTech guildTechLog = new Zealot.Logging.Client.LogClasses.GuildTech();
                guildTechLog.userId = peer.mUserId;
                guildTechLog.charId = peer.GetCharId();
                guildTechLog.message = message;
                guildTechLog.guildid = guildId;
                guildTechLog.type = (byte)type;
                guildTechLog.cost = guildTechLevelJson.fund;
                guildTechLog.fundnow = guildStats.guildGold;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(guildTechLog);
            }
            player.UpdateGuildQuestDailyTimes();
        }

        public static string GetGuildHistory(Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (GuildList.ContainsKey(guildId))
                return GuildList[guildId].mGuildHistory.historyStr;
            return "";
        }

        public static bool DecreaseMaxBuyCount(GameClientPeer peer, byte shopId, int itemId)
        {/*
            // Check buy count
            string buyCntStr = peer.mPlayer.SecondaryStats.guildShopBuyCount;
            string[] splitByShop = buyCntStr.Split('|');
            int splitByShopLen = splitByShop.Length;
            if(splitByShopLen == GuildRepo.mGuildShopGrpIdMap.Count)
            {
                string[] splitByItem = splitByShop[shopId-1].Split('`');
                int splitByItemLen = splitByItem.Length;
                if(splitByItemLen > 0)
                {
                    int count = Int32.Parse(splitByItem[itemId-1]);
                    if (count > 0)
                    {
                        --count;
                        StringBuilder sb = new StringBuilder();
                        // Merge item list
                        for(int i=0; i<splitByItemLen; ++i)
                        {
                            if (i == (itemId-1))
                                sb.Append(count);
                            else
                                sb.Append(splitByItem[i]);
                            if(i < splitByItemLen-1)
                                sb.Append('`');
                        }
                        // Merge shop list
                        StringBuilder sb2 = new StringBuilder();
                        for(int i=0; i<splitByShopLen; ++i)
                        {
                            if (i == (shopId-1))
                                sb2.Append(sb.ToString());
                            else
                                sb2.Append(splitByShop[i]);
                            if(i < splitByShopLen-1)
                                sb2.Append('|');
                        }
                        peer.mPlayer.SecondaryStats.guildShopBuyCount = sb2.ToString();
                        return true;
                    }
                }
            }*/
            return false;
        }

        public static void GuildShopExchange(byte shopId, int itemId, GameClientPeer peer)
        {/*
            if(peer != null)
            {
                List<GuildShopJson> gShopJsonList = GuildRepo.GetGuildShopItemsByShopId(shopId);
                if(gShopJsonList != null)
                {
                    if(itemId > 0 && itemId <= gShopJsonList.Count)
                    {
                        GuildShopJson gShopJson = gShopJsonList[itemId-1];
                        Player player = peer.mPlayer;
                        SecondaryStats secStats = player.SecondaryStats;
                        int guildId = secStats.guildId;
                        if(secStats.guildpoints >= gShopJson.cost)
                        {
                            int buildId = gShopJson.buildinggrpid;
                            int bLvl = GetBuildingLvlByGuildId(guildId, buildId);
                            if(gShopJson.reqbuildlvl <= bLvl) // If have reached required level
                            {
                                if(DecreaseMaxBuyCount(peer, shopId, itemId))
                                {
                                    if(gShopJson.itemid != 0) // Is a item
                                    {
                                        InvRetval retVal = peer.mInventory.AddItemsIntoInventory((ushort)gShopJson.itemid, 1);
                                        if(retVal.retcode == InvReturnCode.AddSuccess)
                                            secStats.guildpoints -= gShopJson.cost;
                                    }
                                    else // Is a side effect
                                    {
                                        //Dictionary<int, SideEffectJson> seJsonList = GuildRepo.GetGuildBuildingSEffectById(gShopJson.buildinggrpid);
                                        //SideEffectJson seJson = seJsonList[bLvl];
                                        //SideEffect se = SideEffectFactory.CreateSideEffect(seJsonList[bLvl]);
                                        //if(seJson.effecttype != EffectType.Damage_SpSkillDamage)
                                        {
                                            //se.Apply(player, player, true);
                                            //secStats.guildpoints -= gShopJson.cost;
                                        }
                                    }
                                }
                                else
                                    peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.NotEnoughShopItems, "", peer);
                            }
                        }
                        else
                            peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.NotEnoughPoints, "", peer);
                    }
                }
            }*/
        }

        public static void OpenSecretShop(GameClientPeer peer)
        {
            if(peer != null)
            {
                SecondaryStats secStats = peer.mPlayer.SecondaryStats;
                if(secStats.guildRank != (byte)GuildRankType.Leader)
                {
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_SecretShopLeaderOnly", "", true, peer);
                    return;
                }
                int guildId = secStats.guildId;
                bool hasOpened = false;
                GuildStatsServer guildStats = GuildList[guildId];
                if(guildStats == null || (guildStats != null && guildStats.guildSecretShopTick > 0)) 
                    hasOpened = true;
                
                if(!hasOpened) // Can open secret shop
                {
                    // Need to pay guild gold
                    if(guildStats.guildGold >= 100000)
                    {
                        guildStats.guildGold -= 100000;
                        DateTime dtOpenUntil = DateTime.Now.AddDays(1);
                        guildStats.guildSecretShopTick = dtOpenUntil.Ticks;
                        guildStats.saveToDB = true;

                        ChatMessage newMsg = new ChatMessage(MessageType.Guild, GUILocalizationRepo.GetLocalizedSysMsgByName("ret_Guild_SecretShopSuccess"));
                        GameApplication.Instance.BroadcastChatMessage_Guild(newMsg, guildId);
                    }
                    else
                        peer.ZRPC.CombatRPC.Ret_GuildResult((byte)GuildReturnCode.InsufficientGold, "", peer);
                }
            }
        }

        public static bool IsGuildLeader(Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return false; //guild not exist
            if (secStats.guildRank != (byte)GuildRankType.Leader)
                return false; //only guild leader can change rank.
            return true;
        }

        public static void AppointGuildRank(string appointName, byte appointRank, Player player) 
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return; // Guild not exist
            string leaderName = player.Name;
            byte leaderRank = (byte)GuildRankType.Leader;
            if (secStats.guildRank != leaderRank)
                return; //only guild leader can change rank.
            if (appointName == leaderName)
                return; //appoint myself not allowed

            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> memberStatDict = guildStats.GetMemberStatsDict();
            if (!memberStatDict.ContainsKey(appointName))
                return; //target not in this guild
            if (!memberStatDict.ContainsKey(leaderName))
                return; //myself not in this guild
            GuildMemberStats appointStats = memberStatDict[appointName];
            if (appointStats.rank == appointRank)
                return; //same rank, no modification;

            bool changeLeader = appointRank == leaderRank;
            if (!changeLeader)
            {
                guildStats.AddHistory(GuildHistoryType.MemberRank, string.Format("{0}|{1}|{2}", appointName, appointStats.rank, appointRank));
                Dictionary<string, string> _paramters = new Dictionary<string, string>();
                _paramters.Add("name", appointName);
                _paramters.Add("from", GUILocalizationRepo.GetLocalizedString("guild_Rank_" + ((GuildRankType)appointStats.rank).ToString()));
                _paramters.Add("to", GUILocalizationRepo.GetLocalizedString("guild_Rank_" + ((GuildRankType)appointRank).ToString()));
                SendGuildMessage(guildStats.guildId, GUILocalizationRepo.GetLocalizedString("guild_History_MemberRank", _paramters));
            }

            GameClientPeer appoint_peer = GameApplication.Instance.GetCharPeer(appointName);
            Player appoint_player = appoint_peer != null ? appoint_peer.mPlayer : null;           
            appointStats.rank = appointRank;
            guildStats.members[appointStats.localObjIdx] = appointStats.ToString();

            if (changeLeader)
            {              
                GuildMemberStats myStats = memberStatDict[leaderName];
                myStats.rank = (byte)GuildRankType.Member;
                guildStats.members[myStats.localObjIdx] = myStats.ToString();
                secStats.guildRank = myStats.rank;
                var old_leader_result = GameApplication.dbRepository.Character.UpdateGuildInfo(leaderName, guildId, myStats.rank);
                guildStats.AddHistory(GuildHistoryType.ChangeLeader, string.Format("{0}|{1}", leaderName, appointName));
                Dictionary<string, string> _paramters = new Dictionary<string, string>();
                _paramters.Add("leader", leaderName);
                _paramters.Add("newleader", appointName);
                SendGuildMessage(guildStats.guildId, GUILocalizationRepo.GetLocalizedString("guild_History_ChangeLeader", _paramters));
            }

            if (appoint_player != null)
            {
                appoint_player.SecondaryStats.guildRank = appointRank;
                if (changeLeader)
                    appoint_peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Guild_NewLeader"), "", true, appoint_peer);
            }
            var result = GameApplication.dbRepository.Character.UpdateGuildInfo(appointName, guildId, appointRank);
            Dictionary<string, string> mail_parameters = new Dictionary<string, string>();
            mail_parameters.Add("rank", GUILocalizationRepo.GetLocalizedString("guild_Rank_" + ((GuildRankType)appointRank).ToString()));
            GameRules.SendMail(appointName, "Guild_RankChanged", mail_parameters);
        }

        public static int GMChangeLeader(int guildId, string appointName)
        {
            if (!GuildList.ContainsKey(guildId))
                return 0; // Guild not exist

            byte appointRank = (byte)GuildRankType.Leader;
            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, GuildMemberStats> memberStatDict = guildStats.GetMemberStatsDict();
            if (!memberStatDict.ContainsKey(appointName))
                return 1; //target not in this guild
            GuildMemberStats appointStats = memberStatDict[appointName];
            if (appointStats.rank == appointRank)
                return 2; //same rank, no modification;

            Dictionary<string, string> mail_parameters = new Dictionary<string, string>();
            foreach (GuildMemberStats member in memberStatDict.Values)
            {
                if (member.rank == appointRank)
                {
                    member.rank = (byte)GuildRankType.Member;
                    guildStats.members[member.localObjIdx] = member.ToString();
                    string old_leader = member.name;
                    GameClientPeer old_leader_peer = GameApplication.Instance.GetCharPeer(old_leader);
                    Player old_leader_player = old_leader_peer != null ? old_leader_peer.mPlayer : null;
                    if (old_leader_player != null)
                        old_leader_player.SecondaryStats.guildRank = member.rank;
                    var old_leader_result = GameApplication.dbRepository.Character.UpdateGuildInfo(old_leader, guildId, member.rank);                  
                    mail_parameters.Add("rank", GUILocalizationRepo.GetLocalizedString("guild_Rank_Member"));
                    GameRules.SendMail(old_leader, "Guild_RankChanged", mail_parameters);
                    break;
                }
            }

            GameClientPeer appoint_peer = GameApplication.Instance.GetCharPeer(appointName);
            Player appoint_player = appoint_peer != null ? appoint_peer.mPlayer : null;
            appointStats.rank = appointRank;
            guildStats.members[appointStats.localObjIdx] = appointStats.ToString();

            if (appoint_player != null)
            {
                appoint_player.SecondaryStats.guildRank = appointRank;
                appoint_peer.ZRPC.CombatRPC.Ret_SendSystemMessageId(GUILocalizationRepo.GetSysMsgIdByName("ret_Guild_NewLeader"), "", true, appoint_peer);
            }
            var result = GameApplication.dbRepository.Character.UpdateGuildInfo(appointName, guildId, appointRank);
            mail_parameters.Clear();
            mail_parameters.Add("rank", GUILocalizationRepo.GetLocalizedString("guild_Rank_" + ((GuildRankType)appointRank).ToString()));
            GameRules.SendMail(appointName, "Guild_RankChanged", mail_parameters);
            return 100;
        }

        public static void GuildInviteWorld(Player player)
        {
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return;
            if (secStats.guildRank == (byte)GuildRankType.Member)
                return;
            GuildStatsServer guildStats = GuildList[guildId];
            if (guildStats.IsMemberFull())
            {               
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Guild_MemberOption_MemberFull", "", false, player.Slot);
                return;
            }
            if (guildStats.UpdateWorldInvitation(DateTime.Now)) //can send                                
            {
                GuildRequestSetting requestSetting = guildStats.mRequestSetting;
                string link = string.Format("|guild|{0};{1};{2};{3};{4}", guildStats.guildId, guildStats.faction, requestSetting.CombatScore, requestSetting.ProgressLvl, 
                    requestSetting.VipLvl);
                Dictionary<string, string> paramters = new Dictionary<string, string>();
                paramters.Add("guild", guildStats.name);
                PlayerSynStats playerSynStats = player.PlayerSynStats;
                string message = GameUtils.GetHyperTextTag(link, GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Guild_Recruit", paramters));
                ChatMessage newMsg = new ChatMessage(MessageType.World, message, player.Name, "", playerSynStats.PortraitID, 
                                                     playerSynStats.jobsect, 0, playerSynStats.faction);
                GameApplication.Instance.BroadcastChatMessage(newMsg);
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Guild_RecuritSent", "", false, player.Slot);
            }
            else
                player.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_Guild_RecuritCD", "", false, player.Slot);
        }

        public static void GuildDreamHouseAdd(DreamhouseType type, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return; // Guild not exist
      
            int dreamHouseStatus = secStats.GuildDreamHouseUsed;
            if (type > DreamhouseType.QiZhenYiBao || GameUtils.IsBitSet(dreamHouseStatus, (int)type))
            {
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_DreamhouseAlreadyUsed", "", false, player.Slot);
                return;
            }

            int ptsToAdd = 0;
            GuildStatsServer guildStats = GuildList[guildId];
            Dictionary<string, string> _paramters = new Dictionary<string, string>();
            _paramters.Add("name", player.Name);
            switch (type)
            {
                case DreamhouseType.YiYeEnZe:
                    if (!player.DeductMoney(GuildRepo.GetValue("DreamHouse_CostYiYeEnZe"), "YiYeEnZe"))
                    {
                        //peer.ZRPC.CombatRPC.OpenUIWindow((byte)LinkUIType.GoAlchemy, -1, peer);
                        return;
                    }
                    ptsToAdd = GuildRepo.GetValue("DreamHouse_PtsYiYeEnZe");
                    secStats.GuildDreamHouseUsed = (byte)GameUtils.SetBit(dreamHouseStatus, (int)type);
                    GameRules.GiveReward_Bag(player, new List<int>() { GuildRepo.mGuildDreamhouseUsedRewards[(int)type] }, true, true, "YiYeEnZe");
                    guildStats.AddHistory(GuildHistoryType.CaveMoney, player.Name);
                    player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildYoumeng);
                    SendGuildMessage(guildId, GUILocalizationRepo.GetLocalizedString("guild_History_CaveMoney", _paramters));
                    break;
                case DreamhouseType.HuaYanQiaoYu:
                    if (!player.DeductGold(GuildRepo.GetValue("DreamHouse_CostHuaYanQiaoYu"), true, true, "HuaYanQiaoYu"))
                    {
                        peer.ZRPC.CombatRPC.OpenUIWindow((byte)LinkUIType.GoTopUp, -1, peer);
                        return;
                    }
                    ptsToAdd = GuildRepo.GetValue("DreamHouse_PtsHuaYanQiaoYu");
                    secStats.GuildDreamHouseUsed = (byte)GameUtils.SetBit(dreamHouseStatus, (int)type);
                    GameRules.GiveReward_Bag(player, new List<int>() { GuildRepo.mGuildDreamhouseUsedRewards[(int)type] }, true, true, "HuaYanQiaoYu");
                    guildStats.AddHistory(GuildHistoryType.CaveGoldSmall, player.Name);
                    player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildYoumeng);
                    SendGuildMessage(guildId, GUILocalizationRepo.GetLocalizedString("guild_History_CaveGoldSmall", _paramters));
                    break;
                case DreamhouseType.QiZhenYiBao:
                    if (!player.DeductGold(GuildRepo.GetValue("DreamHouse_CostQiZhenYiBao"), true, true, "QiZhenYiBao"))
                    {
                        peer.ZRPC.CombatRPC.OpenUIWindow((byte)LinkUIType.GoTopUp, -1, peer);
                        return;
                    }
                    ptsToAdd = GuildRepo.GetValue("DreamHouse_PtsQiZhenYiBao");
                    secStats.GuildDreamHouseUsed = (byte)GameUtils.SetBit(dreamHouseStatus, (int)type);
                    GameRules.GiveReward_Bag(player, new List<int>() { GuildRepo.mGuildDreamhouseUsedRewards[(int)type] }, true, true, "QiZhenYiBao");
                    guildStats.AddHistory(GuildHistoryType.CaveGoldLarge, player.Name);
                    player.Slot.mQuestExtraRewardsCtrler.UpdateTask(QuestExtraType.GuildYoumeng);
                    SendGuildMessage(guildId, GUILocalizationRepo.GetLocalizedString("guild_History_CaveGoldLarge", _paramters));
                    break;
            }
            GMActivityConfigData config = GMActivityConfig.GetConfigInt(GMActivityType.YouMengLou, DateTime.Now);
            if (config != null)
                ptsToAdd = Mathf.FloorToInt(ptsToAdd * config.mDataList[0] / 100);

            int favour_old = guildStats.DreamHouseFavourability;
            int favour_max = GuildRepo.DreamHouseTotalFavourability;
            if (favour_old < favour_max && ptsToAdd > 0)
            {
                int resultPts = favour_old + ptsToAdd;
                guildStats.DreamHouseFavourability = (resultPts > favour_max) ? favour_max : resultPts;
                guildStats.saveToDB = true;

                int increment = guildStats.DreamHouseFavourability - favour_old;
                peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_AddFavourability", string.Format("value;{0}", increment), false, peer);

                // Log
                string message = string.Format("guildId:{0}|dreamhouseType:{1}|points:{2}", guildId, (int)type, ptsToAdd);
                Zealot.Logging.Client.LogClasses.GuildDreamHouseAdd syslog = new Zealot.Logging.Client.LogClasses.GuildDreamHouseAdd();
                syslog.userId = peer.mUserId;
                syslog.charId = peer.GetCharId();
                syslog.message = message;
                syslog.guildId = guildId;
                syslog.dreamhouseType = (int)type;
                syslog.points = ptsToAdd;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(syslog);
            }
        }

        public static void GuildDreamHouseCollect(int milestone, GameClientPeer peer)
        {
            Player player = peer.mPlayer;
            if (player == null || player.Destroyed)
                return;
            SecondaryStats secStats = player.SecondaryStats;
            int guildId = secStats.guildId;
            if (!GuildList.ContainsKey(guildId))
                return; // Guild not exist

            GuildStatsServer guildStats = GuildList[guildId];
            int favourability = guildStats.DreamHouseFavourability;
            if (milestone > favourability)
                return;

            StringBuilder sb = new StringBuilder();            
            int idx = 0;
            bool isFirst = true;
            Dictionary<int, int> guildDreamhouseMap = GuildRepo.mGuildDreamhouseMap;
            bool allCollected = true;
            List<int> rewardListIds = new List<int>();
            foreach (KeyValuePair<int, int> kvp in guildDreamhouseMap)
            {
                bool isCollected = false;
                int currMilestone = kvp.Key;
                if (favourability >= currMilestone)
                {
                    isCollected = TryGetDHMilestoneValue(secStats.GuildDreamHouseCollected, currMilestone);
                    if(currMilestone == milestone)
                    {
                        if (!isCollected) // Send reward
                        {
                            rewardListIds.Add(kvp.Value);
                            isCollected = true;
                            // Log
                            string message = string.Format("milestone:{0}", milestone);
                            Zealot.Logging.Client.LogClasses.GuildDreamHouseCollect syslog = new Zealot.Logging.Client.LogClasses.GuildDreamHouseCollect();
                            syslog.userId = peer.mUserId;
                            syslog.charId = peer.GetCharId();
                            syslog.message = message;
                            syslog.milestone = milestone;
                            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(syslog);
                        }
                    }
                    if (isFirst) isFirst = false;
                    else sb.Append(';');
                    sb.AppendFormat("{0}|{1}", currMilestone, isCollected ? 1 : 0);
                }
                ++idx;
                if (!isCollected)
                    allCollected = false;
            }
            secStats.GuildDreamHouseCollected = sb.ToString();
            if (rewardListIds.Count > 0)
                GameRules.GiveReward_CheckBagSlotThenMail(player, rewardListIds, "Reward_DreamHouse", null, true, true, "DreamHouse");
            //if (allCollected)
                //peer.mPlayer.VIPAchievementStats.UpdateAchievement("dh");
        }

        public static bool TryGetDHMilestoneValue(string str, int milestone)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] collectedArray = str.Split(';');
                if (collectedArray != null)
                {
                    int len = collectedArray.Length;
                    for (int i=0; i<len; ++i)
                    {
                        string[] collectedInfo = collectedArray[i].Split('|');
                        if (collectedInfo != null && collectedInfo.Length == 2)
                        {
                            int mStone = int.Parse(collectedInfo[0]);
                            if(mStone == milestone)
                            {
                                byte collect = byte.Parse(collectedInfo[1]);
                                return collect == 1;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static void SendGuildMessage(int guildId, string message)
        {
            ChatMessage newMsg = new ChatMessage(MessageType.Guild, message);
            GameApplication.Instance.BroadcastChatMessage_Guild(newMsg, guildId);
        }

        public static void ResetAllGuildBossInfo()
        {
            bossPrevResetDT = DateTime.Today; // Store just reset datetime
            foreach(KeyValuePair<int, GuildStatsServer> kvp in GuildList)
            {
                ResetOnNewWeek(kvp.Value);
            }
        }

        public static void ResetOnNewWeek(GuildStatsServer guildStats)
        { 
            guildStats.guildBossLastResetTick = bossPrevResetDT.Ticks; // UpdateTicks
            guildStats.saveToDB = true;
        }

        /*
            Updates needed
            --------------
            1) Update loop to update local object
            2) Scheduled db saving for guild
            Member request - 15mins, all don't use schedule on interval, example use checkgame
        */
        public static void Update()
        {
            foreach(KeyValuePair<int, GuildStatsServer> kvp in GuildList)
            {
                GuildStatsServer guildStats = kvp.Value;
                if (guildStats.IsDirty())
                {                  
                    Dictionary<byte, object> packet = guildStats.Serialize((byte)LOCATEGORY.SharedStats, -1, false);
                    var eventData = GameRules.GetLocalObjEventData(packet);
                    var sendPara = GameRules.GetSendParam(true);
                    Dictionary<string, GuildMemberStats>.KeyCollection memberStatsDictKeys = guildStats.GetMemberStatsDict().Keys;
                    foreach (var membername in memberStatsDictKeys)
                    {
                        GameClientPeer peer = GameApplication.Instance.GetCharPeer(membername);
                        if(peer != null && peer.mPlayer != null && !peer.mPlayer.Destroyed)
                            peer.SendEvent(eventData, sendPara);
                    }
                    guildStats.Reset();
                }

                // Check secret shop expiry
                if (guildStats.guildSecretShopTick > 0 && guildStats.guildSecretShopTick < DateTime.Now.Ticks)
                {
                    guildStats.guildSecretShopTick = 0; // Reset secret shop
                    guildStats.saveToDB = true;
                }
            }
            DonateRules.CheckResetDonateTime();
            executionFiber.Schedule(Update, UpdateInterval);
        }

        public static void OnSaveGuildInterval()
        {
            if(saveDBGuildIdList.Count > 0)
            {
                int guildId = saveDBGuildIdList[currSaveDbIdx];
                currSaveDbIdx = (currSaveDbIdx+1) % saveDBGuildIdList.Count;
                bool noSaveDB = false;
                while (!GuildList[guildId].saveToDB)
                {
                    if (currSaveDbIdx == 0) // Break after roundabout
                    {
                        noSaveDB = true;
                        break;
                    }
                    guildId = saveDBGuildIdList[currSaveDbIdx];
                    currSaveDbIdx = (currSaveDbIdx+1) % saveDBGuildIdList.Count;
                }
                Task task = null;
                if (!noSaveDB)
                    task = SaveGuildStatsToDB(GuildList[guildId]);
            }
            executionFiber.Schedule(OnSaveGuildInterval, saveDBInterval);
        }

        public static void OnStop()
        {
            if (GuildList == null)
                return;

            var tasks = new List<Task>();
            foreach (var kvp in GuildList)
            {
                tasks.Add(SaveGuildStatsToDB(kvp.Value));
            }

            Task t = Task.WhenAll(tasks.ToArray());
            try
            {
                t.Wait();
            }
            catch {}
        }
        
        private static async Task SaveGuildStatsToDB(GuildStatsServer guildStat)
        {
            if (!guildStat.saveToDB)
                return;
            if (guildStat.mGuildDataDirty)
            {
                defaultGuildData.DonateInvData = guildStat.memberDonateInv;
                defaultGuildData.donateRequestCDTime = guildStat.donateRequestCDTime;
                defaultGuildData.GuildIconFree = guildStat.guildIconFree;
                GuildMemberRequestInvData mRequestInvData = defaultGuildData.MemberRequestInvData;
                mRequestInvData.memberRequestList.Clear();
                Dictionary<string, GuildMemberStatsRequest> mRequestDict = guildStat.GetMemberRequestsDict();
                foreach (KeyValuePair<string, GuildMemberStatsRequest> requestKvp in mRequestDict)
                {
                    GuildMemberStatsRequest memberRequest = requestKvp.Value;
                    mRequestInvData.memberRequestList.Add(new MemberRequestElement(memberRequest.name, memberRequest.portrait, memberRequest.viplvl, memberRequest.lvl, memberRequest.combatScore, memberRequest.requestDT));
                }
                mRequestInvData.requestSetting = guildStat.mRequestSetting;
                guildStat.mGuildDataStr = defaultGuildData.SerializeForDB();
                guildStat.mGuildDataDirty = false;
            }
            bool result = await GameApplication.dbRepository.Guild.SaveGuildAsync(guildStat.guildId, guildStat.guildIcon, guildStat.guildLevel,
                                                                                  guildStat.guildGold, guildStat.guildNotice, guildStat.mGuildHistory.historyStr,
                                                                                  guildStat.techs, guildStat.SMBossLevel, guildStat.SMBossDmgDone, 
                                                                                  guildStat.DreamHouseFavourability,
                                                                                  new DateTime(guildStat.guildBossLastResetTick), 
                                                                                  new DateTime(guildStat.guildSecretShopTick),
                                                                                  guildStat.mGuildDataStr);
            executionFiber.Enqueue(() =>
            {
                int guildId = guildStat.guildId;
                if (result)
                    Log.InfoFormat("Saved GuildId: {0}", guildId);
                else
                    Log.ErrorFormat("Fail to save GuildId: {0}", guildId);
            });
            guildStat.saveToDB = false;
        }

        public static void OnCharacterLogout(CharacterData charData, DateTime logoutDT)
        {
            int guildId = charData.GuildId;
            if(guildId != 0 && GuildList.ContainsKey(guildId))
            {
                GuildStatsServer guildStats = GuildList[guildId];
                Dictionary<string, GuildMemberStats> memberStatsDict = guildStats.GetMemberStatsDict();
                if (memberStatsDict.ContainsKey(charData.Name))
                {
                    GuildMemberStats memberStats = memberStatsDict[charData.Name];
                    if (memberStats.online)
                    {
                        int combatscorediff = charData.EquipScore - memberStats.combatScore;
                        memberStats.online = false;
                        memberStats.combatScore = charData.EquipScore;
                        guildStats.members[memberStats.localObjIdx] = memberStats.ToString();
                        guildStats.totalCombatScore += combatscorediff;
                    }
                }
            }
        }

        public static void RefreshTotalCombatScore(int guildId)
        {
            if(guildId != 0 && GuildList.ContainsKey(guildId))
            {
                GuildStatsServer guildStats = GuildList[guildId];
                Dictionary<string, GuildMemberStats> memberStatsDict = guildStats.GetMemberStatsDict();
                int totalScore = 0;
                foreach(KeyValuePair<string, GuildMemberStats> kvp in memberStatsDict)
                    totalScore += kvp.Value.combatScore;
                guildStats.totalCombatScore = totalScore; // Set to new combat score
            }
        }

        public static void OnNewDay()
        {
            var saved = GameApplication.dbRepository.Character.ClearFundToday(GameApplication.Instance.GetMyServerId());
            foreach (KeyValuePair<int, GuildStatsServer> kvp in GuildList)
            {
                GuildStatsServer guildStats = kvp.Value;
                Dictionary<string, GuildMemberStats> memberStatsDict = guildStats.GetMemberStatsDict();
                foreach (KeyValuePair<string, GuildMemberStats> members in memberStatsDict)
                {
                    GuildMemberStats memberStats = members.Value;
                    memberStats.OnNewDay();
                    guildStats.members[memberStats.localObjIdx] = memberStats.ToString();
                }
                // SM Boss reset only if boss is 0 hp
                GuildSMBossJson guildSMBossJson = GuildRepo.GetGuildSMBossByLvl(guildStats.SMBossLevel);
                if (guildSMBossJson != null && guildStats.SMBossDmgDone >= guildSMBossJson.healthmax)
                    guildStats.SMBossDmgDone = 0;

                guildStats.DreamHouseFavourability = 0; // Dream house reset

                guildStats.saveToDB = true;
            }
        }

        public static bool EnterRealmGuildSMBoss(int guildId, int realmId, string levelScene, GameClientPeer peer)
        {
            GuildStatsServer guildStats = GetGuildById(guildId);
            if (guildStats == null)
                return false;

            string smRoomGuid = guildStats.SMBossRoomGuid;
            if (!string.IsNullOrEmpty(smRoomGuid))
            {
                int refCount = GameApplication.Instance.GameCache.TryGetRoomReferenceCount(smRoomGuid);
                if (refCount == 0)
                    peer.TransferRoom(smRoomGuid, levelScene);
                else if (refCount == -1)
                    guildStats.SMBossRoomGuid = peer.CreateRealm(realmId, levelScene);
                else
                    peer.ZRPC.CombatRPC.Ret_SendSystemMessage("ret_Guild_SMBossAtkInProgress", "", false, peer);
            }
            else
                guildStats.SMBossRoomGuid = peer.CreateRealm(realmId, levelScene);
            return true;
        }

        private static void CreateDonateCDTime(GuildStatsServer guildStats,string charName) {
            if (guildStats.donateRequestCDTime.ContainsKey(charName))
                guildStats.donateRequestCDTime[charName] = DateTime.Now.AddHours(GameConfig.Donate_CanAddRequestTimeHour);
            else
                guildStats.donateRequestCDTime.Add(charName, DateTime.Now.AddHours(GameConfig.Donate_CanAddRequestTimeHour));
            guildStats.saveToDB = true;
            guildStats.mGuildDataDirty = true;
        }

        private static void RemoveDonateCDTime(GuildStatsServer guildStats, string charName)
        {
            if (guildStats.donateRequestCDTime.ContainsKey(charName) == false)
                return;
            guildStats.donateRequestCDTime.Remove(charName);
            guildStats.saveToDB = true;
            guildStats.mGuildDataDirty = true;
        }

        public static void OnGuildRealmCompleted(int guildId, string roomguild, string killer)
        {
            //bool isSuccess = false;
            //string newBossInfo = "";
            //byte newBossSeq = 0;
            //GuildStats guildStats = GuildList[guildId];
            //if(guildStats != null && guildStats.guildRoomGuild == roomguild)
            //{
            //    guildStats.guildRoomGuild = string.Empty; // empty room string
            //    newBossSeq = (byte)(guildStats.currBossSequence+1);
            //    string[] gBossStrSplit = guildStats.guildBossInfo.Split('|');
            //    if(gBossStrSplit != null)
            //    {
            //        int gBossStrSplitLen = gBossStrSplit.Length;
            //        GuildBossJson gBossJson = GuildRepo.GetGuildBossBySequence(guildStats.currBossSequence);
            //        bool isUpdated = false;
            //        StringBuilder sb = new StringBuilder();
            //        for(int i = 0; i<gBossStrSplitLen; ++i)
            //        {
            //            string[] gBossStr = gBossStrSplit[i].Split('`');
            //            string bossKiller = (gBossStr != null) ? gBossStr[1] : "";
            //            if(!isUpdated && string.IsNullOrEmpty(bossKiller))
            //            {
            //                sb.Append(gBossJson.id);
            //                sb.Append('`');
            //                sb.Append(killer);
            //                isUpdated = true;
            //                isSuccess = true;
            //            }
            //            else
            //                sb.Append(gBossStrSplit[i]);
            //            if(i < gBossStrSplitLen-1)
            //                sb.Append('|');
            //        }
            //        newBossInfo = sb.ToString();
            //    }
            //}
            //if(isSuccess)
            //{
            //    guildStats.currBossSequence = newBossSeq;
            //    guildStats.guildBossInfo = newBossInfo;
            //    guildStats.guildRoomGuild = "";
            //}
        }
    }
}

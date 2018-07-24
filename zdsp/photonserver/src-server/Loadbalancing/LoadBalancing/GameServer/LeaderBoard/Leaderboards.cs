using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer
{
    #region Leader Boards
    //幫會排行榜
    public class GuildLeaderBoard : BaseLeaderBoard
    {
        public GuildLeaderBoard() : base(LeaderboardType.Guild, 10, "LeaderBoard_RankGuild")
        { }

        public async Task ProcessUpdate()
        {
            DateTime old = lbData.lastupdate;
            lbData.lastupdate = DateTime.Now;
            await SaveToDB(old);
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
        }
    }

    //拉攏英雄排行榜
    public class GuildHeroHouseLeaderBoard : BaseLeaderBoard
    {
        public GuildHeroHouseLeaderBoard() : base(LeaderboardType.HeroHouse, 10, "")
        { }

        public override async Task ProcessUpdate(bool force)
        {
            if (force || CanUpdate())
            {
                lbData.lbRanking.Clear();
                mSerializedData = "";
                RebuildData(null);
                DateTime old = lbData.lastupdate;
                lbData.lastupdate = DateTime.Now;
                await SaveToDB(old);
            }
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            //Dictionary<int, HeroesHouseHeroInfo> heroList = HeroesHouseRules.HeroesList;
            //Dictionary<int, HeroesHouseStats> guildHeroesHouseList = HeroesHouseRules.GuildHeroesHouseList;
            //Dictionary<int, GuildStatsServer> guildList = GuildRules.GuildList;
            //Dictionary<int, LeaderBoardRowData> guildHeros = new Dictionary<int, LeaderBoardRowData>();
            //foreach(HeroesHouseHeroInfo value in heroList.Values)
            //{
            //    int guildid = value.currentGuildId;
            //    if (guildid > 0)
            //    {
            //        if (!guildHeros.ContainsKey(guildid))             
            //            guildHeros[guildid] = new LeaderBoardRowData(0, guildList[guildid].name, (FactionType)guildList[guildid].faction, "", 0, 0);
            //        guildHeros[guildid].para1++;
            //    }
            //}
            //foreach (var kvp in guildHeroesHouseList)
            //{
            //    int guildid = kvp.Key;
            //    int friendship = kvp.Value.GetFriendshipTotalPoint();
            //    if (friendship == 0)
            //        continue;
            //    if (!guildHeros.ContainsKey(guildid))
            //        guildHeros[guildid] = new LeaderBoardRowData(0, guildList[guildid].name, (FactionType)guildList[guildid].faction, "", 0, 0);
            //    guildHeros[guildid].score = friendship;
            //}
            //List<LeaderBoardRowData> sortedList = guildHeros.Values.OrderByDescending(x => x.para1).ThenByDescending(x => x.score).Take(rankingCount).ToList();
            //foreach (var rankdata in sortedList)
            //    lbData.lbRanking.Add(rankdata);
        }
    }

    public class FactionWarLeaderBoard : BaseLeaderBoard
    {
        public FactionWarLeaderBoard() : base(LeaderboardType.FactionWar, 4, "")
        { }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
        }
    }

    //戰鬥力排行榜
    public class GearScoreLeaderBoard : BaseLeaderBoard
    {
        public GearScoreLeaderBoard() : base(LeaderboardType.GearScore, 50, "LeaderBoard_RankGearScore")
        {
        }

        public override void LoadFromDB()
        {
            base.LoadFromDB();
            int count = lbData.lbRanking.Count;
            if (count > 0)
            {
                List<string> charnames = new List<string>();
                for (int index = 0; index < count; index++)
                    charnames.Add(lbData.lbRanking[index].rankName);
                var results = GameApplication.dbRepository.Character.GetCharacterByNames(charnames);
                for (int index = 0; index < results.Count; index++)
                {
                    var row = results[index];
                    string charname = (string)row["charname"];
                    string chardata = (string)row["characterdata"];
                    ProcessAvatar(charname, chardata, LBType);
                }
            }
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                int progresslevel = (int)row["progresslevel"];
                int combatscore = (int)row["combatscore"];
                string chardata = (string)row["characterdata"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(combatscore, charname, FactionType.None, guildName, progresslevel, 0);
                lbData.lbRanking.Add(rankdata);
                ProcessAvatar(charname, chardata, LBType);
            }
        }
    }

    //英雄卡戰鬥力排行榜
    public class HeroBookLeaderBoard : BaseLeaderBoard
    {
        public HeroBookLeaderBoard() : base(LeaderboardType.HeroBook, 50, "LeaderBoard_RankHeroBook")
        { }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                short herocollected = (short)row["herocollected"];
                int heroscore = (int)row["heroscore"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(heroscore, charname, FactionType.None, guildName, herocollected, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }

    //財富排行榜
    public class GoldLeaderBoard : BaseLeaderBoard
    {
        public GoldLeaderBoard() : base(LeaderboardType.Gold, 50, "LeaderBoard_RankGold")
        { }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                int gold = (int)row["money"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(gold, charname, FactionType.None, guildName, 0, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }

    //寵物排行榜
    public class PetLeaderBoard : BaseLeaderBoard
    {
        public PetLeaderBoard() : base(LeaderboardType.Pet, 50, "LeaderBoard_RankPet")
        { }

        public override void LoadFromDB()
        {
            base.LoadFromDB();
            int count = lbData.lbRanking.Count;
            if (count > 0)
            {
                List<string> charnames = new List<string>();
                for (int index = 0; index < count; index++)
                    charnames.Add(lbData.lbRanking[index].rankName);
                var results = GameApplication.dbRepository.Character.GetCharacterByNames(charnames);
                for (int index = 0; index < results.Count; index++)
                {
                    var row = results[index];
                    string charname = (string)row["charname"];
                    string chardata = (string)row["characterdata"];
                    ProcessAvatar(charname, chardata, LBType);
                }
            }
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                int petscore = (int)row["petscore"];
                short petcollected = (short)row["petcollected"];
                string chardata = (string)row["characterdata"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(petscore, charname, FactionType.None, guildName, petcollected, 0);
                lbData.lbRanking.Add(rankdata);
                ProcessAvatar(charname, chardata, LBType);
            }
        }
    }

    //陣營殺人排行榜
    public class FactionKillLeaderBoard : BaseLeaderBoard
    {
        public FactionKillLeaderBoard() : base(LeaderboardType.FactionKill, 50, "LeaderBoard_RankFactionKill")
        { }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                FactionType faction = (FactionType)((byte)row["faction"]);
                int factionkill = (int)row["factionkill"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(factionkill, charname, faction, guildName, 0, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }

    //陣營死亡排行榜
    public class FactionDeathLeaderBoard : BaseLeaderBoard
    {
        public FactionDeathLeaderBoard() : base(LeaderboardType.FactionDeath, 50, "LeaderBoard_RankFactionDeath")
        { }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildlist = GuildRules.GuildList;
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int guildid = (int)row["guildid"];
                string charname = (string)row["charname"];
                FactionType faction = (FactionType)((byte)row["faction"]);
                int factiondeath = (int)row["factiondeath"];
                string guildName = "";
                if (guildid > 0 && guildlist.ContainsKey(guildid))
                    guildName = guildlist[guildid].name;

                var rankdata = new LeaderBoardRowData(factiondeath, charname, faction, guildName, 0, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }

    //公會總戰力排行，會列出所有公會的排名
    public class GuildRankLeaderBoard : BaseLeaderBoard
    {
        public GuildRankLeaderBoard() : base(LeaderboardType.GuildRankAll, 999999, "")
        { }

        public override void LoadFromDB()
        {
            base.LoadFromDB();
            Dictionary<int, GuildStatsServer> guildList = GuildRules.GuildList;
            int rank = 0;
            for (int index = 0; index < lbData.lbRanking.Count; index++)
            {
                rank = index + 1;
                int guildid = lbData.lbRanking[index].para1;
                if (guildList.ContainsKey(guildid))
                    guildList[guildid].rank = rank;
            }
        }

        public override async Task ProcessUpdate(bool force)
        {
            if (force || CanUpdate())
            {
                lbData.lbRanking.Clear();
                mSerializedData = "";
                RebuildData(null);
                DateTime old = lbData.lastupdate;
                lbData.lastupdate = DateTime.Now;
                await SaveToDB(old);
            }
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            Dictionary<int, GuildStatsServer> guildList = GuildRules.GuildList;
            List<GuildStatsServer> sortedList = guildList.Values.OrderByDescending(x => x.totalCombatScore).ThenByDescending(x => x.guildLevel).ThenByDescending(x => x.GetMemberStatsDict().Count).ToList();
            int rank = 0;
            GuildLeaderBoard guildLeaderBoard = GameApplication.Instance.Leaderboard.GetLeaderBoard(LeaderboardType.Guild) as GuildLeaderBoard;
            guildLeaderBoard.ClearData();
            for (int index = 0; index < sortedList.Count; index++)
            {
                rank = index + 1;
                GuildStatsServer guildstats = sortedList[index];
                if (guildstats.rank != rank)
                    guildstats.rank = rank;
                lbData.lbRanking.Add(new LeaderBoardRowData(guildstats.totalCombatScore, "", FactionType.None, "", guildstats.guildId, 0));
                if (rank <= guildLeaderBoard.rankingCount)
                {
                    var rankdata = new LeaderBoardRowData(guildstats.totalCombatScore, guildstats.name, (FactionType)guildstats.faction, "", guildstats.guildLevel, guildstats.GetMemberStatsDict().Count);
                    guildLeaderBoard.lbData.lbRanking.Add(rankdata);
                }
            }
            var task = guildLeaderBoard.ProcessUpdate();
        }
    }

    //列出每個faction的人數 從小到大
    public class FactionRecommendLeaderBoard : BaseLeaderBoard
    {
        public FactionType mFactionRecommend = FactionType.Dragon;

        public FactionRecommendLeaderBoard() : base(LeaderboardType.FactionRecommend, 999999, "LeaderBoard_FactionRecommend")
        { }

        protected override void UpdateNextUpdate()
        {
            if (lbData.lbRanking.Count > 0)
                mFactionRecommend = lbData.lbRanking[0].faction;
            else
                mFactionRecommend = FactionType.Dragon + GameUtils.RandomInt(0, 3);
            //every 1 hours.
            DateTime lastupdate = (lbData.lastupdate == DateTime.MinValue) ? DateTime.Now : lbData.lastupdate;
            nextUpdate = lastupdate.AddHours(1);
        }

        protected override async Task SaveToDB(DateTime old)
        {
            UpdateNextUpdate();
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                FactionType faction = (FactionType)((byte)row["faction"]);
                int count = (int)row["players"];
                var rankdata = new LeaderBoardRowData(count, "", faction, "", 0, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }


    //列出前200名玩家的平均等级
    public class WorldLevelLeaderBoard : BaseLeaderBoard
    {
        public int mWorldLevel = 1;

        public WorldLevelLeaderBoard() : base(LeaderboardType.WorldLevel, 200, "LeaderBoard_WorldLevel")
        { }

        protected override void UpdateNextUpdate()
        {
            base.UpdateNextUpdate();
            if (lbData.lbRanking.Count > 0 && lbData.lbRanking[0].para1 > 0)
                mWorldLevel = (int)lbData.lbRanking[0].score / lbData.lbRanking[0].para1;
            else
                mWorldLevel = 60;
        }

        protected override async Task SaveToDB(DateTime old)
        {
            UpdateNextUpdate();
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            for (int index = 0; index < tempResults.Count; index++)
            {
                var row = tempResults[index];
                int count = (int)row["players"];
                int totallvl = count == 0 ? 0 : (int)row["totallvl"];               
                var rankdata = new LeaderBoardRowData(totallvl, "", FactionType.None, "", count, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }

    public class ArenaLeaderBoard : BaseLeaderBoard
    {
        public ArenaLeaderBoard() : base(LeaderboardType.Arena, 50, "")
        { }

        public override void LoadFromDB()
        {            
            return;//does not load from DB
        }

        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
        }

        public void InitArenaRank(List<ArenaPlayerRecord> arenaRecords)
        {
            //if (ArenaRankRepo.mFakeRankArray == null)
            //    return;
            //ClearData();
            //for (int i = 0; i < rankingCount; i++)
            //{
            //    if (i < arenaRecords.Count)
            //    {
            //        var record = arenaRecords[i];
            //        int rank = i + 1;
            //        if (record.Fake)
            //        {
            //            ArenaFakeRankJson fakeJson = ArenaRankRepo.mFakeRankArray[i];
            //            lbData.lbRanking.Add(new LeaderBoardRowData(fakeJson.equipscore, fakeJson.localizedname, FactionType.None, "", 0, 0));
            //        }
            //        else
            //        {
            //            string charname = record.CharacterCreationData.Name;
            //            lbData.lbRanking.Add(new LeaderBoardRowData(record.CharacterCreationData.EquipScore, charname, FactionType.None, GuildRules.GetGuildNameByPlayer(charname), 1, 0));
            //        }
            //    }
            //}
        }

        public void UpdateArenaRank(int rank, ArenaPlayerRecord record)
        {
            LeaderBoardRowData rowdata = lbData.lbRanking[rank];
            rowdata.para1 = record.Fake ? 0 : 1;
            if (record.Fake)
            {
                //ArenaFakeRankJson fakeJson = ArenaRankRepo.mFakeRankArray[rank];
                //rowdata.score = fakeJson.equipscore;
                //rowdata.rankName = fakeJson.localizedname;
                //rowdata.guild = "";
            }
            else
            {
                string charname = record.CharacterCreationData.Name;
                rowdata.score = record.CharacterCreationData.EquipScore;
                rowdata.rankName = charname;
                rowdata.guild = GuildRules.GetGuildNameByPlayer(charname);
            }
            mSerializedData = "";
        }
    }


    //上一場世界王排名
    public class WorldBossRecordLeaderBoard : BaseLeaderBoard
    {
        public KeyValuePair<string, int>[] ladderData;
        private bool updateFlag = false;

        public WorldBossRecordLeaderBoard() : base(LeaderboardType.WorldBossRecord, 50, "LeaderBoard_WorldBossRecord")
        { }

        public async Task ManualUpdate(KeyValuePair<string, int>[] data)
        {
            this.ladderData = data;
            this.updateFlag = true;
            await ProcessUpdate(true);
        }

        public override async Task ProcessUpdate(bool force)
        {
            if (this.updateFlag)
            {
                this.updateFlag = false;
                lbData.lbRanking.Clear();
                mSerializedData = "";
                RebuildData(null);
                DateTime old = lbData.lastupdate;
                lbData.lastupdate = DateTime.Now;
                await SaveToDB(old);
            }
        }
        protected override void RebuildData(List<Dictionary<string, object>> tempResults)
        {
            for (int index = 0; index < ladderData.Length; index++)
            {
                var row = ladderData[index];
                int score = row.Value;
                string name = row.Key;
                var rankdata = new LeaderBoardRowData(score, name, FactionType.None, "", 0, 0);
                lbData.lbRanking.Add(rankdata);
            }
        }
    }
    #endregion
}

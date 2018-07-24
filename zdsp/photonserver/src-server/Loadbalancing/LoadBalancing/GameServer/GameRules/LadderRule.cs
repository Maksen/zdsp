using System;
using System.Text;
using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using ExitGames.Logging;
using Zealot.Server.Entities;
using Zealot.Common;

namespace Zealot.Server.Rules
{
    public static class LadderRules
    {
        public class RealmFastCompleteOrHighestScoreRecord
        {
            public string playername;
            public int criteria;
            public bool isfast;

            public RealmFastCompleteOrHighestScoreRecord(string playername, int criteria, bool isfast)
            {
                this.playername = playername;
                this.criteria = criteria;
                this.isfast = isfast;
            }
        }

        public class ArenaReports
        {
            public int count;
            public string reports;

            public ArenaReports(string reports)
            {
                this.reports = reports;
                count = (reports == "") ? 0 : reports.Split(';').Length;
            }

            public void AddNew(string report)
            {
                if (count == ArenaReportMax)
                {
                    int index = reports.IndexOf(';');
                    reports = reports.Substring(index + 1);
                    count--;
                }
                if (count > 0)
                    reports += ";" + report;
                else
                    reports += report;
                count++;
            }
        }

        public static Dictionary<int, RealmFastCompleteOrHighestScoreRecord> mRealmFastCompleteOrHighestScore = new Dictionary<int, RealmFastCompleteOrHighestScoreRecord>();
        public static ArenaRankingRecords mLadderArenaRecord = new ArenaRankingRecords(); 
        public static Dictionary<string, int> mLadderArenaNameToIndex = new Dictionary<string, int>();
        public static Dictionary<string, ArenaReports> mArenaReports = new Dictionary<string, ArenaReports>();
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public const int ArenaReportMax = 50;

        public static void Init()
        {
            List<Dictionary<string, object>> realmRecords = GameApplication.dbRepository.RealmLadder.GetRecords();
            foreach (var realm_record in realmRecords)
            {
                mRealmFastCompleteOrHighestScore.Add((int)realm_record["realmid"], new RealmFastCompleteOrHighestScoreRecord((string)realm_record["charname"], (int)realm_record["criteria"], (bool)realm_record["isfast"]));
            }

            int serverId = GameApplication.Instance.GetMyServerId();
            string arenaData = GameApplication.dbRepository.Ladder.GetArenaRecords();

            if (arenaData != "")
            {
                mLadderArenaRecord = ArenaRankingRecords.DeserializeFromDB(arenaData);
                int index = 0;
                foreach(var entry in mLadderArenaRecord.PlayerRecords)
                {                    
                    mLadderArenaRecord.RecordString.Add(entry.GetDisplayString());
                    if (!entry.Fake)
                    {
                        string playername = entry.CharacterCreationData.Name;
                        mLadderArenaNameToIndex.Add(playername, index);
                        string reports = GameApplication.dbRepository.Character.GetArenaReport(playername);
                        mArenaReports.Add(playername, new ArenaReports(reports));
                    }
                    index++;
                }
            }
            else
            {
                //mLadderArenaRecord = new ArenaRankingRecords();
                //foreach (var entry in ArenaRankRepo.mFakeRankArray)
                //{
                //    if (entry == null)
                //        continue;
                //    ArenaPlayerRecord record = new ArenaPlayerRecord();
                //    record.Fake = true;
                //    mLadderArenaRecord.Add(record);
                //}
            }
        }

        public static void UpdateRealmRecord(int realmid, string playername, int criteria, bool isfast, bool forceupdate = false)
        {
            RealmFastCompleteOrHighestScoreRecord current_record;
            if (mRealmFastCompleteOrHighestScore.TryGetValue(realmid, out current_record))
            {
                bool is_new_record = isfast ? current_record.criteria > criteria : current_record.criteria < criteria;
                if (forceupdate || is_new_record)
                {
                    current_record.playername = playername;
                    current_record.criteria = criteria;
                    var saved = GameApplication.dbRepository.RealmLadder.Insert_Update(realmid, playername, criteria, isfast);
                }
            }
            else
            {
                RealmFastCompleteOrHighestScoreRecord new_record = new RealmFastCompleteOrHighestScoreRecord(playername, criteria, isfast);
                mRealmFastCompleteOrHighestScore.Add(realmid, new_record);
                var saved = GameApplication.dbRepository.RealmLadder.Insert_Update(realmid, playername, criteria, isfast);
            }
        }

        public static RealmFastCompleteOrHighestScoreRecord GetRealmRecord(int realmid)
        {
            RealmFastCompleteOrHighestScoreRecord current_record;
            if (mRealmFastCompleteOrHighestScore.TryGetValue(realmid, out current_record))
                return current_record;
            return null;
        }

        public static int GetArenaRank(string playername)
        {
            if (mLadderArenaNameToIndex.ContainsKey(playername))
                return mLadderArenaNameToIndex[playername];
            return 500;
        }

        public static string GetArenaReport(string playername)
        {
            if (mArenaReports.ContainsKey(playername))
                return mArenaReports[playername].reports;
            return "";
        }

        public static string GetArenaChallengers(string playername, out int myRank)
        {
            string retval = "";
            int rank = 500;
            if (mLadderArenaNameToIndex.ContainsKey(playername))
                rank = mLadderArenaNameToIndex[playername];
            myRank = rank;
            int total = mLadderArenaRecord.PlayerRecords.Count;
            ArenaChallengers challengers = new ArenaChallengers();
            if (rank <= 399)
            {
                int step = 1;
                if (rank <= 9)
                    step = 1;
                else if (rank <= 99)
                    step = 2;
                else if (rank <= 299)
                    step = 3;
                else
                    step = 4;                   
                for (int index = rank - step; index >= rank - step * 3; index -= step)
                {
                    if (index <= 2)
                        break;
                    challengers.Ranks.Add(index);
                    challengers.Infos.Add(mLadderArenaRecord.RecordString[index]);
                }
            }
            else
            {
                for (int count = 1; count <= 3; count++)
                {
                    int index = GameUtils.RandomInt(rank - 5*count, rank - 1 - 5*(count-1));
                    challengers.Ranks.Add(index);
                    challengers.Infos.Add(mLadderArenaRecord.RecordString[index]);
                }
            }
            for (int index = 2; index >= 0; index--)
            {
                challengers.Ranks.Add(index);
                challengers.Infos.Add(mLadderArenaRecord.RecordString[index]);
            }
            retval = challengers.Serialize();
            return retval;
        }

        public static bool ValidateChallengeArena(string playername, int rank)
        {
            int my_rank = 500;
            if (mLadderArenaNameToIndex.ContainsKey(playername))
                my_rank = mLadderArenaNameToIndex[playername];
            if (rank >= mLadderArenaRecord.PlayerRecords.Count)
                return false;
            if (rank <= 2)
            {
                if (rank == my_rank)
                    return false;
            }
            else if (rank >= my_rank)
                return false;
            return true;
        }

        public static ArenaPlayerRecord GetArenaPlayerRecord(int rank)
        {
            return mLadderArenaRecord.PlayerRecords[rank];
        }

        public static int ArenaWin(Player player, int rank)
        {
            string playername = player.Name;                
            bool opponentfake = mLadderArenaRecord.PlayerRecords[rank].Fake;
            string opponentname = "";
            if (opponentfake)
            {
                //ArenaFakeRankJson fakeJson = ArenaRankRepo.mFakeRankArray[rank];
                //opponentname = fakeJson.localizedname;
            }
            else
                opponentname = mLadderArenaRecord.PlayerRecords[rank].CharacterCreationData.Name;
            int my_rank = 500; //index = rank -1
            if (mLadderArenaNameToIndex.ContainsKey(playername))
                my_rank = mLadderArenaNameToIndex[playername];

            string my_report;
            string opponent_report;
            if (my_rank > rank)
            {
                if (my_rank <= 499)
                {
                    mLadderArenaRecord.PlayerRecords[my_rank] = mLadderArenaRecord.PlayerRecords[rank];
                    mLadderArenaRecord.RecordString[my_rank] = mLadderArenaRecord.RecordString[rank];
                    if (!opponentfake)
                        mLadderArenaNameToIndex[opponentname] = my_rank;
                }
                else if (!opponentfake)
                    mLadderArenaNameToIndex.Remove(opponentname);

                ArenaPlayerRecord record = new ArenaPlayerRecord();
                player.SetArenaRecord(record);
                mLadderArenaRecord.PlayerRecords[rank] = record;
                mLadderArenaRecord.RecordString[rank] = record.GetDisplayString();

                if (mLadderArenaNameToIndex.ContainsKey(playername))
                    mLadderArenaNameToIndex[playername] = rank;
                else
                    mLadderArenaNameToIndex.Add(playername, rank);
                my_report = "1|" + opponentname + "|1" + "|" + (rank + 1); // i challenge opponent win
                opponent_report = "2|" + playername + "|2" + "|" + (my_rank + 1); // xx challenged me, i lose.
            }
            else
            {
                my_report = "1|" + opponentname + "|1"; // i challenge opponent win
                opponent_report = "2|" + playername + "|2"; // xx challenged me, i lose.
            }

            if (!mArenaReports.ContainsKey(playername))
                mArenaReports.Add(playername, new ArenaReports(""));
            mArenaReports[playername].AddNew(my_report);
            var my_saved = GameApplication.dbRepository.Character.UpdateArenaReport(playername, mArenaReports[playername].reports);
            if (!opponentfake)
            {
                if (!mArenaReports.ContainsKey(opponentname))
                    mArenaReports.Add(opponentname, new ArenaReports(""));
                mArenaReports[opponentname].AddNew(opponent_report);
                var opponent_saved = GameApplication.dbRepository.Character.UpdateArenaReport(opponentname, mArenaReports[opponentname].reports);
            }                        
            mLadderArenaRecord.IsDirty = true;

            if (my_rank > rank)
            {
                if (my_rank < 50)
                    GameApplication.Instance.Leaderboard.OnArenaRankChange(my_rank, mLadderArenaRecord.PlayerRecords[my_rank]);
                if (rank < 50)
                    GameApplication.Instance.Leaderboard.OnArenaRankChange(rank, mLadderArenaRecord.PlayerRecords[rank]);
                if (rank <= 2)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(playername);
                    sb.Append(';');
                    sb.Append(rank+1);
                    sb.Append(';');
                    sb.Append(opponentname);
                    GameApplication.Instance.BroadcastMessage(BroadcastMessageType.ArenaRankUp, sb.ToString());
                }
            }

            //log
            int usedEntry = player.Slot.CharacterData.ArenaInventory.Entries + 1;
            string message = string.Format("oldrank:{0}|newrank:{1}|enemy:{2}|usedEntry:{3}", my_rank + 1, rank + 1, opponentname, usedEntry);
            Zealot.Logging.Client.LogClasses.ArenaWin arenaWinLog = new Zealot.Logging.Client.LogClasses.ArenaWin();
            arenaWinLog.userId = player.Slot.mUserId;
            arenaWinLog.charId = player.Slot.GetCharId();
            arenaWinLog.message = message;
            arenaWinLog.oldRank = my_rank + 1;
            arenaWinLog.winRank = rank + 1;
            arenaWinLog.enemy = opponentname;
            arenaWinLog.usedEntry = usedEntry;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(arenaWinLog);

            return my_rank > rank ? rank : my_rank;
        }

        public static void ArenaLose(Player player, int rank)
        {
            string playername = player.Name;
            bool opponentfake = mLadderArenaRecord.PlayerRecords[rank].Fake;
            string opponentname = "";
            if (opponentfake)
            {
                //ArenaFakeRankJson fakeJson = ArenaRankRepo.mFakeRankArray[rank];
                //opponentname = fakeJson.localizedname;
            }
            else
                opponentname = mLadderArenaRecord.PlayerRecords[rank].CharacterCreationData.Name;
            string my_report = "1|" + opponentname + "|2"; // i challenge opponent lose              
            if (!mArenaReports.ContainsKey(playername))
                mArenaReports.Add(playername, new ArenaReports(""));
            mArenaReports[playername].AddNew(my_report);
            var my_saved = GameApplication.dbRepository.Character.UpdateArenaReport(playername, mArenaReports[playername].reports);

            if (!opponentfake)
            {
                string opponent_report = "2|" + playername + "|1"; // xx challenged me, i win.
                if (!mArenaReports.ContainsKey(opponentname))
                    mArenaReports.Add(opponentname, new ArenaReports(""));
                mArenaReports[opponentname].AddNew(opponent_report);
                var opponent_saved = GameApplication.dbRepository.Character.UpdateArenaReport(opponentname, mArenaReports[opponentname].reports);
            }   
        }

        public static void ResetPlayerRank(string playername)
        {
            int my_rank = GetArenaRank(playername);
            if (my_rank < 500)
            {
                ArenaPlayerRecord record = new ArenaPlayerRecord();
                record.Fake = true;
                mLadderArenaRecord.PlayerRecords[my_rank] = record;
                mLadderArenaRecord.RecordString[my_rank] = record.GetDisplayString();
                mLadderArenaNameToIndex.Remove(playername);
                mLadderArenaRecord.IsDirty = true;
                if (my_rank < 50)
                    GameApplication.Instance.Leaderboard.OnArenaRankChange(my_rank, record);
            }
        }

        public static void OnPlayerOnline(string playername, string report)
        {
            if (!mArenaReports.ContainsKey(playername))
                mArenaReports.Add(playername, new ArenaReports(report));
        }

        public static void OnPlayerOffline(string playername)
        {
            if (mArenaReports.ContainsKey(playername) && !mLadderArenaNameToIndex.ContainsKey(playername))
                mArenaReports.Remove(playername);
        }

        public static void SaveArenaRank()
        {
            string records = "";
            if (mLadderArenaRecord.IsDirty)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                records = mLadderArenaRecord.SerializeForDB(true);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                log.DebugFormat("SaveArenaRank {0}", elapsedMs);
                mLadderArenaRecord.IsDirty = false;
            }
            if (records != "")
            {
                var saved = GameApplication.dbRepository.Ladder.UpdateArena(records);
            }
        }
    }
}

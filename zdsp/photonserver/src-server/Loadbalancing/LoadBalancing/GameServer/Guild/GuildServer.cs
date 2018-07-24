using System;
using System.Text;
using Zealot.Common.Entities;
using Zealot.Common;
using Zealot.Repository;

namespace Photon.LoadBalancing.GameServer
{
    public class GuildStatsServer : GuildStats
    {
        public DateTime mLastWorldChannelInvitation; //10 minutes cd;
        public bool mGuildDataDirty;
        public string mGuildDataStr;
        public GuildHistory mGuildHistory;
        public string SMBossRoomGuid;
        public long guildBossLastResetTick;
        public bool saveToDB;

        public GuildStatsServer() : base()
        {
            mLastWorldChannelInvitation = DateTime.Now.AddHours(-1);
            mGuildDataDirty = false;
            mGuildHistory = new GuildHistory();
            SMBossRoomGuid = "";
            guildBossLastResetTick = 0;
            saveToDB = false;
        }

        public int Member_GetEmptySlot()
        {
            int count = members.Count;
            for (int index = 0; index < count; ++index)
            {
                if (members[index] == null)
                    return index;
            }
            return -1;
        }

        public int Request_GetEmptySlot()
        {
            int count = memberRequests.Count;
            for (int index = 0; index < count; ++index)
            {
                if (memberRequests[index] == null)
                    return index;
            }
            return -1;
        }

        public void UpdateTechString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in mGuildTechDict)
                sb.AppendFormat("{0}|{1};", (byte)kvp.Key, kvp.Value);
            techs = sb.ToString().TrimEnd(';');
        }

        public void ParseTech()
        {
            mGuildTechDict.Clear();
            if (!string.IsNullOrEmpty(techs))
            {
                string[] techs_array = techs.Split(';');
                for (int index = 0; index < techs_array.Length; index++)
                {
                    string[] tech_detail = techs_array[index].Split('|');
                    byte techtype = byte.Parse(tech_detail[0]);
                    int level = int.Parse(tech_detail[1]);
                    mGuildTechDict[(GuildTechType)techtype] = level;
                }
            }
            else
            {
                mGuildTechDict[GuildTechType.Quest] = 1;
                mGuildTechDict[GuildTechType.Love] = 1;
                UpdateTechString();
            }
        }

        public void AddHistory(GuildHistoryType type, string paramters)
        {
            mGuildHistory.Add(type, paramters);
            saveToDB = true;
        }

        public bool UpdateWorldInvitation(DateTime now)
        {
            if ((now - mLastWorldChannelInvitation).TotalMinutes > 10)
            {
                mLastWorldChannelInvitation = now;
                return true;
            }
            return false;
        }

        public void UpdateSMBossLevel(int totalDmgDone, int bossMaxHealth)
        {
            if (totalDmgDone >= bossMaxHealth)
            {
                int lvlLimit = (int)GetGuildTechStats(GuildTechType.Love);
                if (SMBossLevel < lvlLimit && GuildRepo.GetGuildSMBossByLvl(SMBossLevel+1) != null)
                {
                    ++SMBossLevel;
                    SMBossDmgDone = 0;
                }
                else
                    SMBossDmgDone = totalDmgDone;
                saveToDB = true;
            }
            else if (totalDmgDone > SMBossDmgDone)
            {
                SMBossDmgDone = totalDmgDone;
                saveToDB = true;
            }
        }
    }
}

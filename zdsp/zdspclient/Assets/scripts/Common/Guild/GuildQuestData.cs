using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Zealot.Repository;
using Kopio.JsonContracts;

namespace Zealot.Common
{
    [JsonObject (MemberSerialization=MemberSerialization.OptIn)]
    public class GuildQuestInventory
    {
        public static string TIME_FORMAT = @"MM\/dd\/yyyy HH:mm:ss";//convert datetime in this format and in current system culture.
        public static int FREE_REFRESH_TIMES_DAILY = 5;

        [JsonProperty(PropertyName = "questlist")]
        public List<GuildQuestData> questlist = new List<GuildQuestData>();

        [JsonProperty(PropertyName = "additionaltimes")]//this is the additonal refresh time
        public int additionaltimes = 0;

        [JsonProperty(PropertyName = "refreshtimesfree")]
        public int refreshtimesfree = 0;
         

        [JsonProperty(PropertyName = "finishedtimestoday")]
        public int finishedtimestoday = 0;

        [JsonProperty(PropertyName = "lastfinishday")]
        public int lastfinishday = 100;

        [JsonProperty(PropertyName = "daymaxtimes")]
        public int daymaxtimes = 3;
        public void InitDefault()
        {
            questlist.Clear();
            refreshtimesfree = FREE_REFRESH_TIMES_DAILY;
        }

        public void ResetOnNewDay(int vipAdditionals = 0, bool resetFreeTimes = true)
        {
            if(resetFreeTimes)
            {
                refreshtimesfree = FREE_REFRESH_TIMES_DAILY + vipAdditionals;
                finishedtimestoday = 0;
            } 
            else
            {
                refreshtimesfree += vipAdditionals;
            }
            
        }

        public string GetIdLists()
        {
            string res = "";
            res = questlist[0].id.ToString() + "," + questlist[1].id.ToString() + "," + questlist[2].id.ToString();
            return res;
        }

        public bool RefreshQuests(int playerlevel, bool deductfreeTimes)
        { 
            List<GuildQuestJson> avaliableList = new List<Kopio.JsonContracts.GuildQuestJson>();
            foreach (KeyValuePair<int, GuildQuestJson> entry in GuildRepo.mQuestlist)
            {
                GuildQuestJson qjson = entry.Value;
                if (qjson.questlevel <= playerlevel)
                {
                    avaliableList.Add(qjson);
                } 
            }

            if (avaliableList.Count < 3)
                return true;

            questlist.Clear();
            List<int> questids = GuildRepo.GetRandomizeQuestID(avaliableList); 
            for (int i=0;i< questids.Count; i++)
            {
                Kopio.JsonContracts.GuildQuestJson qjson = GuildRepo.GetQuestJson(questids[i]);
                GuildQuestData questdata = new GuildQuestData(questids[i], qjson.duration * 60);
                questlist.Add(questdata);
            }
            if (refreshtimesfree > 0 && deductfreeTimes)
                refreshtimesfree--;
            else if (additionaltimes > 0 && deductfreeTimes)
                additionaltimes--;
                        
            return true;
        }

        public bool AcceptQuest(int id)
        {
            foreach(GuildQuestData gqd in questlist)
            {
                if (gqd.id == id)
                {
                    gqd.status = (int)GuildQuestStatus.Accepted;
                    gqd.starttime = DateTime.UtcNow.ToString(TIME_FORMAT);
                    gqd.leftseconds = GuildRepo.GetQuestJson(gqd.id).duration * 60;
                    return true;
                }
            }
            return false; 
        }

        public bool CancelQuest(int id)
        {
            foreach (GuildQuestData gqd in questlist)
            {
                if (gqd.id == id && gqd.status == (int)GuildQuestStatus.Accepted)
                { 
                    if(gqd.leftseconds > 0)
                    {
                        //gqd.status = (int)GuildQuestStatus.Canceled;//no need this state.when cancel,quest go back to init state
                        gqd.status = (int)GuildQuestStatus.Avialiable;
                        gqd.leftseconds = GuildRepo.GetQuestJson(gqd.id).duration * 60;
                    }                       
                    else 
                        gqd.status = (int)GuildQuestStatus.CollectPending;
                    return true;
                }
            }            
            return false;
        }
        
        public bool FinishQuest(int id,   out int error)
        {
            error = 0;
            int daytoday = DateTime.Now.Day;
             
            if (finishedtimestoday >= this.daymaxtimes && lastfinishday == daytoday)
            {
                 
                error = (int)GuildQuestOperationError.QuestTimesNotEnough;
                return false; 
            }
            for(int i =0; i < questlist.Count; i++)
            {
                GuildQuestData gqd = questlist[i];
                if (gqd.id == id && gqd.status ==(int)GuildQuestStatus.CollectPending)
                {
                    DateTime start = DateTime.ParseExact(gqd.starttime, TIME_FORMAT, null);
                    TimeSpan ts= DateTime.UtcNow.Subtract(start);
                    int timeinseconds = GuildRepo.GetQuestJson(id).duration * 60;
                    if (ts.TotalSeconds >= timeinseconds)
                    {
                        gqd.status = (int)GuildQuestStatus.Finished; 
                        if (lastfinishday != daytoday)
                        {
                            lastfinishday = daytoday;
                            finishedtimestoday = 1; 
                        }else
                        {
                            if (finishedtimestoday< this.daymaxtimes)
                                finishedtimestoday++;
                        }
                        return true;
                    }
                         
                }
            }
            error = (int)GuildQuestOperationError.QuestNotFound;
            return false;
        }

        public void FastForwardQuest()
        {
            for (int i = 0; i < questlist.Count; i++)
            {
                GuildQuestData gqd = questlist[i];
                if(gqd.status == (int)GuildQuestStatus.Accepted)
                {
                    gqd.starttime = DateTime.MinValue.ToString(TIME_FORMAT);

                }
            }
        }
        public void ComputeTime()
        {
            for (int i =0; i < questlist.Count; i ++)
            {
                GuildQuestData gqd = questlist[i];
                if (gqd.status == (int)GuildQuestStatus.Accepted)
                {
                    DateTime start = DateTime.ParseExact(gqd.starttime, TIME_FORMAT, null);
                    TimeSpan ts = DateTime.UtcNow.Subtract(start);
                    int timeinseconds = GuildRepo.GetQuestJson(gqd.id).duration * 60;
                    if (ts.TotalSeconds >= timeinseconds)
                    {
                        gqd.leftseconds = 0;
                        gqd.status = (int)GuildQuestStatus.CollectPending;
                    }
                    else
                    {
                        gqd.leftseconds = timeinseconds - (int)ts.TotalSeconds;
                    }

                }
            }
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GuildQuestData
    {
        #region serializable properties         
        [JsonProperty(PropertyName = "id")]
        public int id; 
        
        [JsonProperty(PropertyName = "status")]
        public int status;

        [JsonProperty(PropertyName = "leftseconds")]
        public int leftseconds;

        [JsonProperty(PropertyName = "starttime")]
        public string starttime; 
        #endregion
         
        public GuildQuestData(int qid, int secs)
        {
            id = qid;
            status = (int)GuildQuestStatus.Avialiable;
            starttime = "";
            leftseconds = secs;
        }
          
    }
}

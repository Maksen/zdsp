﻿using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;
using Kopio.JsonContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Zealot.Server.Rules
{
    public class InteractiveTriggerRule
    {
        private static readonly PoolFiber executionFiber;
        private static Dictionary<int, List<string>> partyUsed = new Dictionary<int, List<string>>();
        public static List<InteractiveGate> interactiveEntity = new List<InteractiveGate>();

        static InteractiveTriggerRule()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }
        
        #region InteractiveTriggerEvent
        public static void UseInteractiveTrigger(int pid, string playerName, InteractiveGate mEntity, 
            int time, bool isArea)
        {
            executionFiber.Enqueue(() =>
            {
                if (!isArea)
                {
                    if(mEntity.step == Common.InteractiveTriggerStep.None)
                    {
                        SetAction(pid, time, isArea, mEntity);
                        mEntity.step = Common.InteractiveTriggerStep.OnProgress;
                    }
                    return;
                }

                if (!partyUsed.ContainsKey(pid))
                {
                    partyUsed.Add(pid, new List<string>() { playerName });
                } else
                {
                    if(!partyUsed[pid].Exists(x => x == playerName))
                        partyUsed[pid].Add(playerName);
                }

                int playerCount = partyUsed.Count;
                if (CanPartyUse(mEntity.mPropertyInfos.min, mEntity.mPropertyInfos.max, playerCount))
                {
                    SetAction(pid, time, isArea, mEntity);
                    mEntity.step = Common.InteractiveTriggerStep.OnProgress;
                }
                else
                {
                    mEntity.StopAction();
                    mEntity.step = Common.InteractiveTriggerStep.OnTrigger;
                }
            });
        }

        static void SetAction(int pid, int time, bool isArea, InteractiveGate mEntity)
        {
            Zealot.Common.Actions.InteractiveTriggerCommand cmd = new Zealot.Common.Actions.InteractiveTriggerCommand();
            cmd.entityId = pid;
            cmd.triggerTime = time;
            cmd.isArea = isArea;
            mEntity.SetAction(cmd);
        }

        public static string GetCurrentPlayer()
        {
            string playerName = string.Empty;
            return playerName;
        }

        public static void InterruptedEvent(int pid, string playerName, InteractiveGate mEntity, bool isArea)
        {
            executionFiber.Enqueue(() =>
            {
                if (isArea)
                {
                    if (partyUsed.ContainsKey(pid))
                    {
                        partyUsed[pid].Remove(playerName);
                    }

                    //if (CanPartyUse(mEntity.mPropertyInfos.min, mEntity.mPropertyInfos.max, partyUsed.Count))
                    //{
                    //    SetAction(pid, mEntity.mPropertyInfos.interactiveTime, true, mEntity);
                    //}
                }
                mEntity.step = Common.InteractiveTriggerStep.None;
                mEntity.StopAction();
            });
        }

        public static void CompeletedEvent(int pid)
        {
            executionFiber.Enqueue(() =>
            {
                if (partyUsed.ContainsKey(pid))
                {
                    partyUsed.Remove(pid);
                }
            });
        }

        static bool CanPartyUse(int min, int max, int now)
        {
            return now >= min && now <= max;
        }
        #endregion

        public static void Init()
        {
            SetEntiesActive();
            CallFiberSchedule();
        }

        #region Update Entity Active Per 30 Minutes
        static void CallFiberSchedule()
        {
            executionFiber.ScheduleOnInterval(SetEntiesActive, GetDurationTime(), 1800000);
        }

        private static long GetDurationTime()
        {
            DateTime now = DateTime.Now;
            int minUnit = (int)Math.Ceiling((decimal)now.Minute / (decimal)30);
            int nxMin = minUnit * 30;
            if (nxMin == 60)
            {
                nxMin = 0;
            }

            int nxHour = (minUnit * 30 == 60) ? now.Hour + 1 : now.Hour;
            if(nxHour == 24)
            {
                nxHour = 0;
                now.AddDays(1);
            }

            DateTime nextTime = new DateTime(now.Year, now.Month, now.Day, nxHour, nxMin, 0);
            long returnTime = (long)(nextTime - DateTime.Now).TotalMilliseconds;
            if(returnTime <= 0)
            {
                returnTime += 1800000;
            }
            return returnTime;
        }

        public static void SetEntiesActive()
        {
            for (int i = 0; i < interactiveEntity.Count; ++i)
            {
                InteractiveGate mEntity = interactiveEntity[i];
                bool compare = CanActiveGameObject(mEntity);
                mEntity.mPropertyInfos.activeObject = compare;
                GameApplication.BroadcastInteractiveCount(mEntity.GetPersistentID(),
                    mEntity.mPropertyInfos.canTrigger, compare, (int)mEntity.step);
            }
        }

        public static bool CanActiveGameObject(InteractiveGate mEntity)
        {
            ScenesModelJson sceneModel = ScenesModelRepo.GetScenesModelJson(mEntity.entityName);
            if (sceneModel == null)
                return true;

            if (IsOpenTime(sceneModel.npcopentime, sceneModel.npcclosetime) && IsCycleTime(sceneModel.npccycletime))
                return true;

            return false;
        }

        public static bool IsCycleTime(string cycleTime)
        {
            if(cycleTime == "")
            {
                return true;
            }
            int number = 0;
            bool isNumber = false;
            List<string> nowTime = cycleTime.Split(';').ToList();
            isNumber = int.TryParse(nowTime[0], out number);

            if (isNumber)
            {
                return CheckDate(nowTime);
            }
            else
            {
                return CheckWeek(nowTime[0]);
            }
        }

        static bool CheckDate(List<string> nowTime)
        {
            int month = DateTime.Now.Month;
            int date = DateTime.Now.Day;
            for (int i = 0; i < nowTime.Count; ++i)
            {
                if (int.Parse(nowTime[i].Substring(0, 2)) == month && int.Parse(nowTime[i].Substring(2, 2)) == date)
                    return true;
            }

            return false;
        }

        static bool CheckWeek(string weekString)
        {
            char[] checkWeek = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G' };
            char[] week = weekString.ToCharArray();
            List<int> weekToInt = new List<int>();
            int wkCount = 0;
            for (int wk = 0; wk < checkWeek.Length; ++wk)
            {
                if (week[wkCount] == checkWeek[wk])
                {
                    weekToInt.Add(wk + 1);
                }
            }

            int intWeek = (int)DateTime.Now.DayOfWeek;
            for (int i = 0; i < weekToInt.Count; ++i)
            {
                if (weekToInt[i] == intWeek)
                    return true;
            }
            return false;
        }

        public static bool IsOpenTime(string openTime, string closeTime)
        {
            if(openTime == "" && closeTime == "")
            {
                return true;
            }
            DateTime now = DateTime.Now;
            List<string> openList = openTime.Split(';').ToList();
            List<string> closeList = closeTime.Split(';').ToList();

            for (int i = 0; i < openList.Count; ++i)
            {
                if (now > DateTime.ParseExact(openList[i], "HHmm", null)
                    && now <= DateTime.ParseExact(closeList[i], "HHmm", null))
                    return true;
            }

            return false;
        }
        #endregion
    }
}
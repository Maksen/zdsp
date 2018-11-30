using System;
using System.Linq;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;

using Kopio.JsonContracts;
using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;

namespace Zealot.Server.Rules
{
    public class InteractiveTriggerRule
    {
        private static readonly PoolFiber executionFiber;
        private static Dictionary<int, List<string>> partyUsed = new Dictionary<int, List<string>>();
        private static Dictionary<int, string> partyProgresser = new Dictionary<int, string>();
        private static List<InteractiveGate> allEntity = new List<InteractiveGate>();

        static InteractiveTriggerRule()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        #region InteractiveTriggerEvent
        public static void UseInteractiveTrigger(int pid, bool isArea, string name, InteractiveGate mEntity, Player peer)
        {
            executionFiber.Enqueue(() =>
            {
                int keyId = mEntity.mPropertyInfos.keyId;
                if (keyId != 0)
                {
                    if (!peer.Slot.mInventory.HasItem((ushort)keyId, 1))
                    {
                        return;
                    }
                }

                if (isArea)
                {
                    if (!partyUsed.ContainsKey(pid))
                    {
                        partyUsed.Add(pid, new List<string>());
                    }

                    if(!partyUsed[pid].Exists(x => x == name))
                    {
                        partyUsed[pid].Add(name);
                    }

                    if (CanPartyUse(pid, mEntity))
                    {
                        if (mEntity.step != InteractiveTriggerStep.OnProgress)
                        {
                            mEntity.SetEntityStep((int)InteractiveTriggerStep.OnProgress, name);
                            if (!partyProgresser.ContainsKey(pid))
                                partyProgresser.Add(pid, name);
                            else
                                partyProgresser[pid] = name;
                        }
                    }
                    else
                    {
                        mEntity.SetEntityStep((int)InteractiveTriggerStep.OnTrigger);
                    }
                }
                else
                {
                    if (!mEntity.isUsing && mEntity.GetEntityCanUse())
                    {
                        mEntity.SetEntityStep((int)InteractiveTriggerStep.OnProgress, name);
                        mEntity.isUsing = true;
                    }
                }
            });
        }

        public static void LeaveInteractiveTrigger(int pid, bool isArea, string name, InteractiveGate mEntity)
        {
            executionFiber.Enqueue(() =>
            {
                if (isArea)
                {
                    if (partyUsed[pid].Exists(x => x == name))
                    {
                        partyUsed[pid].Remove(name);
                    }

                    if(partyProgresser.Count == 0)
                    {
                        return;
                    }

                    bool changeProgressPlayer = false;
                    if(!string.IsNullOrEmpty(partyProgresser[pid]))
                    {
                        changeProgressPlayer = name == partyProgresser[pid];
                    }
                    if (changeProgressPlayer && partyUsed[pid].Count > 0)
                    {
                        partyProgresser[pid] = partyUsed[pid][partyUsed.Count - 1];
                    }

                    if (CanPartyUse(pid, mEntity))
                    {
                        mEntity.SetEntityStep((int)InteractiveTriggerStep.OnProgress, partyProgresser[pid]);
                    }
                    else
                    {
                        mEntity.SetEntityStep((int)InteractiveTriggerStep.None);
                    }
                }
                else
                {
                    mEntity.SetEntityStep((int)InteractiveTriggerStep.None);
                    mEntity.isUsing = false;
                }
            });
        }

        public static void CompeletedInteradtiveTrigger(int pid, bool isArea, InteractiveGate mEntity)
        {
            executionFiber.Enqueue(() =>
            {
                if (isArea)
                {
                    partyProgresser.Remove(pid);
                }
                else
                {
                    mEntity.isUsing = false;
                }
            });
        }
        
        private static bool CanPartyUse(int pid, InteractiveGate mEntity)
        {
            return partyUsed[pid].Count >= mEntity.mPropertyInfos.min && partyUsed[pid].Count <= mEntity.mPropertyInfos.max;
        }

        static bool CanPartyUse(int min, int max, int now)
        {
            return now >= min && now <= max;
        }
        #endregion
        
        #region Update Entity Active
        public static void Init()
        {
            SetEntiesActive();
            StartRepeatRefresh();
        }

        static void StartRepeatRefresh()
        {
            executionFiber.ScheduleOnInterval(SetEntiesActive, GetDurationTime(), 1800000);
        }

        private static long GetDurationTime()
        {
            DateTime now = DateTime.Now;
            int minUnit = (int)Math.Ceiling((decimal)now.Minute / (decimal)30);
            int nxMin = minUnit * 30;
            int nxHour = now.Hour;
            if (nxMin == 60)
            {
                nxMin = 0;
                ++nxHour;
            }

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
            for (int i = 0; i < allEntity.Count; ++i)
            {
                if (CanActiveGameObject(allEntity[i]))
                {
                    allEntity[i].SetEntityStep((allEntity[i].count != 0) ?
                        (int)InteractiveTriggerStep.None : (int)InteractiveTriggerStep.CannotUse);
                }
                else
                {
                    allEntity[i].SetEntityStep((int)InteractiveTriggerStep.InActive);
                }
            }
        }

        public static void AddEntityList(InteractiveGate mEntity)
        {
            allEntity.Add(mEntity);
        }

        public static bool CanActiveGameObject(InteractiveGate mEntity)
        {
            ScenesModelJson sceneModel = ScenesModelRepo.GetScenesModelJson(mEntity.entityName);
            if (sceneModel == null)
            {
                return mEntity.GetEntityActive();
            }
            
            if (!sceneModel.activeonstartup)
            {
                return false;
            }

            if (IsOpenTime(sceneModel.npcopentime, sceneModel.npcclosetime) && IsCycleTime(sceneModel.npccycletime))
            {
                return true;
            }

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
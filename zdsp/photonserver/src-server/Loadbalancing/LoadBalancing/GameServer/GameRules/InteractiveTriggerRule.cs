using System.Collections.Generic;
using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;

namespace Zealot.Server.Rules
{
    class InteractiveTriggerRule
    {
        private static readonly PoolFiber executionFiber;
        private static Dictionary<int, List<string>> partyUsed = new Dictionary<int, List<string>>();

        static InteractiveTriggerRule()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void UseInteractiveTrigger(int triggerId, string playerName, int min, int max, 
            Zealot.Server.Entities.NetEntity myEntity, int time, int count, bool isArea)
        {
            executionFiber.Enqueue(() =>
            {
                if (!partyUsed.ContainsKey(triggerId))
                {
                    partyUsed.Add(triggerId, new List<string>() { playerName });
                } else
                {
                    if(!partyUsed[triggerId].Exists(x => x == playerName))
                    partyUsed[triggerId].Add(playerName);
                }

                ComparePlayer(min, max, myEntity, triggerId, time, count, isArea);
            });
        }

        static void ComparePlayer(int min, int max, Zealot.Server.Entities.NetEntity myEntity, int pid, int time, int count, bool isArea)
        {
            int playerCount = partyUsed.Count;
            if (playerCount >= min)
            {
                if (playerCount <= max)
                {
                    Zealot.Common.Actions.InteractiveTriggerCommand cmd = new Zealot.Common.Actions.InteractiveTriggerCommand();
                    cmd.entityId = pid;
                    cmd.triggerTime = time;
                    cmd.count = count;
                    cmd.isArea = isArea;
                    myEntity.SetAction(cmd);
                }
                else
                {
                    myEntity.StopAction();
                }
            }
            else
            {
                myEntity.StopAction();
            }
        }

        public static string GetCurrentPlayer()
        {
            string playerName = string.Empty;
            return playerName;
        }

        public static void InterruptedEvent(int triggerId, string playerName)
        {
            executionFiber.Enqueue(() =>
            {
                if (partyUsed.ContainsKey(triggerId))
                {
                    partyUsed[triggerId].Remove(playerName);
                    if (partyUsed[triggerId].Count == 0)
                    {
                        partyUsed.Remove(triggerId);
                    }
                }
            });
        }

        public static void CompeletedEvent(int triggerId)
        {
            executionFiber.Enqueue(() =>
            {
                if (partyUsed.ContainsKey(triggerId))
                {
                    partyUsed.Remove(triggerId);
                }
            });
        }
    }
}

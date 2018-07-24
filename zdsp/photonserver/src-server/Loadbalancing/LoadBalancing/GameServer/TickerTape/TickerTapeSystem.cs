using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zealot.Common;

namespace Photon.LoadBalancing.GameServer
{
    public static class TickerTapeSystem
    {
        private static DateTime lastQueryTime;
        private static readonly long IntervalMs = 300000; //every 5 minutes
        private static Dictionary<int, GMMessageData> messageMap = new Dictionary<int, GMMessageData>();
        private static string serializedString = "";

        public static void Init()
        {
            lastQueryTime = DateTime.Now;
            List<GMMessageData> messageList = GameApplication.dbGM.TickerTape.GetAllMessage().Result.Select(e => new GMMessageData(e)).Where(e => IsSameServer(e) && e.IsExpired() == false).ToList();
            messageMap = messageList.ToDictionary(e => e.id, e => e);
            UpdateSerializedString();
            GameApplication.Instance.executionFiber.ScheduleOnInterval(Refresh, IntervalMs, IntervalMs);
        }

        public static void OnLogin(GameClientPeer peer)
        {
            if (messageMap.Count == 0)
                return;
            peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.GMMessageChanged, serializedString, peer);
        }

        private static bool IsSameServer(GMMessageData data)
        {
            return data.server == "all" || data.server.Split(';').Where(e => GameApplication.Instance.GetMyServerId().ToString() == e).Count() > 0;
        }

        private static void UpdateSerializedString()
        {
            serializedString = JsonConvertDefaultSetting.SerializeObject(messageMap.Values.ToList());
        }

        private static void Refresh()
        {
            List<int> expiredIds = new List<int>();
            foreach(var kvp in messageMap)
            {
                if (kvp.Value.IsExpired())
                    expiredIds.Add(kvp.Key);
            }
            if (expiredIds.Count > 0)
            {
                for (int index = 0; index < expiredIds.Count; index++)
                    messageMap.Remove(expiredIds[index]);
                UpdateSerializedString();
            }

            var task = QueryNewTask();
        }

        private static async Task QueryNewTask()
        {
            var result = await GameApplication.dbGM.TickerTape.GetNewMessages(lastQueryTime);
            if (result.Count == 0)
                return;

            List<GMMessageData> newList = result.Select(e => new GMMessageData(e)).ToList();
            GameApplication.Instance.executionFiber.Enqueue(() =>
            {
                BroadcastMessage(newList);
            });
        }

        private static void BroadcastMessage(List<GMMessageData> newList)
        {
            lastQueryTime = DateTime.Now;
            bool hasChange = false;
            for (int index = 0; index < newList.Count; index++)
            {
                GMMessageData data = newList[index];
                int id = data.id;
                if (messageMap.ContainsKey(id))
                {
                    if (IsSameServer(data) && !data.IsExpired())
                        messageMap[id] = data;
                    else
                        messageMap.Remove(id);
                    hasChange = true;
                }
                else
                {
                    if (IsSameServer(data) && !data.IsExpired())
                    {
                        messageMap.Add(id, data);
                        hasChange = true;
                    }
                }
            }

            if (hasChange)
            {
                UpdateSerializedString();
                GameApplication.Instance.BroadcastMessage(BroadcastMessageType.GMMessageChanged, serializedString);
            }
        }
    }
}
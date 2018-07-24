using System;
using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;

namespace Zealot.Server.Rules
{
    public static class BossRules
    {
        public static Dictionary<int, SpecialBossStatus> SpecialBossStatusMap;
        public static string SpecialBossStatusString; //contain boss alive information
        public static bool IsStatusDirty = true;

        public static void Init()
        {
            SpecialBossStatusMap = new Dictionary<int, SpecialBossStatus>();
            SpecialBossStatusString = "";

            int serverId = GameApplication.Instance.GetMyServerId();
            List<Dictionary<string, object>> bossKillerRecords = GameApplication.dbRepository.BossKiller.GetRecords(serverId);

            var boss_idmap = SpecialBossRepo.mIdMap;
            DateTime now = DateTime.Now;
            foreach (var entry in bossKillerRecords)
            {
                int id = (int)entry["bossid"];
                SpecialBossJson _specialBossJson;
                if (!boss_idmap.TryGetValue(id, out _specialBossJson))
                    continue;
                if (_specialBossJson.spawntype == BossSpawnType.SpawnDuration)
                {
                    if (now < DateTime.ParseExact(_specialBossJson.spawnstart, "yyyy/MM/dd/HH:mm", null)
                        || now.AddSeconds(600) < DateTime.ParseExact(_specialBossJson.spawnend, "yyyy/MM/dd/HH:mm", null))
                        continue; //activity not started or is going to end, doesn't need to show boss inforamtion.
                }
                SpecialBossStatusMap.Add(id, new SpecialBossStatus(id, false, (string)entry["killer"], (string)entry["payload"]));
            }
            IsStatusDirty = true;
        }

        public static void OnSpecialBossSpawn(int id)
        {
            SpecialBossStatus _status;
            if (!SpecialBossStatusMap.TryGetValue(id, out _status))
            {
                _status = new SpecialBossStatus(id, false, "", "");
                SpecialBossStatusMap[id] = _status;
            }
            _status.isAlive = true;
            _status.nextSpawn = null;
            IsStatusDirty = true;
        }

        public static void OnSpecialBossKilled(int id, string killer, string payload)
        {
            SpecialBossStatus _status = SpecialBossStatusMap[id];
            _status.isAlive = false;
            _status.lastKiller = killer;
            _status.payload = payload;
            IsStatusDirty = true;

            int serverId = GameApplication.Instance.GetMyServerId();
            var saved = GameApplication.dbRepository.BossKiller.Insert_Update(serverId, id, killer, payload);   
        }

        public static void SetBossNextSpawn(int id, DateTime? nextSpawn)
        {
            SpecialBossStatus _status;
            if (!SpecialBossStatusMap.TryGetValue(id, out _status))
            {
                _status = new SpecialBossStatus(id, false, "", "");
                SpecialBossStatusMap[id] = _status;
            }
            _status.isAlive = false;
            _status.nextSpawn = nextSpawn;
            IsStatusDirty = true;
        }

        public static void AddBossStatus(int id)
        {
            if (!SpecialBossStatusMap.ContainsKey(id))
            {
                SpecialBossStatusMap.Add(id, new SpecialBossStatus(id, false, "", ""));
                IsStatusDirty = true;
            }
        }

        public static void UpdateSpecialBossStatusString()
        {        
            SpecialBossStatusString = "";
            foreach (KeyValuePair<int, SpecialBossStatus> entry in SpecialBossStatusMap)
                SpecialBossStatusString += entry.Value.ToString() + ";";
            IsStatusDirty = false;
        }

        public static string GetSpecialBossStatusString()
        {
            if (IsStatusDirty)
                UpdateSpecialBossStatusString();
            return SpecialBossStatusString;
        }

        public static string GetBossDmgList(int id)
        {
            SpecialBossStatus status;
            if (SpecialBossStatusMap.TryGetValue(id, out status))
                return status.payload;
            return "";
        }
    }
}
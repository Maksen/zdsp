using Kopio.JsonContracts;
using System;
using UnityEngine;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Zealot.Repository;
using Zealot.Server.AI;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    public class SpecialBossSpawner : MonsterSpawnerBase
    {
        public SpecialBossSpawnerJson mSpecialBossSpawnerJson;
        public SpecialBossJson mSpecialBossInfo;
        private PositionHelperData mPositionHelperData;
        private int mRandomIndex = -1;

        public SpecialBossSpawner(SpecialBossSpawnerJson info, GameLogic instance)
            : base(info, instance)
        {
            mSpecialBossSpawnerJson = info;
            if (info.archetype != "")
            {
                mSpecialBossInfo = SpecialBossRepo.GetInfoByName(info.archetype);
                if (mSpecialBossInfo != null)
                    mArchetype = CombatNPCRepo.GetNPCById(mSpecialBossInfo.archetypeid);
            }
        }

        public override int GetPopulation()
        {
            return 1;
        }

        public override void InstanceStartUp()
        {
            if (mArchetype == null)
                return;
            SetNextSpawnTimer(true);
        }

        public override void SpawnAllMonster()
        {
            DateTime now = DateTime.Now;
            if (mArchetype == null || maChildren.Count >= 1)
                return;
            SpawnMonster();
            mInstance.BroadcastEvent(this, "OnChildrenSpawn");
            if (mSpecialBossInfo.spawntype != BossSpawnType.Event)
                BossRules.OnSpecialBossSpawn(mSpecialBossInfo.id);
            GameApplication.Instance.BroadcastMessage(BroadcastMessageType.BossSpawn, mSpecialBossInfo.id.ToString());
        }

        public override void SpawnMonster()
        {
            //Spawn monster at server
            Monster monster = mInstance.mEntitySystem.SpawnNetEntity<Monster>(true, mArchetype.archetype);
            NPCSynStats playerStats = new NPCSynStats();            
            monster.PlayerStats = playerStats;

            RandomPosition();
            monster.Position = mPositionHelperData.position;
            monster.Forward = mPropertyInfos.forward;
            monster.Init(this);
            monster.SetAIBehaviour(new MonsterAIBehaviour(monster));
            maChildren.Add(monster);
        }

        public void RandomPosition()
        {
            int _count = mSpecialBossSpawnerJson.positionhelper.Count;
            int _randomIndex = 0;
            if (_count > 1)
            {
                do
                {
                    _randomIndex = GameUtils.RandomInt(0, _count - 1);
                }
                while (mRandomIndex == _randomIndex); //prevent random same position
            }
            mPositionHelperData = mSpecialBossSpawnerJson.positionhelper[_randomIndex];
            mRandomIndex = _randomIndex;
        }

        public override void OnChildDead(Monster child, IActor attacker)
        {
            base.OnChildDead(child, attacker);
            object[] paramters = { attacker };
            mInstance.BroadcastEvent(this, "OnChildDead", paramters);
            SetNextSpawnTimer(false);

            if (mSpecialBossInfo.spawntype != BossSpawnType.Event)
            {
                BossKillData _bossKillData = new BossKillData();
                string _killer = "";
                if (mSpecialBossInfo.bosstype == BossType.BigBoss)
                {
                    string _name = "";
                    if (child.mPartyScoreRank.Count > 0)
                        _killer = child.mPartyScoreRank[0].Key + "-" + child.mPartyScoreRank[0].Value;
                    foreach (var element in child.mPartyScoreRank)
                    {
                        _name = element.Key;
                        BossKillScoreRecord _record = new BossKillScoreRecord();
                        _bossKillData.scoreRecords.Add(_record);
                        _record.Name.Add(_name);
                        _record.Score = element.Value;
                        PartyStatsServer _party = PartyRules.GetMyParty(_name);
                        if (_party != null)
                        {
                            foreach(string _member in _party.GetPartyMemberList().Keys)
                            {
                                if (_member != _name)
                                    _record.Name.Add(_member);
                            }
                        }
                    }
                }
                else
                {
                    if (child.mPlayerDamageRank.Count > 0)
                        _killer = child.mPlayerDamageRank[0].Key + "-" + child.mPlayerDamageRank[0].Value;
                    foreach (var kvp in child.mPlayerDamageRank)
                    {
                        BossKillDmgRecord _record = new BossKillDmgRecord();
                        _bossKillData.dmgRecords.Add(_record);
                        _record.Name = kvp.Key;
                        _record.Score = kvp.Value;
                    }
                }
                _bossKillData.bossId = mSpecialBossInfo.id;
                BossRules.OnSpecialBossKilled(mSpecialBossInfo.id, _killer, _bossKillData.SerializeForDB());
            }

            if (attacker != null)
            {
                string attackerName = "";
                if (mSpecialBossInfo.bosstype == BossType.BigBoss)
                {
                    if (child.mPartyScoreRank.Count > 0)
                        attackerName = child.mPartyScoreRank[0].Key;
                }
                else
                {
                    if (child.mPlayerDamageRank.Count > 0)
                        attackerName = child.mPlayerDamageRank[0].Key;
                }
                if (!string.IsNullOrEmpty(attackerName))
                {
                    string paramStr = string.Format("{0};{1}", mSpecialBossInfo.id, attackerName);
                    GameApplication.Instance.BroadcastMessage(BroadcastMessageType.BossKilled, paramStr);
                }
            }
        }

        private void SetNextSpawnTimer(bool startup)
        {
            DateTime now = DateTime.Now;
            bool foundNext = false;
            long timetoNextSpawn = 0;
            switch(mSpecialBossInfo.spawntype)
            {
                case BossSpawnType.SpawnDaily:
                    if (mSpecialBossInfo.spawndaily != "")
                    {
                        timetoNextSpawn = GameUtils.TimeToNextEventDailyFormat(now, mSpecialBossInfo.spawndaily, out foundNext, 5000);
                        if (foundNext)
                            BossRules.AddBossStatus(mSpecialBossInfo.id);
                    }
                    break;
                case BossSpawnType.SpawnWeekly:
                    if (mSpecialBossInfo.spawnweekly != "")
                    {
                        timetoNextSpawn = GameUtils.TimeToNextEventWeeklyFormat(now, mSpecialBossInfo.spawnweekly, out foundNext, 5000);
                        if (foundNext)
                            BossRules.AddBossStatus(mSpecialBossInfo.id);
                    }
                    break;
                case BossSpawnType.SpawnDuration:
                    int _spawnDelay = 0;             
                    if (startup)
                    {
                        DateTime _spawnstart = DateTime.ParseExact(mSpecialBossInfo.spawnstart, "yyyy/MM/dd/HH:mm", null);
                        if (now < _spawnstart)
                        {
                            foundNext = true;
                            timetoNextSpawn = (long)(_spawnstart - now).TotalMilliseconds + 600000;
                        }
                        else
                            _spawnDelay = 600;
                    }
                    else
                        _spawnDelay = mSpecialBossInfo.respawntime;
                    if (_spawnDelay > 0)
                    {
                        DateTime _spawnend = DateTime.ParseExact(mSpecialBossInfo.spawnend, "yyyy/MM/dd/HH:mm", null);
                        DateTime _nextSpawn = now.AddSeconds(_spawnDelay);
                        if (_nextSpawn < _spawnend)
                        {
                            foundNext = true;
                            timetoNextSpawn = _spawnDelay * 1000;
                            BossRules.SetBossNextSpawn(mSpecialBossInfo.id, _nextSpawn);
                        }
                    }
                    break;
                case BossSpawnType.Event:
                    return; //event boss doesn't respawn
            }

            if (foundNext)
                timer = mInstance.SetTimer(timetoNextSpawn, (arg) => {
                    timer = null;
                    SpawnAllMonster();
                }, null);
        }

        public override Vector3 GetPos()
        {
            return mPositionHelperData.position;
        }

        public override bool CanRoam()
        {
            return mSpecialBossSpawnerJson.canroam;
        }

        public override bool CanPathFind()
        {
            return mSpecialBossSpawnerJson.canpathfind;
        }

        public override bool IsAggressive()
        {
            return mSpecialBossSpawnerJson.aggressive;
        }

        public override float GetCombatRadius()
        {
            return mPositionHelperData.combatRadius;
        }

        public override float GetAggroRadius()
        {
            return mPositionHelperData.aggroRadius;
        }
    }
}

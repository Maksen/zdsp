
namespace Zealot.Server.Entities
{
    using Kopio.JsonContracts;
    using UnityEngine;
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using Zealot.Common;
    using Zealot.Common.RPC;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Actions;
    using Photon.LoadBalancing.GameServer;
    using Zealot.Server.AI;
    using Rules;
    using Repository;
    using EventMessage;

    public class BigBossScoreRecord
    {
        public int score;
        public uint tick; //4800 ticks to clear this record;
    }

    public class Monster : Actor
    {
        private long elapsedDT;
        private long regenDT;
        public MonsterSpawnerBase mSp;
        public CombatNPCJson mArchetype;        
        private Vector3 mSpawnPos;
        private GameTimer livetimer;
        private GameTimer deadtimer;
        protected BaseAIBehaviour mAIController;
        private bool mIsBigBossLoot;
        private bool mIsBoss;
        private long mBossNoDmgCountdown = 0;
        private long mBossNoDmgCountdownConst = 0;

        private Dictionary<string, int> mPlayerDamages; //Track damages caused by players
        public List<KeyValuePair<string, int>> mPlayerDamageRank; //player damage rank for boss
        private Dictionary<string, BigBossScoreRecord> mPlayerScore; //Track players score for bigboss
        public List<KeyValuePair<string, long>> mPartyScoreRank; //party score rank for bigboss, key is leader name or player self.
        private uint mOnAttackedTick = 0;

        public bool LogAI { get
            {
                return true;
                //bool logflag = mArchetype.monsterclass == MonsterClass.Boss;
                //if (mSp != null)
                //    return mSp.LogAI && logflag;
                //else
                //    return false;
            }
        }
        public Monster() : base()
        {
            this.EntityType = EntityType.Monster;           
            elapsedDT = 0;
            mPlayerDamages = new Dictionary<string, int>();
            mPlayerDamageRank = new List<KeyValuePair<string, int>>();
            mPlayerScore = new Dictionary<string, BigBossScoreRecord>();
            mPartyScoreRank = new List<KeyValuePair<string, long>>();
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {            
            peer.ZRPC.CombatRPC.SpawnMonsterEntity(mnPersistentID, mArchetype.id, Position.ToRPCPosition(), Forward.ToRPCDirection(), GetHealth(), peer);
        }
        #endregion

        public bool HasEvasion()
        {
            //normalmonster which can be knockback will not dodge.
            return !(/*mArchetype.canbeknockback && */mArchetype.monsterclass == MonsterClass.Normal);
        }

        public override void Update(long dt)
        {            
            base.Update(dt);

            elapsedDT += dt;
            if (!bAIDisabled && elapsedDT >= 500)
            {
                mAIController.OnUpdate(elapsedDT);                        
                elapsedDT = 0;                
            }

            RegenHealth(dt);

            if (mIsBoss)
            {
                mBossNoDmgCountdown -= dt;
                if (mBossNoDmgCountdown < 0)
                {
                    mBossNoDmgCountdown = mBossNoDmgCountdownConst;
                    ((SpecialBossSpawner)mSp).RandomPosition();
                    Position = mSp.GetPos();
                    mAIController.GotoState("Goback");
                }
            }
        }

        protected bool bAIDisabled = false;
        public void StopAI()
        {
            bAIDisabled = true;
        }

        public delegate bool OverwriteStatsCallback();
        public void Init(MonsterSpawnerBase spawner, OverwriteStatsCallback callback = null, long liveDuration=0)
        {
            mSp = spawner;
            mArchetype = spawner.mArchetype;
            SetInstance(spawner.mInstance);
            this.Name = mArchetype.localizedname;
            var sp = spawner.mPropertyInfos;            
            PlayerStats.Alive = true;
            PlayerStats.Team = -100; //default not same as pc
            PlayerStats.MoveSpeed = mArchetype.movespeed;
            PlayerStats.Level = mArchetype.level;
            mSpawnPos = sp.position;

            PlayerCombatStats monstercombatstats = new PlayerCombatStats();
            monstercombatstats.SetPlayerLocalAndSyncStats(null, PlayerStats, this);
            
            CombatStats = monstercombatstats;
            SkillPassiveStats = new SkillPassiveCombatStats(EntitySystem.Timers, this);

            bool overwriteStats = false;
            if (callback != null)
                overwriteStats = callback();
            if (!overwriteStats)
            {
                monstercombatstats.SuppressComputeAll = true;
                CombatStats.SetField(FieldName.AttackBase, mArchetype.attack);
                CombatStats.SetField(FieldName.ArmorBase, mArchetype.armor);
                CombatStats.SetField(FieldName.AccuracyBase, mArchetype.accuracy);
                CombatStats.SetField(FieldName.EvasionBase, mArchetype.evasion);
                // Monster stats needs to be changed
                CombatStats.SetField(FieldName.PierceDamage, 50);
                CombatStats.SetField(FieldName.SliceDamage, 10);
                CombatStats.SetField(FieldName.SmashDamage, 10);
                CombatStats.SetField(FieldName.PierceDefense, 20);
                CombatStats.SetField(FieldName.SliceDefense, 20);
                CombatStats.SetField(FieldName.SmashDefense, 20);
                CombatStats.SetField(FieldName.MetalDefense, 20);
                CombatStats.SetField(FieldName.WoodDefense, 20);
                CombatStats.SetField(FieldName.EarthDefense, 20);
                CombatStats.SetField(FieldName.WaterDefense, 20);
                CombatStats.SetField(FieldName.FireDefense, 20);
                CombatStats.SetField(FieldName.IgnoreArmorBase, 10);
                CombatStats.SetField(FieldName.WeaponAttackBase, 10);
                CombatStats.SetField(FieldName.StrengthBonus, 12);
                CombatStats.SetField(FieldName.IntelligenceBase, 80);
                //CombatStats.SetField(FieldName.VSNullDamage, 100);
                CombatStats.SetField(FieldName.VSHumanDefenseBonus, 10);
                CombatStats.SetField(FieldName.DecreaseFinalDamage, 10);
                CombatStats.SetField(FieldName.BlockRate, 30);
                CombatStats.SetField(FieldName.BlockValueBonus, 80);
                //CombatStats.SetField(FieldName.CriticalDamageBase, mArchetype.criticaldamage);
                //CombatStats.SetField(FieldName.CocriticalBase, mArchetype.cocritical);
                //CombatStats.SetField(FieldName.CriticalBase, mArchetype.critical);
                //CombatStats.SetField(FieldName.CoCriticalDamageBase, mArchetype.cocriticaldamage);
                //CombatStats.SetField(FieldName.TalentPointCloth, mArchetype.talentcloth);
                //CombatStats.SetField(FieldName.TalentPointScissors, mArchetype.talentscissors);
                //CombatStats.SetField(FieldName.TalentPointStone, mArchetype.talentstone);
                SetHealthMax(mArchetype.healthmax); // Init max health first
                SetHealth(mArchetype.healthmax); // Health now uses combatstats
                monstercombatstats.SuppressComputeAll = false;
                monstercombatstats.ComputeAll();//TODO:check the above stats is initialized properly.
            }
            Idle();
            if (liveDuration > 0)
                livetimer = mInstance.SetTimer(liveDuration, OnLiveTimeUp, null);
            SpecialBossSpawner bossSpawner = spawner as SpecialBossSpawner;
            if (bossSpawner != null)
            {
                mIsBoss = true;
                mIsBigBossLoot = bossSpawner.mSpecialBossInfo.bosstype == BossType.BigBoss;
                mBossNoDmgCountdownConst = SpecialBossRepo.BossNoDmgRandomPos * 1000;
                mBossNoDmgCountdown = mBossNoDmgCountdownConst;
            }
        }

        public override float GetExDamage()
        {
            //return mArchetype.exdamage;
            return 0;
        }

        public override void SetHealth(int val)
        {
            base.SetHealth(val);
            float newhp = (float)val / GetHealthMax();            
            PlayerStats.DisplayHp = newhp;
        }

        private void RegenHealth(long dt)
        {
            int healthMax = GetHealthMax();
            if (mArchetype.hpregenamt > 0 && IsAlive() && GetHealth() < healthMax)
            {
                regenDT += dt;
                if (regenDT > mArchetype.healthregeninterval * 1000)
                {
                    int regenAmt = (int)(mArchetype.hpregenamtbypercent / 100.0f * healthMax) + mArchetype.hpregenamt;
                    OnRecoverHealth(regenAmt);
                    regenDT = 0;
                }
            }
            else
                regenDT = 0;
        }

        public void SetAIBehaviour(BaseAIBehaviour behaviour)
        {
            mAIController = behaviour;
            mAIController.StartMonitoring();
        }

        public bool IsAggressive()
        {
            return mSp.IsAggressive();         
        }

        public override bool IsInvalidTarget()
        {
            return !IsAlive();
        }

        public override bool IsInSafeZone()
        {
            return false;
        }
        
        public Actor QueryForThreat()
        {
            float aggroRadius = mSp.GetAggroRadius();
            if (aggroRadius == 0)
                return null;
            if (!string.IsNullOrWhiteSpace(mSummoner))
            {
                var peer = GameApplication.Instance.GetCharPeer(mSummoner);
                if (peer != null)
                {
                    var summoner = peer.mPlayer;
                    if (summoner != null && summoner.mInstance == mInstance && Vector3.SqrMagnitude(summoner.Position - Position) <= aggroRadius * aggroRadius)
                    return summoner;
                }
            }
            else
                return EntitySystem.QueryForClosestEntityInSphere(this.Position, aggroRadius, (queriedEntity) =>
                {                        
                        IActor target = queriedEntity as IActor;
                        return (target != null && CombatUtils.IsValidEnemyTarget(this, target));
                }) as Actor;
            return null;
        }

        private void OnLiveTimeUp(object arg)
        {
            livetimer = null;
            mSp.OnChildDead(this, null);
            mSp = null;
            CleanUp();
        }

        private void OnDeadTimeUp(object arg)
        {
            deadtimer = null;
            CleanUp();
        }

        public void CleanUp()
        {
            if (deadtimer != null)
            {
                mInstance.StopTimer(deadtimer);
                deadtimer = null;
            }
            if (livetimer != null)
            {
                mInstance.StopTimer(livetimer);
                livetimer = null;
            }
            mInstance.mEntitySystem.RemoveAlwaysShow(this);
            mInstance.mEntitySystem.RemoveEntityByPID(GetPersistentID(), mArchetype.monsterclass == MonsterClass.Boss);                         
        }

        public override void OnKilled(IActor attacker)
        {          
            base.OnKilled(attacker);
            mAIController.OnKilled();
            HandleBossOnKilled();
    
            PerformAction(new ServerAuthoASDead(this, new DeadActionCommand()));
            deadtimer = mInstance.SetTimer(CombatUtils.DYING_TIME, OnDeadTimeUp, null);//give  seconds for client
            NetEntity ne = (NetEntity)attacker;
            Player killer = null;                
            if (ne.IsPlayer())
                killer = attacker as Player;                   
            else if (ne.IsHero())
                killer = (attacker as HeroEntity).Owner;  //set the hero's owner as the killer
            HandleLoot(killer);
            mSp.OnChildDead(this, killer);
            mSp = null;
        }

        #region DamageRecord        
        public void ResetDamageRecords()
        {
            mPlayerDamages.Clear();
            mPlayerScore.Clear();
        }

        private void AddDamageRecord(string playerName, int damage)
        {
            if (mIsBigBossLoot)
            {
                uint ticknow = EntitySystem.Timers.GetTick();
                BigBossScoreRecord record;
                if (mPlayerScore.TryGetValue(playerName, out record))
                {
                    if (ticknow - record.tick > 4800)
                        record.score = damage;
                    else
                        record.score += damage;
                }
                else
                {
                    record = new BigBossScoreRecord { score = damage };
                    mPlayerScore.Add(playerName, record);
                }
                record.tick = ticknow;
            }
            if (mPlayerDamages.ContainsKey(playerName))
                mPlayerDamages[playerName] += damage;
            else
                mPlayerDamages.Add(playerName, damage);
        }

        public void AddDamageToPlayer(string playerName, int damage)
        {
            if (mIsBigBossLoot)
            {
                int score = damage / 2;
                uint ticknow = EntitySystem.Timers.GetTick();
                BigBossScoreRecord record;
                if (mPlayerScore.TryGetValue(playerName, out record))
                {
                    if (ticknow - record.tick > 4800)
                        record.score = score;
                    else
                        record.score += score;
                }
                else
                {
                    record = new BigBossScoreRecord { score = score };
                    mPlayerScore.Add(playerName, record);
                }
                record.tick = ticknow;
            }
        }

        private void HandleBossOnKilled()
        {
            if (!mIsBoss)
                return;

            var _peers = GameApplication.Instance.GetAllCharPeer();
            string _bossinfo = mArchetype.id + ";";
            if (mIsBigBossLoot)
            {
                uint ticknow = EntitySystem.Timers.GetTick();
                mPlayerScore = mPlayerScore.Where(kvp => ticknow - kvp.Value.tick <= 4800).ToDictionary(pair => pair.Key, pair => pair.Value);

                Dictionary<string, long> _partyScore = new Dictionary<string, long>();
                foreach (var kvp in mPlayerScore)
                {
                    string _playername = kvp.Key;
                    int score = kvp.Value.score;
                    int _partyid = PartyRules.GetPartyIdByPlayerName(_playername);
                    if (_partyid != 0)
                    {
                        string leader = PartyRules.GetPartyById(_partyid).leader;
                        if (_partyScore.ContainsKey(leader))
                            _partyScore[leader] += score;
                        else
                            _partyScore.Add(leader, score);
                    }
                    else
                        _partyScore[_playername] = score;
                }

                foreach (var kvp in _partyScore)
                {
                    PartyStatsServer _party = PartyRules.GetMyParty(kvp.Key);
                    if (_party != null && _party.GetPartyMemberList().Count > 1)
                    {
                        StringBuilder _sb = new StringBuilder();
                        _sb.Append(_bossinfo);
                        _sb.Append(kvp.Value + ";");
                        foreach (string _member in _party.GetPartyMemberList().Keys)
                        {
                            if (mPlayerScore.ContainsKey(_member))
                                _sb.AppendFormat("{0}: {1};", _member, mPlayerScore[_member].score);
                        }
                        GameApplication.Instance.BroadcastMessage_Party(BroadcastMessageType.BossKilledMyScore, _sb.ToString(), _party);
                    }
                    else
                    {
                        GameClientPeer _peer;
                        if (_peers.TryGetValue(kvp.Key, out _peer))
                            _peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.BossKilledMyScore, _bossinfo + kvp.Value, _peer);
                    }
                }
                mPartyScoreRank = _partyScore.ToList().OrderByDescending(x => x.Value).Take(10).ToList();
            }
            else
            {
                Dictionary<string, int> validPlayerInfo = new Dictionary<string, int>();
                foreach (var kvp in mPlayerDamages)
                {
                    GameClientPeer _peer;
                    if (_peers.TryGetValue(kvp.Key, out _peer) && _peer.mPlayer != null)
                    {
                        _peer.ZRPC.CombatRPC.BroadcastMessageToClient((byte)BroadcastMessageType.BossKilledMyDmg, _bossinfo + kvp.Value, _peer);
                        validPlayerInfo.Add(kvp.Key, kvp.Value);
                    }
                }
                mPlayerDamageRank = validPlayerInfo.ToList().OrderByDescending(x => x.Value).Take(10).ToList();
            }
        }

        private void HandleLoot(Player killer)
        {           
            int monsterLvl = mArchetype.level;
            MonsterClass monsterClass = mArchetype.monsterclass;

            var _peers = GameApplication.Instance.GetAllCharPeer();
            List<Player> validPlayers = new List<Player>();
            int validPlayerCount = 0;
            int validDamageTotal = 0; 
            foreach (var kvp in mPlayerDamages)
            {
                GameClientPeer _peer;
                if (_peers.TryGetValue(kvp.Key, out _peer) && _peer.mPlayer != null && _peer.mPlayer.mInstance == mInstance)
                {
                    validPlayers.Add(_peer.mPlayer);
                    validDamageTotal += kvp.Value;
                }
            }
            validPlayerCount = validPlayers.Count;
            //distribute exp
            int expTotal = mArchetype.exp;
            for (int index = 0; index < validPlayerCount; index++)
            {
                if (expTotal > 0)
                    validPlayers[index].OnNPCKilled(mArchetype, expTotal * mPlayerDamages[validPlayers[index].Name] / validDamageTotal);
                else
                    validPlayers[index].OnNPCKilled(mArchetype, 0);
            }

            NPCLootLink npcLootLink = CombatNPCRepo.GetNPCLootLink(mArchetype.id);
            if (npcLootLink == null)
                return;

            Dictionary<LootType, List<int>> lootMap = npcLootLink.GetLootGroupIDs(DateTime.Now);
            LootItemDisplayInventory lootItemDisplayInventory = new LootItemDisplayInventory();
            foreach (var kvp in lootMap)
            {
                switch (kvp.Key)
                {
                    case LootType.Normal:                                                
                        if (validPlayerCount > 0)
                        {
                            var lootItems = LootRepo.RandomItems(kvp.Value);
                            int lootItemsCount = lootItems.Count;
                            if (lootItemsCount > 0)
                            {
                                int index = GameUtils.RandomInt(0, validPlayerCount - 1);
                                Player toPlayer = validPlayers[index];
                                string toPlayerName = toPlayer.Name;
                                var partyInfo = PartyRules.GetMyParty(toPlayerName);
                                if (partyInfo != null)
                                {
                                    List<Player> toPartyPlayers = partyInfo.GetSameInstancePartyMembers(toPlayerName, mInstance);
                                    toPartyPlayers.Add(toPlayer);
                                    int toPartyPlayersCount = toPartyPlayers.Count;
                                    if (toPartyPlayersCount > 1)
                                        toPlayer = toPartyPlayers[GameUtils.RandomInt(0, toPartyPlayersCount - 1)];
                                }
                                LootRules.GenerateLootItem(toPlayer, lootItems, monsterClass, monsterLvl, null);
                            }
                        }
                        break;
                    case LootType.Share:
                        if (validPlayerCount > 0)
                        {
                            var lootItems = LootRepo.RandomItems(kvp.Value);
                            int lootItemsCount = lootItems.Count;
                            if (lootItemsCount > 0)
                            {
                                //store given player names to avoid give double loot.
                                Dictionary<string, bool> lootGiven = new Dictionary<string, bool>();
                                for (int index = 0; index < validPlayerCount; index++)
                                {
                                    Player toPlayer = validPlayers[index];
                                    string toPlayerName = toPlayer.Name;
                                    if (lootGiven.ContainsKey(toPlayerName))
                                        continue;
                                    LootRules.GenerateLootItem(toPlayer, lootItems, monsterClass, monsterLvl, null);
                                    lootGiven.Add(toPlayerName, true);

                                    var partyInfo = PartyRules.GetMyParty(toPlayerName);
                                    if (partyInfo != null)
                                    {
                                        foreach (var _member in partyInfo.GetPartyMemberList())
                                        {
                                            if (_member.Value.IsHero() || lootGiven.ContainsKey(_member.Key))
                                                continue;
                                            GameClientPeer _peer;
                                            if (_peers.TryGetValue(_member.Key, out _peer) && _peer.mPlayer != null && _peer.mPlayer.mInstance == this.mInstance)
                                            {
                                                LootRules.GenerateLootItem(_peer.mPlayer, lootItems, monsterClass, monsterLvl, null);
                                                lootGiven.Add(_member.Key, true);
                                            } 
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case LootType.Boss:
                        {
                            var lootItems = LootRepo.RandomItems(kvp.Value);
                            int lootItemsCount = lootItems.Count;
                            if (lootItemsCount > 0)
                            {
                                int validDamageRankCount = mPlayerDamageRank.Count;
                                if (validDamageRankCount == 1)
                                {
                                    GameClientPeer _peer;
                                    if (_peers.TryGetValue(mPlayerDamageRank[0].Key, out _peer) && _peer.mPlayer != null)
                                        LootRules.GenerateLootItem(_peer.mPlayer, lootItems, monsterClass, monsterLvl, lootItemDisplayInventory);
                                }
                                else if (validDamageRankCount >= 2)
                                {
                                    int _dmgRatioTop2 = Mathf.FloorToInt(100.0f * mPlayerDamageRank[1].Value / (mPlayerDamageRank[0].Value + mPlayerDamageRank[1].Value));
                                    if (_dmgRatioTop2 <= 16) // second player dmg less than first by ratio 1/5
                                    {
                                        GameClientPeer _peer;
                                        if (_peers.TryGetValue(mPlayerDamageRank[0].Key, out _peer) && _peer.mPlayer != null)
                                            LootRules.GenerateLootItem(_peer.mPlayer, lootItems, monsterClass, monsterLvl, lootItemDisplayInventory);
                                    }
                                    else
                                    {
                                        Player top1Player = null;
                                        Player top2Player = null;
                                        GameClientPeer _peer;
                                        if (_peers.TryGetValue(mPlayerDamageRank[0].Key, out _peer) && _peer.mPlayer != null)
                                            top1Player = _peer.mPlayer;
                                        if (_peers.TryGetValue(mPlayerDamageRank[1].Key, out _peer) && _peer.mPlayer != null)
                                            top2Player = _peer.mPlayer;                                     
                                        List<LootItem> toTop1Player = new List<LootItem>();
                                        List<LootItem> toTop2Player = new List<LootItem>();
                                        for (int index = 0; index < lootItemsCount; index++)
                                        {
                                            if (GameUtils.RandomInt(1, 100) <= _dmgRatioTop2)
                                                toTop2Player.Add(lootItems[index]);
                                            else
                                                toTop1Player.Add(lootItems[index]);
                                        }
                                        if (toTop1Player.Count > 0)
                                            LootRules.GenerateLootItem(top1Player, toTop1Player, monsterClass, monsterLvl, lootItemDisplayInventory);
                                        if (toTop2Player.Count > 0)
                                            LootRules.GenerateLootItem(top2Player, toTop2Player, monsterClass, monsterLvl, lootItemDisplayInventory);
                                    }
                                }
                            }
                        }
                        break;
                    case LootType.BigBoss:
                        {
                            var lootItems = LootRepo.RandomItems(kvp.Value);
                            int lootItemsCount = lootItems.Count;
                            if (lootItemsCount > 0)
                            {
                                List<string> _lootPlayers = new List<string>();
                                if (mPartyScoreRank.Count >= 1)
                                {
                                    string _name = mPartyScoreRank[0].Key;
                                    PartyStatsServer _party = PartyRules.GetMyParty(_name);
                                    if (_party != null)
                                    {
                                        foreach (var _member in _party.GetPartyMemberList())
                                        {
                                            if (!_member.Value.IsHero() && mPlayerScore.ContainsKey(_member.Key))
                                                _lootPlayers.Add(_member.Key);
                                        }
                                    }
                                    else
                                        _lootPlayers.Add(_name);
                                }
                                int lootPlayerCount = _lootPlayers.Count;
                                if (lootPlayerCount == 1)
                                {
                                    string playerName = _lootPlayers[0];
                                    GameClientPeer _peer;
                                    if (_peers.TryGetValue(playerName, out _peer) && _peer.mPlayer != null)
                                        LootRules.GenerateLootItem(_peer.mPlayer, lootItems, monsterClass, monsterLvl, lootItemDisplayInventory);
                                    else
                                        LootRules.GenerateLootItem_SendMail(playerName, lootItems, lootItemDisplayInventory);
                                }
                                else
                                {
                                    int interval = 100 / lootPlayerCount;
                                    Dictionary<string, List<LootItem>> toLootPlayers = new Dictionary<string, List<LootItem>>();
                                    for (int index = 0; index < lootItemsCount; index++)
                                    {
                                        int toLootPlayerIndex = Math.Min(GameUtils.RandomInt(0, 99) / interval, lootPlayerCount - 1);
                                        string toLootPlayerName = _lootPlayers[toLootPlayerIndex];
                                        if (!toLootPlayers.ContainsKey(toLootPlayerName))
                                            toLootPlayers.Add(toLootPlayerName, new List<LootItem>());
                                        toLootPlayers[toLootPlayerName].Add(lootItems[index]);
                                    }
                                    foreach(var kvp2 in toLootPlayers)
                                    {
                                        string playerName = kvp2.Key;
                                        GameClientPeer _peer;
                                        if (_peers.TryGetValue(playerName, out _peer) && _peer.mPlayer != null)
                                            LootRules.GenerateLootItem(_peer.mPlayer, kvp2.Value, monsterClass, monsterLvl, lootItemDisplayInventory);
                                        else
                                            LootRules.GenerateLootItem_SendMail(playerName, kvp2.Value, lootItemDisplayInventory);
                                    }
                                }
                            }
                        }
                        break;
                    case LootType.LastHit:
                        if (killer != null)
                        {
                            var lootItems = LootRepo.RandomItems(kvp.Value);
                            if (lootItems.Count > 0)
                                LootRules.GenerateLootItem(killer, lootItems, monsterClass, monsterLvl, null);
                        }
                        break;
                    case LootType.Explore: //monster should not set to Explore
                        break;
                }
            }
            //send LootItemDisplay to players within 20m
            if (lootItemDisplayInventory.records.Count > 0)
            {
                Vector3 pos = Position;
                lootItemDisplayInventory.SetPos(pos.x, pos.y, pos.z);
                List<Entity> qr = new List<Entity>();
                EntitySystem.QueryNetEntitiesInCircle(pos, 15, (queriedEntity) =>
                {
                    return (queriedEntity as Player != null);
                }, qr);
                int qrCount = qr.Count;
                if (qrCount > 0)
                {
                    string lootDisplay = lootItemDisplayInventory.ToString();
                    RPCBroadcastData rpcdata = mInstance.ZRPC.CombatRPC.GetSerializedRPC(ServerCombatRPCMethods.LootItemDisplay, lootDisplay);
                    for (int index = 0; index < qrCount; index++)
                        ((Player)qr[index]).Slot.SendEvent(rpcdata.EventData, rpcdata.SendParameters);
                }
            }
        }
        #endregion

        public override void OnDamage(IActor attacker, AttackResult res, bool pbasicattack)
        {
            if (mArchetype.dmgbyhitcount)
                res.RealDamage = 1;

            if (res.RealDamage > 0)
            {
                Player player = attacker as Player;
                if (player != null)
                {
                    AddDamageRecord(player.Name, res.RealDamage); //actual damage caused is less if health is lower than damage
                    if (mInstance.mRealmController != null)
                        mInstance.mRealmController.OnDealtDamage(player, this, res.RealDamage);
                }
            }
            if (mSp!=null)
                mSp.OnChildDamaged(attacker);
             
            base.OnDamage(attacker, res, pbasicattack);
        }

        public override void OnAttacked(IActor attacker, int aggro)
        {
            mAIController.OnAttacked(attacker, aggro);
            if (mIsBigBossLoot)
            {
                uint ticknow = EntitySystem.Timers.GetTick();
                if (mOnAttackedTick == 0 || ticknow - mOnAttackedTick > 75)
                {
                    mOnAttackedTick = ticknow;
                    List<Entity> qr = new List<Entity>();
                    EntitySystem.QueryNetEntitiesInCircle(this.Position, 15, (queriedEntity) =>
                    {
                        return (queriedEntity as Player != null);
                    }, qr);
                    foreach(var entity in qr)
                    {
                        string playerName = ((Player)entity).Name;
                        if (!mPlayerScore.ContainsKey(playerName))
                            mPlayerScore.Add(playerName, new BigBossScoreRecord { score = 1, tick = ticknow });
                        else
                            mPlayerScore[playerName].tick = ticknow;
                    }
                }
            }
            if (mIsBoss)
                mBossNoDmgCountdown = mBossNoDmgCountdownConst;
        }

        public void OnGroupAggro(int pid, IActor attacker)
        {
            if (GetPersistentID() != pid)
            {
                mAIController.OnGroupAggro(attacker, 1);
            }
        }

        public void OnKnockedBack(Vector3 targetpos)
        { 
            //if (!mArchetype.canbeknockback)
            //    return;
            
            mAIController.GotoState("RecoverFromKnockedBack");
            KnockedBackCommand cmd = new KnockedBackCommand();
            cmd.targetpos = targetpos;
            ServerAuthoKnockedBack kbAction = new ServerAuthoKnockedBack(this, cmd);
            kbAction.SetCompleteCallback(() => {
                Idle();
            }); 
            PerformAction(kbAction);//the monster may be in any AIBehaviour State when perform knockedBack aciton. 
        }
         
        public void OnKnockedUp(float dur)
        {
            mAIController.GotoState("RecoverFromKnockedBack");
            KnockedUpCommand cmd = new KnockedUpCommand();
            cmd.dur = dur;
            ServerAuthoKnockedUp action = new ServerAuthoKnockedUp(this, cmd);
            action.SetCompleteCallback(() => {
                Idle(); 
            });
            PerformAction(action);
        }

        public override void onDragged(Vector3 pos, float dur, float speed)
        {             
            DraggedActionCommand cmd = new DraggedActionCommand();
            cmd.pos = pos;
            cmd.dur = dur;
            cmd.speed = speed;
            ASDragged action = new ASDragged(this, cmd);
            action.SetCompleteCallback(() => {
                Idle();
            });
            PerformAction(action);
        }

        public bool HasMoved { get; set; }

        public override void OnStun()
        {
            base.OnStun();
            if (mArchetype.monsterclass == MonsterClass.Normal)
                mAIController.GotoState("Stun");
        }

        public override void OnRoot()
        {
            base.OnRoot();
            if (IsMoving())
            {
                Idle();
            }
        }
                     
        ///////////////////////////////////////////////////////////////
        //Available actions performable by monster:        
        public void Idle()
        {
            ServerAuthoASIdle idleAction = new ServerAuthoASIdle(this, new IdleActionCommand());
            PerformAction(idleAction);
        }

        public void MoveTo(Vector3 pos, bool roam = false)
        {
            WalkActionCommand cmd = new WalkActionCommand();
            cmd.targetPos = pos;
            cmd.speed = roam ? PlayerStats.MoveSpeed / 2 : 0;
            ServerAuthoASWalk walkAction = new ServerAuthoASWalk(this, cmd);
            walkAction.SetCompleteCallback(Idle);
            PerformAction(walkAction);
        }

        public void CastSkill(int skillid, int targetPID)
        {            
            CastSkillCommand cmd = new CastSkillCommand();
            cmd.skillid = skillid;
            cmd.targetpid = targetPID;
            ServerAuthoCastSkill action = new ServerAuthoCastSkill(this, cmd);            
            action.SetCompleteCallback(Idle);
            PerformAction(action);
        }

        public void ApproachTarget(int targetPID, float range)
        {
            ApproachCommand cmd = new ApproachCommand();
            cmd.targetpid = targetPID;
            cmd.range = range;            
            ServerAuthoASApproach approachAction = new ServerAuthoASApproach(this, cmd);
            approachAction.SetCompleteCallback(Idle);
            PerformAction(approachAction);
        }

        public void ApproachTargetWithPathFind(int targetPID, Vector3? pos, float range, bool targetposSafe, bool movedirectonpathfound)
        {
            ApproachWithPathFindCommand cmd = new ApproachWithPathFindCommand();
            cmd.targetpid = targetPID;
            cmd.targetpos = pos;
            cmd.range = range;
            cmd.targetposSafe = targetposSafe;
            cmd.movedirectonpathfound = movedirectonpathfound;
            ASApproachWithPathFind approachAction = new ASApproachWithPathFind(this, cmd);
            approachAction.SetCompleteCallback(Idle);
            PerformAction(approachAction);
        }

        ///////////////////////////////////////////////////////////////
    }
}

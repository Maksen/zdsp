using Zealot.Client.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Zealot.Common.Entities
{
    public class ClientEntitySystem : EntitySystem
    {
        enum EntityPriority
        {
            ////boss->party->monster->player
            BOSS,
            PARTY,
            MONSTER,
            OTHERPLAYER
        }
        public int MAX_ENTITY { get; set; }
        public int TotalQuestNPCSpawner { get; set; }
        List<MonsterGhost> mAllBoss;
        List<MonsterGhost> mAllNormalMonster;
        /// <summary>
        /// excluding ownself
        /// </summary>
        List<PlayerGhost> mAllPartyMember;
        /// <summary>
        /// excluding ownself
        /// </summary>
        List<PlayerGhost> mAllOtherPlayer;

        public ClientEntitySystem(Timers timers) : base(timers)
        {
            MAX_ENTITY = 15;
            mAllBoss = new List<MonsterGhost>();
            mAllNormalMonster = new List<MonsterGhost>();
            mAllPartyMember = new List<PlayerGhost>();
            mAllOtherPlayer = new List<PlayerGhost>();
            TotalQuestNPCSpawner = 0;
        }

        //Corresponding Ghost at client
        public T SpawnNetEntityGhost<T>(int pid) where T : BaseNetEntityGhost, new()
        {
            int id = mIDPool.AllocID(mTimers.GetTick());
            T entity = new T();
            entity.EntitySystem = this;
            entity.SetID(id);
            entity.SetPersistentID(pid);

            OnAddEntity(id, pid, entity);
            return entity;
        }

        public T SpawnClientEntity<T>() where T : BaseClientEntity, new()
        {
            int id = mIDPool.AllocID(mTimers.GetTick());
            T entity = new T();
            entity.EntitySystem = this;
            entity.SetID(id);

            OnAddEntity(id, 0, entity);
            return entity;
        }

        public void RemoveAllNetEntities()
        {
            List<int> pids = mNetEntities.Keys.ToList();
            for (int index = pids.Count - 1; index >= 0; index--)
                RemoveEntityByPID(pids[index]);
        }

        /// <summary>
        /// this is for cuiling purpose
        /// </summary>
        public void AddMonster(MonsterGhost monster)
        {
            //if (monster != null)
            //{
            //    if (monster.mArchetype.monsterclass == MonsterClass.Boss)
            //        mAllBoss.Add(monster);
            //    else
            //        mAllNormalMonster.Add(monster);
            //}
        }

        public void AddNPC(QuestNPC npc)
        {
            //if (npc != null)
            //    mAllNPC.Add(npc);
        }

        public void AddPlayer(PlayerGhost player, bool partyMember)
        {
            //if (partyMember)
            //{
            //    mAllPartyMember.Add(player);
            //}
            //else
            //{
            //    mAllOtherPlayer.Add(player);
            //}
        }
        //public void RemoveMonster(MonsterGhost monster)
        //{
        //    if (monster != null)
        //    {
        //        if (monster.mArchetype.monstertype == PiliQ.MonsterType.Boss)
        //            mAllBoss.Remove(monster);
        //        else
        //            mAllNormalMonster.Remove(monster);
        //    }
        //}

        //For client to get relevant players
        public List<PlayerGhost> GetPlayers()
        {
            List<PlayerGhost> res = new List<PlayerGhost>();
            foreach (Entity e in mNetEntities.Values)
            {
                PlayerGhost ep = e as PlayerGhost;
                if (ep != null && !ep.IsLocal)
                {
                    res.Add(ep);
                }
            }
            return res;
        }

        public PlayerGhost GetPlayerByName(string name)
        {
            foreach (Entity e in mNetEntities.Values)
            {
                PlayerGhost ep = e as PlayerGhost;
                if (ep != null && !ep.IsLocal)
                {
                    if (ep.Name == name)
                        return ep;
                }
            }
            return null;
        }

        //For client to get relevant monsters
        public List<MonsterGhost> GetMonsters()
        {
            List<MonsterGhost> res = new List<MonsterGhost>();
            foreach (Entity e in mNetEntities.Values)
            {
                if (e.EntityType == EntityType.MonsterGhost)
                {
                    res.Add(e as MonsterGhost);
                }
            }
            return res;
        }

        public void RemoveClientEntityByType(Entity type)
        {
            //if (type.IsMonster() == true)
            //{
            //    MonsterGhost monster = type as MonsterGhost;
            //    if (monster.mArchetype.monsterclass == MonsterClass.Boss)
            //    {
            //        mAllBoss.Remove(monster);
            //    }
            //    else
            //    {
            //        mAllNormalMonster.Remove(monster);
            //    }
            //}
            //else if(type.IsPlayer() == true)
            //{
            //    PlayerGhost player = type as PlayerGhost;
            //    mAllPartyMember.Remove(player);
            //    mAllOtherPlayer.Remove(player);
            //}
        }

        //For map to get all nearby radar visible entities
        public void GetRadarVisibleEntities(List<string> partyMemberNames, List<UnityEngine.Vector3> pmembers,
                                            List<UnityEngine.Vector3> players, List<UnityEngine.Vector3> monsters, List<UnityEngine.Vector3> crystal)
        {
            foreach (Entity e in mNetEntities.Values)
            {
                if (e.EntityType == EntityType.MonsterGhost)
                {
                    monsters.Add(e.Position);
                }
                else if (e.EntityType == EntityType.PlayerGhost)
                {
                    PlayerGhost ep = e as PlayerGhost;
                    if (ep != null)
                    {
                        if (partyMemberNames.Contains(ep.Name))
                        {
                            pmembers.Add(ep.Position);
                        }
                        else if (!ep.IsLocal)
                        {
                            players.Add(ep.Position);
                        }
                    }
                }
            }
        }

        public void GetRadarVisibleEntities2(List<string> partyMemberNames, List<UnityEngine.Vector3> pmembers,
                                             List<UnityEngine.Vector3> monsters, List<UnityEngine.Vector3> miniboss,
                                             List<UnityEngine.Vector3> boss)
        {
            foreach (Entity e in mNetEntities.Values)
            {
                if (e.EntityType == EntityType.MonsterGhost)
                {
                    MonsterGhost mg = e as MonsterGhost;
                    if (mg.mArchetype == null)
                        continue;

                    switch (mg.mArchetype.monsterclass)
                    {
                        case MonsterClass.Normal:
                            monsters.Add(e.Position);
                            break;
                        case MonsterClass.Mini:
                            miniboss.Add(e.Position);
                            break;
                        case MonsterClass.Boss:
                            boss.Add(e.Position);
                            break;
                    }
                }
                else if (e.EntityType == EntityType.PlayerGhost)
                {
                    PlayerGhost ep = e as PlayerGhost;
                    if (ep != null && partyMemberNames.Contains(ep.Name))
                    {
                        pmembers.Add(ep.Position);
                    }
                }
            }
        }

        public void GetRadarVisibleEntities3(List<string> partyMemberNames, List<UnityEngine.GameObject> pmembers,
                                             List<UnityEngine.GameObject> monsters, List<UnityEngine.GameObject> miniboss,
                                             List<UnityEngine.GameObject> boss)
        {
            foreach (Entity e in mNetEntities.Values)
            {
                if (e.EntityType == EntityType.MonsterGhost)
                {
                    MonsterGhost mg = e as MonsterGhost;
                    if (mg.mArchetype == null)
                        continue;

                    switch (mg.mArchetype.monsterclass)
                    {
                        case MonsterClass.Normal:
                            monsters.Add(mg.AnimObj);
                            break;
                        case MonsterClass.Mini:
                            miniboss.Add(mg.AnimObj);
                            break;
                        case MonsterClass.Boss:
                            boss.Add(mg.AnimObj);
                            break;
                    }
                }
                else if (e.EntityType == EntityType.PlayerGhost)
                {
                    PlayerGhost ep = e as PlayerGhost;
                    if (ep != null && partyMemberNames.Contains(ep.Name))
                    {
                        pmembers.Add(ep.AnimObj);
                    }
                }
            }
        }

        public void ShowAllEntities(bool show)
        {
            foreach (var entity in mEntities.Values)
            {
                if (entity != null && entity is BaseClientEntity)
                    ((BaseClientEntity)entity).Show(show);
            }
        }

        void CheckPriority()
        {
            if (mEntities.Count - TotalQuestNPCSpawner - 1<= MAX_ENTITY)//minus one as to exlucde local player
            {
                foreach(var entity in mEntities)
                {
                    BaseClientEntity bce = entity.Value as BaseClientEntity;
                    if (bce != null && bce.IsNPC() == false)
                    {
                        if (bce.HasAnimObj == true && bce.IsVisible() == false)
                            bce.Show(true);
                    }
                }
            }
            else
            {
                //boss->party->enemy player->monster->friendly player->neutral player
                //boss->party->monster->player
                //for (int i = MAX_ENTITY; i < mEntities.Count; i++)
                //{
                //    BaseClientEntity bce = mEntities[i] as BaseClientEntity;
                //    if (bce.GetVisible() == true)
                //        bce.Show(false);
                //}

                ////////////boss///////////////
                int entityleft = MAX_ENTITY;
                int currentendindex = mAllBoss.Count;
                if (mAllBoss.Count > MAX_ENTITY)
                {
                    currentendindex = MAX_ENTITY;
                }

                //show
                for (int i = 0; i < currentendindex; i++)
                {
                    MonsterGhost boss = mAllBoss[i];
                    if (boss.IsVisible() == false)
                        boss.Show(true);
                }

                //hide
                for (int i = currentendindex; i < mAllBoss.Count; i++)
                {
                    MonsterGhost boss = mAllBoss[i];
                    if (boss.IsVisible() == true)
                        boss.Show(false);
                }

                ////////////party///////////////
                entityleft -= currentendindex;
                if (entityleft <= 0)
                {
                    HideRemainingPriorityEntity(EntityPriority.BOSS);
                    return;
                }

                currentendindex = mAllPartyMember.Count;
                if (mAllPartyMember.Count > entityleft)
                {
                    currentendindex = entityleft;
                }
                //show
                for (int i = 0; i < currentendindex; i++)
                {
                    PlayerGhost partyplayer = mAllPartyMember[i];
                    if (partyplayer.IsVisible() == false)
                        partyplayer.Show(true);
                }
                //hide
                for (int i = currentendindex; i < mAllPartyMember.Count; i++)
                {
                    PlayerGhost partyplayer = mAllPartyMember[i];
                    if (partyplayer.IsVisible() == true)
                        partyplayer.Show(false);
                }
                ////////////party///////////////

                ////////////npc///////////////

                ////////////npc///////////////
                //entityleft -= currentendindex;
                //if (entityleft <= 0)
                //    return;

                //currentendindex = mAllNPC.Count;
                //if (mAllNPC.Count > entityleft)
                //{
                //    currentendindex = entityleft;
                //}
                ////show
                //for (int i = 0; i < currentendindex; i++)
                //{
                //    QuestNPC npc = mAllNPC[i];
                //    if (npc.GetVisible() == false)
                //        npc.Show(true);
                //}
                ////hide
                //for (int i = currentendindex; i < mAllNPC.Count; i++)
                //{
                //    QuestNPC npc = mAllNPC[i];
                //    if (npc.GetVisible() == true)
                //        npc.Show(false);
                //}
                //entityleft -= currentendindex;
                //if (entityleft <= 0)
                //    return;
                ////////////npc///////////////

                ////////////monster///////////////
                entityleft -= currentendindex;
                if (entityleft <= 0)
                {
                    HideRemainingPriorityEntity(EntityPriority.PARTY);
                    return;
                }

                currentendindex = mAllNormalMonster.Count;
                if (mAllNormalMonster.Count > entityleft)
                {
                    currentendindex = entityleft;
                }
                //show
                for (int i = 0; i < currentendindex; i++)
                {
                    MonsterGhost monster = mAllNormalMonster[i];
                    if (monster.IsVisible() == false)
                        monster.Show(true);
                }
                //hide
                for (int i = currentendindex; i < mAllNormalMonster.Count; i++)
                {
                    MonsterGhost monster = mAllNormalMonster[i];
                    if (monster.IsVisible() == true)
                        monster.Show(false);
                }
                ////////////monster///////////////

                ////////////player///////////////
                entityleft -= currentendindex;
                if (entityleft <= 0)
                {
                    HideRemainingPriorityEntity(EntityPriority.MONSTER);
                    return;
                }

                //List<PlayerGhost> allplayer = GetPlayers();
                currentendindex = mAllOtherPlayer.Count;
                if (mAllOtherPlayer.Count > entityleft)
                {
                    currentendindex = entityleft;
                }
                //show
                for (int i = 0; i < currentendindex; i++)
                {
                    PlayerGhost player = mAllOtherPlayer[i];
                    if (player.IsVisible() == false)
                        player.Show(true);
                }

                //hide
                for (int i = currentendindex; i < mAllOtherPlayer.Count; i++)
                {
                    PlayerGhost player = mAllOtherPlayer[i];
                    if (player.IsVisible() == true)
                        player.Show(false);
                }
                ////////////player///////////////
            }
        }

        void HideRemainingPriorityEntity(EntityPriority type)
        {
            switch (type)
            {
                case EntityPriority.BOSS:
                    HidePartyMemeber();
                    HideMonster();
                    HideOtherPlayer();
                    break;
                case EntityPriority.PARTY:
                    HideMonster();
                    HideOtherPlayer();
                    break;
                case EntityPriority.MONSTER:
                    HideOtherPlayer();
                    break;
                case EntityPriority.OTHERPLAYER:
                    break;
                default:
                    break;
            }
        }

        void HideBoss()
        {
            for(int i = 0; i < mAllBoss.Count; i++)
                {
                MonsterGhost boss = mAllBoss[i];
                if (boss.IsVisible() == true)
                    boss.Show(false);
            }
        }
        void HidePartyMemeber()
        {
            for(int i=0;i< mAllPartyMember.Count;i++)
            {
                PlayerGhost partyplayer = mAllPartyMember[i];
                if (partyplayer.IsVisible() == true)
                    partyplayer.Show(false);
            }
        }
        
        void HideMonster()
        {
            for (int i = 0; i < mAllNormalMonster.Count; i++)
            {
                MonsterGhost monster = mAllNormalMonster[i];
                if (monster.IsVisible() == true)
                    monster.Show(false);
            }
        }

        void HideOtherPlayer()
        {
            for (int i = 0; i < mAllOtherPlayer.Count; i++)
            {
                PlayerGhost player = mAllOtherPlayer[i];
                if (player.IsVisible() == true)
                    player.Show(false);
            }
        }
        public override void Update(long dt)
        {
            base.Update(dt);
            //CheckPriority();
        }

        public List<QuestNPC> GetAllQuestNPC()
        {
            return mEntities.Values.Where(entity => entity.GetType() == typeof(QuestNPC)).Select(entity => entity as QuestNPC).ToList();
        }

        public QuestNPC GetQuestNPC(string archetype)
        {
            foreach (KeyValuePair<int, Entity> entity in mEntities)
            {
                if (entity.Value.GetType() == typeof(QuestNPC))
                {
                    QuestNPC questnpc = entity.Value as QuestNPC;
                    if (questnpc.ArchetypeName == archetype)
                        return questnpc;
                }
            }
            return null;
        }

        public StaticClientNPCAlwaysShow GetStaticClientNPC(int archetypeid)
        {
            foreach (KeyValuePair<int, Entity> entity in mEntities)
            {
                if (entity.Value.GetType() == typeof(QuestNPC))
                {
                    QuestNPC questnpc = entity.Value as QuestNPC;
                    if (questnpc.mArchetypeID == archetypeid)
                        return questnpc;
                }
                else if (entity.Value.GetType() == typeof(StaticAreaGhost))
                {
                    StaticAreaGhost staticarea = entity.Value as StaticAreaGhost;
                    if (staticarea.mArchetypeID == archetypeid)
                        return staticarea;
                }
                else if (entity.Value.GetType() == typeof(StaticTargetGhost))
                {
                    StaticTargetGhost statictarget = entity.Value as StaticTargetGhost;
                    if (statictarget.mArchetypeID == archetypeid)
                        return statictarget;
                }
            }
            return null;
        }
        public StaticClientNPCAlwaysShow GetStaticClientNPC(string archetypeName)
        {
            foreach (KeyValuePair<int, Entity> entity in mEntities)
            {
                if (entity.Value.GetType() == typeof(QuestNPC))
                {
                    QuestNPC questnpc = entity.Value as QuestNPC;
                    if (string.Compare(questnpc.ArchetypeName, archetypeName) == 0)
                        return questnpc;
                }
                else if (entity.Value.GetType() == typeof(StaticAreaGhost))
                {
                    StaticAreaGhost staticarea = entity.Value as StaticAreaGhost;
                    if (string.Compare(staticarea.ArchetypeName, archetypeName) == 0)
                        return staticarea;
                }
                else if (entity.Value.GetType() == typeof(StaticTargetGhost))
                {
                    StaticTargetGhost statictarget = entity.Value as StaticTargetGhost;
                    if (string.Compare(statictarget.ArchetypeName, archetypeName) == 0)
                        return statictarget;
                }
            }
            return null;
        }

        public List<StaticClientNPCAlwaysShow> GetVisibleStaticNPC()
        {
            List<StaticClientNPCAlwaysShow> npclist = new List<StaticClientNPCAlwaysShow>();
            foreach (KeyValuePair<int, Entity> entity in mEntities)
            {
                if (entity.Value.GetType() == typeof(QuestNPC) || entity.Value.GetType() == typeof(StaticAreaGhost) || entity.Value.GetType() == typeof(StaticTargetGhost))
                {
                    StaticClientNPCAlwaysShow staticnpc = entity.Value as StaticClientNPCAlwaysShow;
                    if (staticnpc.IsVisible())
                    {
                        npclist.Add(staticnpc);
                    }
                }
            }
            return npclist;
        }
    }
}
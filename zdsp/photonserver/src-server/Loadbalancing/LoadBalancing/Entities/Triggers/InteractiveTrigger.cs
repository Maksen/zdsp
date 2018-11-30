using Photon.LoadBalancing.GameServer;
using System.Collections.Generic;
using Zealot.Entities;
using Zealot.Common;
using Zealot.Common.Datablock;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    public class InteractiveTrigger : IServerEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public GameLogic mInstance;

        private InteractiveGate mEntity = null;
        private bool isArea;

        public InteractiveTrigger(InteractiveTriggerJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            isArea = info.isArea;
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {
            if (mEntity == null && !string.IsNullOrEmpty(mPropertyInfos.npcArchetype))
                SpawnEntity();
        }

        void SpawnEntity()
        {
            InteractiveGate entity = mInstance.mEntitySystem.SpawnNetEntity<InteractiveGate>();
            mInstance.mEntitySystem.AddAlwaysShow(entity);
            entity.Position = mPropertyInfos.position;
            entity.Init(this);
            mEntity = entity;
        }

        public void OnInteractiveUse(int pid, bool enter, Player peer)
        {
            string name = peer.Name;
            if (enter)
            {
                if(CanUseByCount())
                    InteractiveTriggerRule.UseInteractiveTrigger(pid, isArea, name, mEntity, peer);
            }
            else
            {
                InteractiveTriggerRule.LeaveInteractiveTrigger(pid, isArea, name, mEntity);
            }
        }

        public void OnInteractive(int pid, Player peer)
        {
            bool canCount = CanUseByCount();
            
            if (canCount)
            {
                if (mPropertyInfos.keyId > 0)
                {
                    List<ItemInfo> consumeItem = new List<ItemInfo>() {
                        new ItemInfo { itemId = (ushort)mPropertyInfos.keyId, stackCount = 1 }
                    };
                    InvRetval result = peer.Slot.mInventory.DeductItems(consumeItem, "OnInteractive");
                    if (result.retCode == InvReturnCode.UseFailed)
                    {
                        return;
                    }
                }
                mEntity.count = (mEntity.count == -1) ? -1 : mEntity.count - 1;
                InteractiveTriggerRule.CompeletedInteradtiveTrigger(pid, isArea, mEntity);
                
                if (CanUseByCount())
                {
                    if (isArea)
                    {
                        OnInteractiveUse(pid, true, peer);
                    }
                    mEntity.SetEntityStep((int)InteractiveTriggerStep.None);
                }
                else
                {
                    mEntity.SetEntityStep((int)InteractiveTriggerStep.CannotUse);
                }

                object[] _paramters = { peer };
                mInstance.BroadcastEvent(this, "OnInteractive", _paramters);
            }
        }

        private bool CanUseByCount()
        {
            return mEntity.count > 0 || mEntity.count == -1;
        }

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mEntity.ForceSetEntityActive(true);
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            mEntity.ForceSetEntityActive(false);
        }

        public void Reset (IServerEntity sender, object[] parameters = null)
        {
            mEntity.count = mPropertyInfos.counter;
            mEntity.ForceSetEntityActive(true);
        }
        #endregion
    }

    public class InteractiveGate : NetEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public string entityName;
        public int count;
        public bool isUsing;
        public bool canUse = true;
        public InteractiveTriggerStep step;

        private bool isArea;
        private InteractiveTriggerSynStats mdbInteractiveTriggerStats;

        public InteractiveGate() : base()
        {
            this.EntityType = EntityType.InteractiveTrigger;
            mdbInteractiveTriggerStats = new InteractiveTriggerSynStats();
        }

        public void Init(InteractiveTrigger interactive)
        {
            mInstance = interactive.mInstance;
            mPropertyInfos = interactive.mPropertyInfos;
            entityName = mPropertyInfos.npcArchetype;
            count = interactive.mPropertyInfos.counter;
            step = InteractiveTriggerStep.None;

            isArea = interactive.mPropertyInfos.isArea;
            mdbInteractiveTriggerStats.entityId = this.GetPersistentID();
            mdbInteractiveTriggerStats.step = (interactive.mPropertyInfos.npcActiveOnStartup) ?
                (int)InteractiveTriggerStep.None : (int)InteractiveTriggerStep.InActive;
            InteractiveTriggerRule.AddEntityList(this);
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.SpawnInteractiveEntity(mnPersistentID, entityName, mPropertyInfos.parentPath,
                     mPropertyInfos.isArchetypeNpc, Position.ToRPCPosition(), peer);

            AddLocalObject(peer);
            SetEntityStep((int)step);
        }
        #endregion

        public override void UpdateEntitySyncStats(GameClientPeer peer)
        {
            if (mdbInteractiveTriggerStats.IsDirty())
            {
                peer.ZRPC.LocalObjectRPC.UpdateLocalObject((byte)LOCATEGORY.EntitySyncStats, GetPersistentID(), mdbInteractiveTriggerStats, peer);
            }
        }

        public override void ResetSyncStats()
        {
            if (mdbInteractiveTriggerStats.IsDirty())
            {
                mdbInteractiveTriggerStats.Reset();
            }
        }

        public bool GetEntityCanUse()
        {
            return mdbInteractiveTriggerStats.step != (int)InteractiveTriggerStep.CannotUse;
        }

        public bool GetEntityActive()
        {
            return mdbInteractiveTriggerStats.step != (int)InteractiveTriggerStep.InActive;
        }

        public void SetEntityActive(bool active)
        {
            mdbInteractiveTriggerStats.step = GetStep(active);
            mdbInteractiveTriggerStats.SetDirty();
        }

        private int GetStep(bool active)
        {
            if (active)
            {
                bool canActiveWithTime = InteractiveTriggerRule.CanActiveGameObject(this);
                if (!canActiveWithTime)
                {
                    step = InteractiveTriggerStep.InActive;
                    return (int)step;
                }
                step = (canUse) ? InteractiveTriggerStep.None : InteractiveTriggerStep.CannotUse;
            }
            else
            {
                step = InteractiveTriggerStep.InActive;
            }

            return (int)step;
        }

        public void ForceSetEntityActive(bool active)
        {
            mdbInteractiveTriggerStats.step = (active) ? (int)InteractiveTriggerStep.None : (int)InteractiveTriggerStep.InActive;
            SetEntityActive(active);
        }

        public void SetEntityStep(int triggerStats, string name = "")
        {
            step = (InteractiveTriggerStep)triggerStats;
            mdbInteractiveTriggerStats.step = triggerStats;
            mdbInteractiveTriggerStats.playerName = name;
            mdbInteractiveTriggerStats.SetDirty();
        }
    }
}
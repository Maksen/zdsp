using Photon.LoadBalancing.GameServer;
using System.Collections.Generic;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    public class InteractiveTrigger : IServerEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public GameLogic mInstance;
        private int mCounter = 0;
        public InteractiveGate mEntity = null;

        public InteractiveTrigger(InteractiveTriggerJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            mCounter = info.counter;
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {
            if (mEntity == null)
                SpawnEntity();
        }

        void SpawnEntity()
        {
            InteractiveGate entity = mInstance.mEntitySystem.SpawnNetEntity<InteractiveGate>();
            mInstance.mEntitySystem.AddAlwaysShow(entity);
            entity.Position = mPropertyInfos.position;
            entity.Forward = mPropertyInfos.forward;
            entity.Init(this);
            mEntity = entity;

            InteractiveTriggerController.AddEntityToPid(mEntity);
        }

        public void OnInteractiveUse(int pid, bool enter, GameClientPeer player)
        {
            if (enter)
            {
                if(CanUseByCount())
                    InteractiveTriggerRule.UseInteractiveTrigger(pid, player.mPlayer.Name,
                        mEntity, mPropertyInfos.interactiveTime, mPropertyInfos.isArea);
            }
            else
            {
                InteractiveTriggerRule.InterruptedEvent(pid, player.mPlayer.Name, mEntity, mPropertyInfos.isArea);
            }
        }

        public void OnInteractive(int pid, GameClientPeer player)
        {
            bool canCount = CanUseByCount();
            
            if (canCount)
            {
                mEntity.ClearAction();
                if (mPropertyInfos.keyId > 0)
                {
                    List<ItemInfo> consumeItem = new List<ItemInfo>() {
                        new ItemInfo { itemId = (ushort)mPropertyInfos.keyId, stackCount = 1 }
                    };
                    InvRetval result = player.mPlayer.Slot.mInventory.DeductItems(consumeItem, "OnInteractive");
                    if (result.retCode == InvReturnCode.UseFailed)
                    {
                        return;
                    }
                }

                mCounter = (mCounter == -1) ? -1 : mCounter - 1;
                InteractiveTriggerRule.CompeletedEvent(pid);
                
                if (mPropertyInfos.isArea && canCount)
                {
                    OnInteractiveUse(pid, true, player);
                }

                bool afterCanCount = CanUseByCount();
                mEntity.step = (afterCanCount) ? InteractiveTriggerStep.None : InteractiveTriggerStep.CannotUse;
                mEntity.canTrigger = afterCanCount;

                if (!afterCanCount)
                {
                    BroadcastAllPlayer();
                }

                mInstance.BroadcastEvent(this, "OnInteractive", null);
            }
        }

        private bool CanUseByCount()
        {
            return mCounter > 0 || mCounter == -1;
        }

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mEntity.mPropertyInfos.activeOnStartup = true;
            BroadcastAllPlayer();
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            mEntity.mPropertyInfos.activeOnStartup = false;
            BroadcastAllPlayer();
        }

        public void Reset (IServerEntity sender, object[] parameters = null)
        {
            mCounter = mPropertyInfos.counter;
            mEntity.canTrigger = true;
            mEntity.step = InteractiveTriggerStep.None;
            BroadcastAllPlayer();
        }
        #endregion

        private void BroadcastAllPlayer()
        {
            string canTriggerable = System.Convert.ToInt32(mEntity.canTrigger).ToString();
            string canActive = System.Convert.ToInt32(InteractiveTriggerRule.CanActiveGameObject(mEntity)).ToString();
            GameApplication.BroadcastInteractiveCount(mEntity.GetPersistentID().ToString(), canTriggerable,
                canActive, ((int)mEntity.step).ToString());
        }
    }

    public class InteractiveGate : NetEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public string entityName;
        public bool canTrigger;
        public InteractiveTriggerStep step = InteractiveTriggerStep.None;

        public InteractiveGate() : base()
        {
            this.EntityType = EntityType.InteractiveTrigger;
        }

        public void Init(InteractiveTrigger interactive)
        {
            mPropertyInfos = interactive.mPropertyInfos;
            mInstance = interactive.mInstance;
            entityName = mPropertyInfos.npcArchetype;
            canTrigger = true;
        }

        public void CancelAction()
        {
            this.mAction = null;
            this.mActionCmd = null;
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            CancelAction();

            if (string.IsNullOrEmpty(entityName))
                return;

            string path = StaticNPCRepo.GetModelPrefabPathByArchetype(entityName);
            if(string.IsNullOrEmpty(path))
            {
                path = ScenesModelRepo.GetScenesModelJson(entityName).modelprefabpath;
                if (string.IsNullOrEmpty(path))
                    return;
            }

            peer.ZRPC.CombatRPC.SpawnInteractiveEntity(mnPersistentID, path, mPropertyInfos.parentPath,
                Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }
        #endregion
    }
}

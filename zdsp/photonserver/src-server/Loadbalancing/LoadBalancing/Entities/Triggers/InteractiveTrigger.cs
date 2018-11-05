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
        private bool mActive = false;
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
            mActive = mPropertyInfos.activeOnStartup;
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
                    List<ItemInfo> consumeItem = new List<ItemInfo>() { new ItemInfo { itemId = (ushort)mPropertyInfos.keyId, stackCount = 1 } };
                    InvRetval result = player.mPlayer.Slot.mInventory.DeductItems(consumeItem, "OnInteractive");
                    if (result.retCode == InvReturnCode.UseFailed)
                    {
                        return;
                    }
                }

                mCounter = (mCounter == -1) ? -1 : mCounter - 1;
                mEntity.mPropertyInfos.counter = mCounter;
                InteractiveTriggerRule.CompeletedEvent(pid);

                if (mPropertyInfos.isArea && canCount)
                {
                    OnInteractiveUse(pid, true, player);
                }

                mEntity.step = (canCount) ? InteractiveTriggerStep.None : InteractiveTriggerStep.CannotUse;
                mEntity.mPropertyInfos.canTrigger = canCount;
                BroadcastAllPlayer(mEntity.GetPersistentID());
                object[] _paramters = { player };
                mInstance.BroadcastEvent(this, "OnInteractive", _paramters);
            }
        }

        private bool CanUseByCount()
        {
            return mCounter > 0 || mCounter == -1;
        }

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            mActive = false;
        }

        public void Reset (IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            mCounter = mPropertyInfos.counter;
            mEntity.mPropertyInfos.canTrigger = true;
            mEntity.step = InteractiveTriggerStep.None;
            BroadcastAllPlayer(mEntity.GetPersistentID());
        }
        #endregion

        private void BroadcastAllPlayer(int pid)
        {
            GameApplication.BroadcastInteractiveCount(pid, mEntity.mPropertyInfos.canTrigger,
                InteractiveTriggerRule.CanActiveGameObject(mEntity), (int)mEntity.step);
        }
    }

    public class InteractiveGate : NetEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public string entityName;
        public InteractiveTriggerStep step = InteractiveTriggerStep.None;

        public InteractiveGate() : base()
        {
            this.EntityType = EntityType.InteractiveTrigger;
        }

        public void Init(InteractiveTrigger interactive)
        {
            mPropertyInfos = interactive.mPropertyInfos;
            mInstance = interactive.mInstance;
            entityName = mPropertyInfos.archetype;
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            this.mActionCmd = null;

            string path = StaticNPCRepo.GetModelPrefabPathByArchetype(entityName);
            if(path == null || path == "")
            {
                path = ScenesModelRepo.GetScenesModelJson(entityName).modelprefabpath;
            }

            peer.ZRPC.CombatRPC.SpawnInteractiveEntity(mnPersistentID, path, mPropertyInfos.parentPath,
                Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }
        #endregion
    }
}

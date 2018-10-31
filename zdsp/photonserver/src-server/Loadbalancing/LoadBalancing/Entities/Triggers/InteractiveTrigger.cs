using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
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
        private int min = 0;
        private int max = 0;
        public InteractiveGate mEntity = null;
        public NetEntity netEntity = null;

        public InteractiveTrigger(InteractiveTriggerJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            min = info.min;
            max = info.max;
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {
            mActive = mPropertyInfos.activeOnStartup;
            mCounter = mPropertyInfos.counter;
            if (mActive)
                SpawnEntity();
        }

        void SpawnEntity()
        {
            if (mEntity != null) return;
            InteractiveGate entity = mInstance.mEntitySystem.SpawnNetEntity<InteractiveGate>();
            mInstance.mEntitySystem.AddAlwaysShow(entity);
            entity.Position = mPropertyInfos.position;
            entity.Forward = mPropertyInfos.forward;
            entity.Init(this);

            mEntity = entity;
            netEntity = mInstance.mEntitySystem.GetEntityByPID(mEntity.GetPersistentID()) as NetEntity;
        }

        public void OnInteractiveUse(int pid, bool enter, GameClientPeer player)
        {
            if (enter)
            {
                InteractiveTriggerRule.UseInteractiveTrigger(pid, player.mPlayer.Name, min, max,
                    netEntity, mPropertyInfos.interactiveTime, mCounter, mPropertyInfos.isArea);
            }
            else
            {
                InteractiveTriggerRule.InterruptedEvent(pid, player.mPlayer.Name);
                netEntity.StopAction();
            }
            object[] _paramters = { player };
            mInstance.BroadcastEvent(this, "OnInteractiveUse", _paramters);
        }

        public void OnInteractive(int pid, GameClientPeer player)
        {
            netEntity.StopAction();

            if (mCounter > 0 || mCounter == -1)
            {
                if (mPropertyInfos.keyId == 0)
                {
                    mCounter = (mCounter == -1) ? -1 : mCounter - 1;
                }
                else
                {
                    List<ItemInfo> consumeItem = new List<ItemInfo>() { new ItemInfo { itemId = (ushort)mPropertyInfos.keyId, stackCount = 1 } };
                    InvRetval result = player.mPlayer.Slot.mInventory.DeductItems(consumeItem, "OnInteractive");
                    if (result.retCode == InvReturnCode.UseSuccess)
                    {
                        mCounter = (mCounter == -1) ? -1 : mCounter - 1;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Write("Player not has item.");
                        return;
                    }
                }
                object[] _paramters = { player };
                mInstance.BroadcastEvent(this, "OnInteractive", _paramters);
                InteractiveTriggerRule.CompeletedEvent(pid);
                mEntity.ConsumeTrigger(mCounter, player);
                if (mPropertyInfos.isArea && (mCounter > 0 || mCounter == -1))
                {
                    OnInteractiveUse(pid, true, player);
                }
            }
        }

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            object[] paramters = { mActive };
            mInstance.BroadcastEvent(this, "TurnOn", paramters);
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            mActive = false;
            object[] paramters = { mActive };
            mInstance.BroadcastEvent(this, "TurnOff", paramters);
        }

        public void Reset (IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            mCounter = mPropertyInfos.counter;
            GameClientPeer mPlayer = parameters[0] as GameClientPeer;
            mEntity.ConsumeTrigger(mCounter, mPlayer);

            object[] paramters = { mActive, mCounter };
            mInstance.BroadcastEvent(this, "Reset", paramters);
        }
        #endregion
    }

    public class InteractiveGate : NetEntity
    {
        public InteractiveTriggerJson mPropertyInfos;

        public InteractiveGate() : base()
        {
            this.EntityType = EntityType.InteractiveTrigger;
        }

        public void Init(InteractiveTrigger interactive)
        {
            mPropertyInfos = interactive.mPropertyInfos;
            mInstance = interactive.mInstance;
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            string path = Repository.StaticNPCRepo.GetModelPrefabPathByArchetype(mPropertyInfos.archetype);
            if(path == null || path == "")
            {
                path = Repository.ScenesModelRepo.GetScenesModelJson(mPropertyInfos.archetype).modelprefabpath;
            }

            peer.ZRPC.CombatRPC.SpawnInteractiveEntity(mnPersistentID, path, mPropertyInfos.parentPath,
                Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }

        public void ConsumeTrigger (int count, GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.InteractiveTrigger(mnPersistentID, count, peer);
        }
        #endregion
    }
}

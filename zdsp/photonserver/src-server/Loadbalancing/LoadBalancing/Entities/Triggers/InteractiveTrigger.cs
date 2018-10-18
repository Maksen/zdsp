using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Common.Entities;
using Zealot.Common.RPC;

namespace Zealot.Server.Entities
{
    public class InteractiveTrigger : IServerEntity
    {
        public InteractiveTriggerJson mPropertyInfos;
        public GameLogic mInstance;
        private bool mActive = false;
        private int mCounter = 0;
        private bool interactiveArea = false;
        public InteractiveGate mEntity = null;

        public InteractiveTrigger(InteractiveTriggerJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
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
        }

        public void OnInteractiveUse(bool enter, GameClientPeer player)
        {
            if (mActive)
            {
                if (mCounter > 0)
                {
                    interactiveArea = enter;
                    player.mPlayer.InteractiveTriggerStats.waitResponse = interactiveArea;
                    object[] _paramters = { player.mPlayer };
                    mInstance.BroadcastEvent(this, "OnInteractiveUse", _paramters);
                }
                else
                {
                    interactiveArea = false;
                }
            }
            else
            {
                interactiveArea = false;
            }
            player.mPlayer.InteractiveTriggerStats.canTrigger = interactiveArea;
        }

        public void OnInteractive(GameClientPeer player, int keyId)
        {
            if (interactiveArea && mCounter > 0)
            {
                if (keyId == 0)
                {
                    mCounter--;
                    object[] _paramters = { player.mPlayer };
                    mInstance.BroadcastEvent(this, "OnInteractive", _paramters);
                }
                else
                {
                    List<ItemInfo> consumeItem = new List<ItemInfo>() {new ItemInfo { itemId = (ushort)keyId, stackCount = 1 }};
                    InvRetval result = player.mPlayer.Slot.mInventory.DeductItems(consumeItem, "OnInteractive");
                    if (result.retCode == InvReturnCode.UseSuccess)
                    {
                        mCounter--;
                        object[] _paramters = { player.mPlayer };
                        mInstance.BroadcastEvent(this, "OnInteractive", _paramters);
                    }
                }
            }
            player.mPlayer.InteractiveTriggerStats.waitResponse = false;
            player.mPlayer.InteractiveTriggerStats.canTrigger = false;
        }

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            mCounter = mPropertyInfos.counter;
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            mActive = false;
        }

        public void Reset (IServerEntity sender, object[] parameters = null)
        {
            mCounter = mPropertyInfos.counter;
        }
        #endregion

        public bool GetInteractive()
        {
            return interactiveArea;
        }
    }

    public class InteractiveGate : BaseNetEntity
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
            peer.ZRPC.CombatRPC.InteractiveEntity(mnPersistentID, path, mPropertyInfos.parentPath,
                Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }
        #endregion
    }
}

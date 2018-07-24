using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;

namespace Zealot.Server.Entities
{
    public class ObjectSpawner : IServerEntity
    {
        public ObjectSpawnerJson mPropertyInfos;
        public GameLogic mInstance;
        public AnimationObject mChild;

        public ObjectSpawner(ObjectSpawnerJson info, GameLogic instance)
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
            if (mPropertyInfos.activeOnStartup && mPropertyInfos.prefab != "")
                SpawnChild(mPropertyInfos.prefab);
        }

        public void SpawnChild(string modelprefab)
        {
            AnimationObject animationobject = mInstance.mEntitySystem.SpawnNetEntity<AnimationObject>();
            animationobject.Position = mPropertyInfos.position;
            animationobject.Forward = mPropertyInfos.forward;
            animationobject.Init(this, modelprefab);
            mChild = animationobject;
        }

        #region Triggers
        public void TriggerSpawn(IServerEntity sender, object[] parameters = null)
        {
            string new_modelprefab = mPropertyInfos.prefab;
            if (mChild == null && !string.IsNullOrEmpty(new_modelprefab))
                SpawnChild(new_modelprefab);               
        }

        public void DestoryAll(IServerEntity sender, object[] parameters = null)
        {
            if (mChild != null)
            {
                mInstance.mEntitySystem.RemoveEntityByPID(mChild.GetPersistentID());
                mChild = null;
            }
        }
        #endregion
    }

    public class AnimationObject : StaticNetEntity
    {
        public ObjectSpawnerJson mPropertyInfos;
        public string mModelPrefab;

        public AnimationObject() : base()
        {
            this.EntityType = EntityType.AnimationObject;
        }

        public void Init(ObjectSpawner spawner, string modelprefab)
        {
            mPropertyInfos = spawner.mPropertyInfos;
            mModelPrefab = modelprefab;
            mInstance = spawner.mInstance;
            GotoState("Active");
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.SpawnAnimationObject(mnPersistentID, mModelPrefab, Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }
        #endregion
    }
}

using Zealot.Entities;
using Zealot.Common.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common.RPC;

namespace Zealot.Server.Entities
{
    public class GateSpawner : IServerEntity
    {
        public GateSpawnerJson mPropertyInfos;
        public GameLogic mInstance;
        public Gate mChild = null;

        public GateSpawner(GateSpawnerJson info, GameLogic instance)
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
            if (mPropertyInfos.activeOnStartup)
                SpawnGate();            
        }

        private void SpawnGate()
        {
            if (mChild != null)
                return;
            Gate gate = mInstance.mEntitySystem.SpawnNetEntity<Gate>();
            mInstance.mEntitySystem.AddAlwaysShow(gate);
            gate.Position = mPropertyInfos.position;
            gate.Forward = mPropertyInfos.forward;
            gate.Init(this);
            mChild = gate;
        }

        #region Triggers
        public void Open(IServerEntity sender, object[] parameters = null)
        {
            if (mChild != null)
            {
                mInstance.mEntitySystem.RemoveAlwaysShow(mChild);
                mInstance.mEntitySystem.RemoveEntityByPID(mChild.GetPersistentID());
                mChild = null;
            }
        }

        public void Close(IServerEntity sender, object[] parameters = null)
        {
            SpawnGate();
        }
        #endregion
    }

    public class Gate : BaseNetEntity
    {
        public GateSpawnerJson mPropertyInfos;

        public Gate() : base()
        {
            this.EntityType = EntityType.Gate;
        }

        public void Init(GateSpawner spawner)
        {
            mPropertyInfos = spawner.mPropertyInfos;
            mInstance = spawner.mInstance;
        }

        #region Implement abstract methods
        public override void SpawnAtClient(GameClientPeer peer)
        {
            peer.ZRPC.CombatRPC.SpawnGate(mnPersistentID, mPropertyInfos.width, mPropertyInfos.height,
                mPropertyInfos.prefab, Position.ToRPCPosition(), Forward.ToRPCDirection(), peer);
        }
        #endregion
    }
}

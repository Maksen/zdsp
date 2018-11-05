using Photon.LoadBalancing.GameServer;
using Zealot.Entities;

namespace Zealot.Server.Entities
{
    public class CutsceneBroadcaster : IServerEntity
    {
        public CutsceneBroadcasterJson mPropertyInfos;
        public GameLogic mInstance;

        public CutsceneBroadcaster(CutsceneBroadcasterJson info, GameLogic instance)
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
        }

        public void OnCutsceneFinished()
        {
            mInstance.BroadcastEvent(this, "OnCutsceneFinished");
        }

        #region Triggers
        public void Play(IServerEntity sender, object[] parameters = null)
        {
            mInstance.ZRPC.CombatRPC.BroadcastCutScene(mPropertyInfos.ObjectID, mInstance.mRoom);
        }
        #endregion
    }
}

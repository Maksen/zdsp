using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class CutSceneBroadcaster : IServerEntity
    {
        public CutSceneBroadcasterJson mPropertyInfos;
        public GameLogic mInstance;

        public CutSceneBroadcaster(CutSceneBroadcasterJson info, GameLogic instance)
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

        #region Triggers
        public void Play(IServerEntity sender, object[] parameters = null)
        {
            mInstance.ZRPC.CombatRPC.BroadcastCutScene(mPropertyInfos.ObjectID, mInstance.mRoom);
        }
        #endregion
    }
}

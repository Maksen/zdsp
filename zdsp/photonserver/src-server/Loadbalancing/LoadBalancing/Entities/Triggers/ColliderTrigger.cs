using Photon.LoadBalancing.GameServer;
using Zealot.Entities;

namespace Zealot.Server.Entities
{
    public class ColliderTrigger : IServerEntity
    {
        public ColliderTriggerJson mPropertyInfos;
        public GameLogic mInstance;
        private bool mActive = false;
        private int mCount = 0;

        public ColliderTrigger(ColliderTriggerJson info, GameLogic instance)
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
        }

        public void OnEnter(Player player)
        {
            if (mActive && (mCount < mPropertyInfos.count || mPropertyInfos.count < 0))
            {
                if (mPropertyInfos.count > 0)
                    ++mCount;
                object[] _paramters = { player };
                mInstance.BroadcastEvent(this, "OnEnter", _paramters);
            }
        }

        public void OnLeave(Player player)
        {
            if (mActive)
            {
                object[] _paramters = { player };
                mInstance.BroadcastEvent(this, "OnLeave", _paramters);
            }
        }

        #region Triggers
        public void On(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            mCount = 0;
        }

        public void Off(IServerEntity sender, object[] parameters = null)
        {
            mActive = false;
        }

        public void ResetCount(IServerEntity sender, object[] parameters = null)
        {
            mCount = 0;
        }
        #endregion
    }
}

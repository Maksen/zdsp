using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class Counter : IServerEntity
    {
        public CounterJson mPropertyInfos;
        public GameLogic mInstance;
        private bool mActive = false;
        private int mCount = 0;

        public Counter(CounterJson info, GameLogic instance)
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

        #region Triggers
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
            mCount = 0;
        }

        public void Reset(IServerEntity sender, object[] parameters = null)
        {
            mCount = 0;
        }

        public void Increase(IServerEntity sender, object[] parameters = null)
        {
            if(mActive)
            {
                mCount++;
                if(mCount == mPropertyInfos.count)
                {
                    mCount = 0;
                    mInstance.BroadcastEvent(this, "OnCounter");
                }
            }
        }

        public void Decrease(IServerEntity sender, object[] parameters = null)
        {
            if (mActive)
            {
                if (mCount > 0)
                    mCount--;
            }
        }
        #endregion
    }
}

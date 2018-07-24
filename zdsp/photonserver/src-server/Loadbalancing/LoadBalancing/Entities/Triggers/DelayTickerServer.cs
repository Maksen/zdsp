using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class DelayTickerServer : IServerEntity
    {
        public DelayTickerJson mPropertyInfos;
        public GameLogic mInstance;
        private GameTimer timer;

        public DelayTickerServer(DelayTickerJson info, GameLogic instance)
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
                Start();  
        }

        public void Start()
        {
            if (mPropertyInfos.delay > 0)
                timer = mInstance.SetTimer(mPropertyInfos.delay, OnDelayTimeUp, null); 
            else if (mPropertyInfos.ticker > 0)
                timer = mInstance.SetTimer(mPropertyInfos.ticker, OnTickerTimeUp, null);
        }

        private void OnDelayTimeUp(object parameters = null)
        {
            mInstance.BroadcastEvent(this, "OnDelay");
            if (mPropertyInfos.ticker > 0)
                timer = mInstance.SetTimer(mPropertyInfos.ticker, OnTickerTimeUp, null);
            else
                timer = null;
        }

        private void OnTickerTimeUp(object parameters = null)
        {
            mInstance.BroadcastEvent(this, "OnTicker");
            timer = mInstance.SetTimer(mPropertyInfos.ticker, OnTickerTimeUp, null);
        }

        #region trigger
        public void TurnOn(IServerEntity sender, object[] parameters = null)
        {
            if (timer == null)
                Start();
        }

        public void TurnOff(IServerEntity sender, object[] parameters = null)
        {
            if (timer != null)
            {
                mInstance.StopTimer(timer);
                timer = null;
            }
        }
        #endregion
    }
}

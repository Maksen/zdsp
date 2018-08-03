using System;
using System.Collections.Generic;
using Zealot.Entities;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public class RandomTrigger : IServerEntity
    {
        public RandomTriggerJson mPropertyInfos;
        public GameLogic mInstance;
        private bool mActive = false;
        private List<int> mEventList = new List<int>();

        public RandomTrigger(RandomTriggerJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            ResetLoop();
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {
            mActive = mPropertyInfos.activeOnStartup; 
        }

        private void ResetLoop()
        {
            if (mPropertyInfos.loop)
            {
                mEventList = new List<int>();
                for (int index = 1; index <= mPropertyInfos.size; index++)
                    mEventList.Add(index);
            }
        }

        #region Triggers
        public void On(IServerEntity sender, object[] parameters = null)
        {
            mActive = true;
        }

        public void Off(IServerEntity sender, object[] parameters = null)
        {
            mActive = false;
        }

        public void Random(IServerEntity sender, object[] parameters = null)
        {
            if (!mActive || mPropertyInfos.size == 0)
                return;
            if (mPropertyInfos.loop)
            {
                int count = mEventList.Count;
                if (count > 0)
                {
                    int randIndex = GameUtils.RandomInt(0, count - 1);
                    int selectedEvent = mEventList[randIndex];
                    mEventList.RemoveAt(randIndex);
                    count--;
                    if (count == 0)
                        ResetLoop();
                    mInstance.BroadcastEvent(this, "Event" + selectedEvent, parameters);
                }
            }
            else
            {
                int selectedEvent = GameUtils.RandomInt(1, mPropertyInfos.size);
                mInstance.BroadcastEvent(this, "Event" + selectedEvent, parameters);
            }
        }

        public void Reset(IServerEntity sender, object[] parameters = null)
        {
            ResetLoop();
        }
        #endregion
    }
}

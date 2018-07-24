using System;
using System.Collections.Generic;
using Photon.LoadBalancing.GameServer;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Common.Entities;
using Zealot.Server.Rules;

namespace Zealot.Server.Entities
{
    /*public class PortalEntryServer : IServerEntity
    {
        public PortalEntryJson mPropertyInfos;
        public GameLogic mInstance;
        private GameTimer timer;
        private long mQueryInterval = 1000;
        private Dictionary<int, bool> mPlayersInside;
        private LocationData mPortalExitData;

        public PortalEntryServer(PortalEntryJson info, GameLogic instance)
        {
            mPropertyInfos = info;
            mInstance = instance;
            mPlayersInside = new Dictionary<int, bool>();
            PortalInfos.mExits.TryGetValue(mPropertyInfos.exitName, out mPortalExitData);
        }

        public ServerEntityJson GetPropertyInfos()
        {
            return mPropertyInfos;
        }

        public void InstanceStartUp()
        {
            if (mPropertyInfos.activeOnStartup && mPortalExitData != null)
                Start();
        }

        public void Start()
        {
            QueryPlayer();
        }

        public void QueryPlayer(object parameters = null)
        {
            List<IActor> targets = null;
            switch (mPropertyInfos.detectionArea.mType)
            {
                case AreaType.Sphere:
                    float radius = mPropertyInfos.detectionArea.mRadius;
                    if(radius <= 0)
                        return;
                    targets = mInstance.mEntitySystem.QueryActorsInSphere(mPropertyInfos.position, radius, QueryActorFilter);
                    break;
                //case AreaType.Box:

            }
            if(targets != null)
            {
                Dictionary<int, bool> now_PlayersInside = new Dictionary<int, bool>();
                if (targets.Count > 0)
                {
                    foreach (IActor target in targets)
                    {
                        Player player = (Player)target;
                        int pid = player.GetPersistentID();
                        if (!mPlayersInside.ContainsKey(pid))
                            GameRules.TeleportToPortalExit(player, mPropertyInfos.exitName);                            
                        now_PlayersInside.Add(pid, true);
                    }
                }
                mPlayersInside = now_PlayersInside;
            }
            timer = mInstance.SetTimer(mQueryInterval, QueryPlayer, null);       
        }

        public bool QueryActorFilter(IActor actor)
        {
            Zealot.Server.Entities.Player target = actor as Zealot.Server.Entities.Player;
            if (target != null)
                return target.IsAlive();
            return false;
        }
    }*/
}

namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;
    using ExitGames.Logging;

    public class ServerAuthoASDead : Action
    {           
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public ServerAuthoASDead(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //Just do nothing at server
            NetEntity ne = (NetEntity) mEntity;
            log.DebugFormat("NetEntity {0} activeenter ServerAuthoASIdle", ne.GetPersistentID());

            DeadActionCommand cmd = new DeadActionCommand();
            ne.SetAction(cmd);
        }
    }
}

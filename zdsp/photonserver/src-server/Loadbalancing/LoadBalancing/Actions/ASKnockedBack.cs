namespace Zealot.Server.Actions
{
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    using ExitGames.Logging;
    using UnityEngine;

    public class ServerAuthoKnockedBack : Action //Used for entities owned by server e.g. monster entity
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private Vector3 mTargetPos;
        private float dist;

        public ServerAuthoKnockedBack(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        public override void Start()
        {
            KnockedBackCommand cmd = (KnockedBackCommand)mdbCommand;
            
            mTargetPos = cmd.targetpos;
            dist = Vector3.Magnitude(mTargetPos - mEntity.Position);

            //mRangeSqr = cmd.range * cmd.range;
            base.Start();
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //Just do nothing at server
            NetEntity ne = (NetEntity) mEntity;
            log.DebugFormat("NetEntity {0} activeenter ServerAuthoKnockedBack", ne.GetPersistentID());

             
            ne.SetAction(mdbCommand);
        }

        protected override void OnActiveUpdate(long dt)
        {
            //base.OnActiveUpdate(dt);
            Actor entity = (Actor)mEntity;
            //Vector3 targetPos = ((KnockedBackCommand)mdbCommand).targetpos;
            Vector3 forward = mTargetPos - entity.Position;
            forward.y = 0;

            if (forward.sqrMagnitude <=  0.01)
            {
                //entity.Position = mTargetPos;
                GotoState("Completed");
                return;
            }

            forward.Normalize();
            //entity.Forward = forward;
            float moveSpeed = 8f;
            entity.Position = Vector3.MoveTowards(entity.Position, mTargetPos , moveSpeed * dt / 1000.0f);
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            base.OnCompleteEnter(prevstate);
        }

    }

     
}

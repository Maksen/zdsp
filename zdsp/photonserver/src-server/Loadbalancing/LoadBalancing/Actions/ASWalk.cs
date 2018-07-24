namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    public class ServerAuthoASWalk : Action
    {
        private const float Epsilon = 0.04f;        

        public ServerAuthoASWalk(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {            
            NetEntity entity = (NetEntity)mEntity;
            entity.SetAction(mdbCommand);

            Vector3 targetPos = ((WalkActionCommand)mdbCommand).targetPos;
            Vector3 forward = targetPos - entity.Position;
            forward.y = 0;
            forward.Normalize();
            entity.Forward = forward;
        }
        
        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            Actor entity = (Actor)mEntity;
            Vector3 targetPos = ((WalkActionCommand)mdbCommand).targetPos;
            Vector3 forward = targetPos - entity.Position;
            forward.y = 0;

            if (forward.sqrMagnitude <= Epsilon)
            {
                entity.Position = targetPos;
                GotoState("Completed");
                return;
            }

            float moveSpeed = ((WalkActionCommand)mdbCommand).speed;
            if (moveSpeed == 0)
                moveSpeed = entity.PlayerStats.MoveSpeed;
            entity.Position = Vector3.MoveTowards(entity.Position, targetPos, moveSpeed * dt / 1000.0f);
        }
    }
}

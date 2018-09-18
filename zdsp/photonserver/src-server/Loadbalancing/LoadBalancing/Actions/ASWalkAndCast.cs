using UnityEngine;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;

namespace Zealot.Server.Actions
{
    //Only player will use this action
    public class NonServerAuthoWalkAndCast : BaseServerCastSkill
    {
        public NonServerAuthoWalkAndCast(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            if (IsSkillUsable())
            {
                base.OnActiveEnter(prevstate);
            }
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

            ((NetEntity)mEntity).ClearAction();
        }
    }

    public class ServerAuthoWalkAndCast : BaseServerCastSkill
    {
        private const float Epsilon = 0.04f;

        public ServerAuthoWalkAndCast(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);

            Actor entity = (Actor)mEntity;
            entity.SetAction(mdbCommand);

            //skill is always usable for server authoritative actions
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);

            //-----------------------------------------------
            //Movement here if there is
            Actor actor = (Actor)mEntity;
            if (actor.HasControlStatus(ControlSEType.Root))
                return;

            Vector3 targetPos = ((WalkAndCastCommand)mdbCommand).targetPos;
            Vector3 forward = targetPos - actor.Position;
            forward.y = 0;

            if (forward.sqrMagnitude <= Epsilon)
            {
                actor.Position = targetPos;
                return;
            }

            float moveSpeed = actor.PlayerStats.MoveSpeed;
            actor.Position = Vector3.MoveTowards(actor.Position, targetPos, moveSpeed * dt / 1000.0f);
            actor.Forward = forward.normalized;
        }

        public void WalkToNewPos(Vector3 pos)
        {
            Actor actor = (Actor)mEntity;
            WalkAndCastCommand cmd = (WalkAndCastCommand)mdbCommand;
            cmd.targetPos = pos;
            actor.SetAction(mdbCommand);
        }
    }
}
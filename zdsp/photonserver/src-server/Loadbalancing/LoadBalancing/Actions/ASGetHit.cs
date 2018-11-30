namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    public class BaseServerGetHit : Action
    {
        public BaseServerGetHit(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        public override void Start()
        {
            base.Start();
        }

        protected override void OnActiveEnter(string prevstate)
        {
            Actor ghost = (Actor)mEntity;
            if (!ghost.PlayerStats.HeavyStand)
            {
                ghost.OnGetHit(200);
                SetTimer(200, OnActiveTimeUp, null);
            }
            else
            {
                GotoState("Completed");
            }
        }

        protected virtual void OnActiveTimeUp(object arg)
        {
            GotoState("Completed");
        }

        public override bool Update(Action newAction)
        {
            if (newAction.mdbCommand.GetActionType() == mdbCommand.GetActionType())
            {
                mdbCommand = newAction.mdbCommand;
                GotoState("Active");
                return true;
            }
            return false;
        }
    }

    public class NonServerAuthoGethit : BaseServerGetHit
    {
        public NonServerAuthoGethit(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }
    }

    public class ServerAuthoGethit : BaseServerGetHit
    {
        public ServerAuthoGethit(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {

        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);

            Actor entity = (Actor)mEntity;
            entity.SetAction(mdbCommand);
        }
    }
}

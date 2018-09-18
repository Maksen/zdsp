using UnityEngine;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;

namespace Zealot.Server.Actions
{

    class ServerAuthoFrozen : Action
    {
        private float duration;

        public ServerAuthoFrozen(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            duration = ((FrozenActionCommand)cmd).dur;
        }

        public override void Start()
        {
            base.Start();
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            Actor actor = (Actor)mEntity;
            actor.SetAction(mdbCommand);
            SetTimer((long)duration, OnTimesUp, null);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
        }

        protected virtual void OnTimesUp(object args)
        {
            GotoState("Completed");
        }
    }
}

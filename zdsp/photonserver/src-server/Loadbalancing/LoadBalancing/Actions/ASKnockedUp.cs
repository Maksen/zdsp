

namespace Zealot.Server.Actions
{
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    class KnockedUpBase : Action
    {

        public KnockedUpBase(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            long dur = (long)((KnockedUpCommand)mdbCommand).dur;
            SetTimer(dur * 1000, OnFinishedDuration, null);
        }

        private void OnFinishedDuration(object arg)
        {
            GotoState("Completed");
        }
    }

    class ServerAuthoKnockedUp :KnockedUpBase
    {
        public ServerAuthoKnockedUp(Entity entity, ActionCommand cmd) :base(entity, cmd)
        {
            
        }

        public override void Start()
        {
             base.Start();             
        }
        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            NetEntity ne = (NetEntity)mEntity;
            ne.SetAction(mdbCommand);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            Actor actor = (Actor)mEntity;
            if (actor.HasControlStatus(ControlSEType.Root))//Root entity can not be KnockedUp
            {
                GotoState("complete");
            } 
        }
    }
}

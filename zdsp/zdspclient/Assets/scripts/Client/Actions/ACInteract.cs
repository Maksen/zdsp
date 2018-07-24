using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Common.Actions;

namespace Zealot.Client.Actions
{
	public class BaseClientInteract : Action
	{
        protected Entity mTarget;

        public BaseClientInteract(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
        }

        public virtual void Init()
        {
            InteractCommand cmd = (InteractCommand)mdbCommand;
            mTarget = mEntity.EntitySystem.GetEntityByPID(cmd.targetpid);
        }
		
		public void FaceTarget()
		{
            Vector3 direction = mTarget.Position - mEntity.Position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001)
                return;
            direction.Normalize();
            ((NetEntityGhost)mEntity).Forward = direction;
		}

		protected override void OnActiveEnter(string prevstate)
		{           
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
			ghost.PlayEffect("gather");
			SetTimer (((InteractCommand)mdbCommand).chargetime, OnActiveTimeUp, null);
		}

		protected virtual void OnActiveTimeUp(object arg)
		{
			GotoState("Completed");
		}
	}
	
	public class ClientAuthoACInteract : BaseClientInteract //Responsible for Setting Action
    {
		public ClientAuthoACInteract(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}

        public override void Start()
        {
            Init();
            if (mTarget == null || mTarget.Destroyed)
                GotoState("Completed");
            else
            {
                FaceTarget();
                base.Start();
            }
        }

        protected override void OnActiveEnter(string prevstate)
		{
			base.OnActiveEnter(prevstate);
            float seconds = ((InteractCommand)mdbCommand).chargetime / 1000.0f;
            //HUD.Combat.ChargeBar.Init(seconds, ClientUtils.LoadIcon("UI_Icons/Skill/Buff/ChargeBar_Icon1.png"));
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
			ghost.SetAction (mdbCommand);
		}

        protected override void OnActiveLeave()
        {
            //HUD.Combat.ChargeBar.Hide();
        }

        protected override void OnCompleteEnter(string prevstate)
		{			
			RPCFactory.CombatRPC.OnPickResource(((InteractCommand)mdbCommand).targetpid);

            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.PerformAction(new ClientAuthoACIdle(ghost, new IdleActionCommand()));
        }
	}
	
	public class NonClientAuthoACInteract : BaseClientInteract
    {
		public NonClientAuthoACInteract(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}

        public override void Start()
        {
            Init();
            if (mTarget != null)
                FaceTarget();
            base.Start();
        }
    }
}

using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
	using Zealot.Common.Actions;

    public class ClientAuthoDragged : Action
    {
        private Vector3 mTargetPos;
        private float dur;
        private float speed;
        public ClientAuthoDragged(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            DraggedActionCommand cmd = mdbCommand as DraggedActionCommand;
            mTargetPos = cmd.pos;
            dur = cmd.dur;
            speed = cmd.speed;
            ActorGhost ghost = (ActorGhost)mEntity;
            ghost.SetAction(mdbCommand);
            ghost.PlayEffect("standby");
                
            SetTimer((long)(1000*dur), (object args) => {
                GotoState("Completed");
            }, null);
        }
        
        protected override void OnTerminatedEnter(string prevstate)
        {
            base.OnTerminatedEnter(prevstate);
            FinishDrag();//handle interrupted.
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            FinishDrag(); 
        }

        private void FinishDrag()
        {            
            ActorGhost ghost = (ActorGhost)mEntity;
            //if (UIManager.UIHudFX.HudFilterAnim3 != null)
                //UIManager.UIHudFX.HudFilterAnim3.Play("ActionLines_FadeOut");
                //ghost.StopAnimFilter(UIManager.UIHudFX.HudFilter3, UIManager.UIHudFX.HudFilterAnim3);

        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            Vector3 forward = mTargetPos - mEntity.Position;
            float distToTarget = forward.magnitude;
            if (distToTarget < 0.01f)
            { 
                return;
            }
            forward.Normalize();
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, speed, dt / 1000.0f);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Move(motion);
        }
    }




    public class NonClientAuthoDragged : Action
    { 
        private float speed = 0;
        private float dur = 0;
        private Vector3 mTargetPos;

        public NonClientAuthoDragged(Entity entity, ActionCommand cmd)
            : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            DraggedActionCommand cmd = mdbCommand as DraggedActionCommand;
            mTargetPos = cmd.pos;
            speed = cmd.speed;
            dur = cmd.dur;//this will not used by non-authoclient
            NetEntityGhost ghost = (NetEntityGhost)mEntity;

            ghost.PlayEffect("standby");
            Debug.Log("NonClientAuthoDragged started.");

        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            Vector3 forward = mTargetPos - mEntity.Position;
            float distToTarget = forward.magnitude;
            if (distToTarget < 0.01f)
            {
                return;
            }
            forward.Normalize();
            Vector3 motion = ClientUtils.MoveTowards(forward, distToTarget, speed, dt / 1000.0f);
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.Move(motion);

        }
    }
}

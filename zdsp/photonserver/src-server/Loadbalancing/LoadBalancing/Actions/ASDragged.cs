
using UnityEngine;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;

namespace Zealot.Server.Actions
{
    
    class ASDragged: Action
    {
        private Vector3 mTargetPos;
        private float dur = 1.0f;
        private float speed = 10f;
        public ASDragged(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {

        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            DraggedActionCommand cmd = mdbCommand as DraggedActionCommand;
            dur = cmd.dur; 
            speed = cmd.speed; 
            mTargetPos = cmd.pos;

            NetEntity netent = mEntity as NetEntity;
            netent.SetAction(mdbCommand);
            
           
            SetTimer((long)(1000*dur), (object args) => { 
                //mEntity.Position = caster.Position;
                GotoState("Completed");
            }, null);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
            Actor entity = (Actor)mEntity;
            Vector3 forward = mTargetPos - entity.Position;
            forward.y = 0;

            if (forward.sqrMagnitude <= 0.01)
            {
                //entity.Position = mTargetPos;
               // GotoState("Completed");
                return;
            }

            forward.Normalize(); 
            entity.Position = Vector3.MoveTowards(entity.Position, mTargetPos, speed * dt / 1000.0f);
        }
    }
     
}

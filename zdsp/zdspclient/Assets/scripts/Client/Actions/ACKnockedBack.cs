using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
	using Zealot.Common.Actions;

	public class BaseKnockedBackWalk : Zealot.Common.Actions.Action // base for all client actions
	{ 
        protected bool mNewStart = true;
        private Vector3 targetPos;        
        private Vector3 startDir;
        protected Vector3 mDesiredForward;
        private float dist; 

        public BaseKnockedBackWalk(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}
        
		protected override void OnActiveEnter(string prevstate)
		{
			//base.OnActiveEnter(prevstate);   
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
            
            if (mNewStart) {
                //Debug.Log ("NetEntityGhost [" + ghost.GetPersistentID () + "] animate walk");			                
                //ghost.PlayEffect("hit");//no need to play a new effect.
            }
            targetPos = ((KnockedBackCommand)mdbCommand).targetpos;
            Vector3 forward = targetPos - mEntity.Position; 

            forward.y = 0;
            dist = forward.magnitude;
            if (forward.sqrMagnitude < 0.05f)
                forward = ghost.Forward;
            forward.Normalize();            
            mDesiredForward = forward;
            startDir = forward;
        }

		protected override void OnActiveUpdate(long dt) //dt in msec
		{
			ActorGhost ghost = (ActorGhost)mEntity;

            Vector3 forward = targetPos - ghost.Position;            
            forward.y = 0;
			float distToTarget = forward.magnitude;

            forward = forward / distToTarget; //normalize            
            if (distToTarget <= 0.01 )  
            {
                //if (dotproduct < 0.0f)
                //    Debug.LogFormat("distToTarget = {0} dotproduct = {1}", distToTarget, dotproduct);

                ghost.Position = ((KnockedBackCommand)mdbCommand).targetpos;                
                GotoState("Completed");
                return;
            }
            mDesiredForward = forward;    
            float moveSpeed = 8 ; //the dist control how fast is the move, so the duration is constant 
            Vector3 motion = ClientUtils.MoveTowards (forward, distToTarget, moveSpeed, dt / 1000.0f);
            ghost.Move(motion);
            //ghost.Position = Vector3.MoveTowards(ghost.Position, targetPos, moveSpeed * dt / 1000.0f);
        }


        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

             
        }
    }
     
	
	public class NonClientAuthoKnockedBackWalk : BaseKnockedBackWalk
	{
        

        public NonClientAuthoKnockedBackWalk(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
             
        }

		protected override void OnCompleteEnter(string prevstate)
		{            
			//NetEntityGhost ghost = (NetEntityGhost)mEntity;        
            //IdleActionCommand cmd = new IdleActionCommand ();
			//ghost.PerformAction (new NonClientAuthoACIdle(ghost, cmd));

            base.OnCompleteEnter(prevstate);            
        }                

        protected override void OnActiveUpdate(long dt) //dt in msec
        {
            base.OnActiveUpdate(dt);
            
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
        }
    }

    
}

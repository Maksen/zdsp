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

	public class BaseKnockedUp : Zealot.Common.Actions.Action // base for all client actions
	{
        protected string eff = ""; 

        public BaseKnockedUp(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}
        
		protected override void OnActiveEnter(string prevstate)
		{
            //base.OnActiveEnter(prevstate);
            ActorGhost ghost = (ActorGhost)mEntity;
            //eff = "Effects_generic/prefab/stun.prefab";
            //var dur = ((KnockedUpCommand)mdbCommand).dur; 
            ghost.PlayAnimation("knockup1", -1);
        }

		protected override void OnActiveUpdate(long dt) //dt in msec
		{
			 
        }


        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            ActorGhost ghost = (ActorGhost)mEntity; 
            //the duration of the animation should always be same as .
            ghost.PlayAnimation("cbt_standby", -1);
        }
    }
     
	
	public class NonClientAuthoKnockedUp : BaseKnockedUp
	{
        

        public NonClientAuthoKnockedUp(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
             
        }

		protected override void OnCompleteEnter(string prevstate)
		{             

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

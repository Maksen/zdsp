using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Common.Actions;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
    public class BaseClientIdle : AIdle
    {
		public BaseClientIdle(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //ActorGhost ghost = (ActorGhost)mEntity;
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            //Debug.Log ("NetEntityGhost [" + ghost.GetPersistentID() + "] animate idle");
            
            string anim = ghost.GetStandbyAnimation();
            if(anim!="")
                ghost.PlayEffect(anim);
        }
    }

	public class ClientAuthoACIdle : BaseClientIdle  //Responsible for Setting Action
    {
		public ClientAuthoACIdle(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //Responsible for Setting Action
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
			ghost.SetAction (mdbCommand);
        }
	}

    public class NonClientAuthoACIdle : BaseClientIdle
	{
		public NonClientAuthoACIdle(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}
	}
}

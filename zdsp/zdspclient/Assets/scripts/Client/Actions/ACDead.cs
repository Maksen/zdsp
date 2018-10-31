using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Common.Actions;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
	public class BaseClientDead : Zealot.Common.Actions.Action
	{
        public BaseClientDead(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{           
        }
		
		protected override void OnActiveEnter(string prevstate)
		{
			base.OnActiveEnter(prevstate);            
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
			//Debug.Log ("NetEntityGhost [" + ghost.GetPersistentID() + "] animate dead");
            if (ghost.IsActor())
                ghost.OnGhostDie();
             
			ghost.PlayEffect(ghost.GetWeaponExtension() + "dying", ghost.GetDyingEffect());

            if (((ActorGhost)ghost).HasControlStatus(EffectVisualTypes.Frozen))
            {
                ((ActorGhost)ghost).HandleSideEffectVisuals("VisualEffectTypes", 0);
                ghost.PlayEffect("", "Frozen_Death");
            }

            long duration = ghost.GetDyingDuration(); //1 sec for dying animation  
            SetTimer(duration, OnDyingFinished, null); 
        }

        private void OnDyingFinished(object arg)
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.PlayEffect(ghost.GetWeaponExtension() + "dead");

            if (ghost.IsMonster())
            {               
                ghost.ShowShadow(false);
                Dissolve dissolve = ghost.AnimObj.AddComponent<Dissolve>();
                dissolve.Init(ghost.AnimObj.GetComponentInChildren<SkinnedMeshRenderer>(true));
            }
        }        
    }
	
	public class ClientAuthoACDead : BaseClientDead  //Responsible for Setting Action
	{
		public ClientAuthoACDead(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}
		
		protected override void OnActiveEnter(string prevstate)
		{
			base.OnActiveEnter(prevstate);			
			NetEntityGhost ghost = (NetEntityGhost)mEntity;
			ghost.SetAction (mdbCommand);
            GameInfo.gLocalPlayer.OnDead();
		}      
    }
	
	public class NonClientAuthoACDead : BaseClientDead
	{
		public NonClientAuthoACDead(Entity entity, ActionCommand cmd): base(entity, cmd)  
		{
		}
	}
}


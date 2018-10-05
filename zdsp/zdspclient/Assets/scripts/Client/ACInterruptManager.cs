using System;
using System.Collections.Generic;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using UnityEngine;

namespace Zealot.Client.Actions
{
    using Zealot.Common.Actions;

    public static class InterruptFn
	{
		public static bool AlwaysInterrupt(Action current, Action newAction)
		{
			return true;
		}

		public static bool NoInterrupt(Action current, Action newAction)
		{
			return false;
		}

		public static bool AfterComplete(Action current, Action newAction)
		{
			return current.IsCompleted ();
		}

        public static bool SkillInterruptSkill(Action current, Action newAction)
        {
            
            //if basicattack, may be able to interrupt early            
            ISkillCastCommandCommon currSkillCmd = (ISkillCastCommandCommon)current.mdbCommand;
            ISkillCastCommandCommon newSkillCmd = (ISkillCastCommandCommon)newAction.mdbCommand;
            int currskillid = currSkillCmd.GetSkillID();
            int newskillid = newSkillCmd.GetSkillID();
            SkillData currskill = SkillRepo.GetSkill(currskillid);
            SkillData newskill = SkillRepo.GetSkill(newskillid);
            if (currskill == null || newskill == null)
                return false;
            if(currskill.skillgroupJson.skilltype == Common.SkillType.BasicAttack && 
                newskill.skillgroupJson.skilltype == Common.SkillType.Active)
                return true; //skill will interrrupt basic atttack
             else if (currskill.skillgroupJson.skilltype == Common.SkillType.Active &&
                newskill.skillgroupJson.skilltype == Common.SkillType.BasicAttack)
            {
                return current.IsCompleted();//basic attack afte skill complete
            }
            //SkillGroupJson skillgroup = currskill.skillgroupJson;
            //if (skillgroup.interruptable)
            //{
            //    return true;
            //}

            return false;
        }       

        public static bool MoveInterruptSkill(Action current, Action newAction)
        {
            ISkillCastCommandCommon currSkillCmd = (ISkillCastCommandCommon)current.mdbCommand;
            SkillData skilldata = SkillRepo.GetSkill(currSkillCmd.GetSkillID());
            SkillGroupJson skillgroup = skilldata.skillgroupJson;
            if (skillgroup.skilltype == Common.SkillType.BasicAttack)
                return true;
            if (skillgroup.moonwalk)
            {
                ClientAuthoWalkAndCast walkAndCastAction = (ClientAuthoWalkAndCast)current;
                WalkActionCommand walkCmd = (WalkActionCommand)newAction.mdbCommand;
                walkAndCastAction.WalkToNewPos(walkCmd.targetPos);
                return false;
            }
            //else if (skillgroup.interruptable)
            //{
            //    return true;
            //}
            else if (skillgroup.canturn)
            {
                //Change facing and return false
                WalkActionCommand walkCmd = (WalkActionCommand) newAction.mdbCommand;
                Entity entity = current.GetEntity();
                Vector3 currPos = entity.Position;
                Vector3 newFacing = (walkCmd.targetPos - currPos).normalized;
                entity.Forward = newFacing;
                return false;
            }

            return current.IsCompleted();
        }


        public static bool Update(Action current, Action newAction)
		{
			return !current.Update (newAction);
		}
	}

	public static class AuthoACInterruptManager
	{
        public static Dictionary<ACTIONTYPE, Func<Action, Action, bool>> AllAfterComplete = new Dictionary<ACTIONTYPE, Func<Action, Action, bool>>(){
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.AfterComplete},
                    {ACTIONTYPE.Flash, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.AfterComplete},
                    {ACTIONTYPE.INTERACT, InterruptFn.AfterComplete },
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DASHATTACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDUP, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DRAGGED, InterruptFn.AfterComplete},
                    {ACTIONTYPE.GETHIT, InterruptFn.AfterComplete},
                };

        public static Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action,Action, bool>>> manager = new Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action, Action, bool>>>()
		{
			{ACTIONTYPE.WALK, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
					{ACTIONTYPE.WALK, InterruptFn.Update} 	
				}
			},
            {ACTIONTYPE.APPROACH, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.APPROACH, InterruptFn.Update}
                }
            },
            {ACTIONTYPE.APPROACH_PATHFIND, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.Update}
                }
            },
            {ACTIONTYPE.CASTSKILL, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
					{ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
					{ACTIONTYPE.WALK, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.SkillInterruptSkill},
                    {ACTIONTYPE.Flash, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.APPROACH, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.INTERACT, InterruptFn.AfterComplete },
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.SkillInterruptSkill},                    
                    {ACTIONTYPE.DASHATTACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDUP, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DRAGGED, InterruptFn.AfterComplete},
                    {ACTIONTYPE.GETHIT, InterruptFn.AfterComplete},
                }
            },
            {ACTIONTYPE.Flash, AllAfterComplete},
            {ACTIONTYPE.WALKANDCAST, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.SkillInterruptSkill},
                    {ACTIONTYPE.Flash, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.MoveInterruptSkill},
                    {ACTIONTYPE.INTERACT, InterruptFn.AfterComplete },
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.SkillInterruptSkill},
                    {ACTIONTYPE.DASHATTACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDUP, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DRAGGED, InterruptFn.AfterComplete},
                    {ACTIONTYPE.GETHIT, InterruptFn.AfterComplete},
                }
            },            
            {ACTIONTYPE.DEAD, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.IDLE, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.WALK, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.Flash, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.APPROACH, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.INTERACT, InterruptFn.NoInterrupt },
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.DASHATTACK, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.KNOCKEDUP, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.DRAGGED, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.GETHIT, InterruptFn.NoInterrupt},
                }
            },
            {ACTIONTYPE.DASHATTACK, AllAfterComplete},
            {ACTIONTYPE.KNOCKEDBACK, AllAfterComplete},
            {ACTIONTYPE.KNOCKEDUP, AllAfterComplete},
            {ACTIONTYPE.DRAGGED, AllAfterComplete},
            {ACTIONTYPE.GETHIT, new Dictionary<ACTIONTYPE, Func<Action, Action, bool>>() {
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.NoInterrupt},
                    {ACTIONTYPE.Flash, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.AfterComplete},
                    {ACTIONTYPE.INTERACT, InterruptFn.AfterComplete },
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DASHATTACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.KNOCKEDUP, InterruptFn.AfterComplete},
                    {ACTIONTYPE.DRAGGED, InterruptFn.AfterComplete},
                    {ACTIONTYPE.GETHIT, InterruptFn.Update},
                }
            },
        };	
    }

	public static class NonAuthoACInterruptManager
	{
		public static Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action,Action, bool>>> manager = new Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action, Action, bool>>>()
		{
            {ACTIONTYPE.WALK, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.WALK, InterruptFn.Update}
                }
            },
            {ACTIONTYPE.WALK_WAYPOINT, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.WALK_WAYPOINT, InterruptFn.Update}
                }
            },
            {ACTIONTYPE.DEAD, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.DEAD, InterruptFn.NoInterrupt} //possible trying to perform multiply dying action
                }
            },
            {ACTIONTYPE.KNOCKEDBACK,  new Dictionary<ACTIONTYPE, Func<Action, Action, bool>>(){
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.Flash, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.APPROACH_PATHFIND, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.INTERACT, InterruptFn.AfterComplete },
                    //{ACTIONTYPE.WALKANDCAST, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.DASHATTACK, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.KNOCKEDUP, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.DRAGGED, InterruptFn.AfterComplete},
                    //{ACTIONTYPE.GETHIT, InterruptFn.AfterComplete},
                }
            },
            {ACTIONTYPE.FROZEN, new Dictionary<ACTIONTYPE, Func<Action, Action, bool>>()
            {
                {ACTIONTYPE.IDLE, InterruptFn.AfterComplete },
                {ACTIONTYPE.CASTSKILL, InterruptFn.AfterComplete },
                {ACTIONTYPE.WALK, InterruptFn.AfterComplete },
                {ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                //{ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
            }
            }
        };
	}
}


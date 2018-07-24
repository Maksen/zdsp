using System;
using System.Collections.Generic;

namespace Zealot.Server.Actions
{
    using Zealot.Common.Actions;
    using Zealot.Repository;
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
            return current.IsCompleted();
        }

        public static bool Update(Action current, Action newAction)
        {
            return !current.Update(newAction);
        }

        public static bool SkillInterruptSkill(Action current, Action newAction)
        {
            ISkillCastCommandCommon currSkillCmd = (ISkillCastCommandCommon)current.mdbCommand;
            int currskillid = currSkillCmd.GetSkillID();                       
            SkillData currSkillData = SkillRepo.GetSkillByGroupID(currskillid);
            if (currSkillData.skillgroupJson.moonwalk)
            {                
                if (current.IsCompleted())
                    return true;
                else
                {
                    ISkillCastCommandCommon newSkillCmd = (ISkillCastCommandCommon)newAction.mdbCommand;
                    int newskillid = newSkillCmd.GetSkillID();
                    if (newskillid == currskillid) //same moonwalk skill that is not completed yet
                    {
                        current.mdbCommand = newAction.mdbCommand; //just update the command which doesn't do anything since walk is done by client and not by server
                        return false;
                    }
                }                 
            }
            
            return true; //server always follow action from player
        }
    }

    public static class AuthoASInterruptManager
    {
        public static bool CanPerformAction(ACTIONTYPE currActionType,  ACTIONTYPE newActionType)
        {
            bool canStart = true;
            var manager = AuthoASInterruptManager.manager;
            if (  manager.ContainsKey(currActionType))
            {
                var interruptInfo = manager[currActionType]; 
                if (interruptInfo.ContainsKey(newActionType))
                {
                    if (interruptInfo[newActionType] == InterruptFn.NoInterrupt
                        || interruptInfo[newActionType] == InterruptFn.AfterComplete)
                    {
                        canStart = false;
                    }
                }  
            }
            return canStart;
        }

        public static Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action, Action, bool>>> manager = new Dictionary<ACTIONTYPE, Dictionary<ACTIONTYPE, Func<Action, Action, bool>>>()
		{
			{ACTIONTYPE.APPROACH, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
					{ACTIONTYPE.APPROACH, InterruptFn.Update},	
					{ACTIONTYPE.DRAGGED, InterruptFn.AlwaysInterrupt},
                }
			},
             {ACTIONTYPE.WALKANDCAST, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.WALKANDCAST, InterruptFn.SkillInterruptSkill},
                }
            },
             {ACTIONTYPE.KNOCKEDBACK, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.AfterComplete},
                }
            },
             {ACTIONTYPE.CASTSKILL, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.AfterComplete},
                }
            },
             {ACTIONTYPE.DEAD, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                    {ACTIONTYPE.KNOCKEDBACK, InterruptFn.NoInterrupt}, 
                    {ACTIONTYPE.DRAGGED, InterruptFn.NoInterrupt},//list down all action possible(those may happened due to other effects.
                }
            },
             {ACTIONTYPE.DRAGGED, new Dictionary<ACTIONTYPE, Func<Action,Action, bool>>(){
                     
                    {ACTIONTYPE.IDLE, InterruptFn.AfterComplete},
                    {ACTIONTYPE.CASTSKILL, InterruptFn.AfterComplete},
                    {ACTIONTYPE.WALK, InterruptFn.AfterComplete},
                    {ACTIONTYPE.APPROACH, InterruptFn.AfterComplete},
                }
            }
        };
    }
}
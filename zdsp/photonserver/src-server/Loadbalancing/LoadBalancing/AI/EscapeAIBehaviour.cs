using System;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Server.Entities;
using UnityEngine;

namespace Zealot.Server.AI
{
    
    public class MonsterEscapeAIBehaviour : BaseAIBehaviour
    {
        //public const int ROAM_COOLDOWN_TIME = 8000; //in msec 
        protected readonly List<Vector3> Escape_Directions = new List<Vector3>() {
            Vector3.left,
            Vector3.right,
            Vector3.forward,
            Vector3.down,
            new Vector3(0.70710678f, 0,0.70710678f),
            new Vector3(0.70710678f, 0,-0.70710678f),
            new Vector3(-0.70710678f, 0,0.70710678f),
            new Vector3(-0.70710678f, 0,-0.70710678f),
        };
        //protected bool mCanPathFind;
        protected Monster mMonster;
        protected MonsterSpawnerBase mSpawner; 
        //protected Actor mTarget;
       
        public MonsterEscapeAIBehaviour(Monster monster):base(monster)
        {             
            mSpawner = monster.mSp;            
        }

        public override void GotoState(string stateName)
        {
            if (stateName.Equals("Stun") || stateName.Equals("RecoverFromKnockedBack")) //group common state into one
                GotoState("Interrupted");
            else
                base.GotoState(stateName);
        } 

        protected override void InitStates()
        {
            AddState("Idle", OnIdleEnter, OnIdleLeave, OnIdleUpdate);
            AddState("Interrupted", OnInterruptedEnter, OnInterruptedLeave, OnInterruptedUpdate);
            AddState("Escape", OnEscapeEnter, OnEscapeLeave, OnEscapeUpdate);
            
        }

        public override void StartMonitoring()
        {
            mMonster = mActor as Monster;
            GotoState("Idle"); 
        }

        protected override void OnIdleEnter(string prevstate)
        {
            mMonster.HasMoved = false; //reset it . 
        }
        
        protected override void OnIdleUpdate(long dt)
        {     
        }

        #region RecoverFromKnockBack state
        protected  void OnInterruptedEnter(string prevstate)
        {
            
        }

        protected  void OnInterruptedUpdate(long dt)
        {
             
            ACTIONTYPE currAction = mMonster.GetActionCmd().GetActionType();
            if (currAction == ACTIONTYPE.KNOCKEDBACK || currAction == ACTIONTYPE.KNOCKEDUP)
            {
                return;
            }
            if (mMonster.HasControlStatus(ControlSEType.Stun))
                return;
            GotoState("Escape");
             
        }

        protected void OnInterruptedLeave()
        {

        }
        #endregion
         
        #region Escape State
 

        protected  void OnEscapeEnter(string prevstate)
        {
            DetermineNextPoint();                  
        }

        private Vector3 mTargetPos = Vector3.zero; 
        //private bool mbRestart = true;
        private List<int> mRandomDir = new List<int>(8) { 0, 1, 2, 3, 4, 5, 6, 7 }; 
        private void DetermineNextPoint()
        {
            //determine a target to escape. 
            //if (mbRestart)
            //{
                //mRandomDir = new List<int>(8) { 0, 1, 2, 3, 4, 5, 6, 7 };
            //}
            //if (mRandomDir.Count == 0)//it is very unlikely all 8 direction can not find a path.\
            //{
               // GotoState("Idle");
                //return;// all dirtion no path.
            //}
            int nextRand = GameUtils.RandomInt(0, mRandomDir.Count-1);
            int dir = mRandomDir[nextRand];
            mRandomDir.Remove(dir);//remove the element so it will not be randomized again, this can reduce attemp 
            if (mRandomDir.Count == 0)
            {
                mRandomDir = new List<int>(8) { 0, 1, 2, 3, 4, 5, 6, 7 };//a cycle is finished. restart 
            }
            Vector3 nextDir = Escape_Directions[dir];
            mTargetPos = mActor.Position + nextDir * 10f;
            (mActor as Monster).ApproachTargetWithPathFind(-1, mTargetPos, 1.0f, false, false);
            return; 
        }

        

        protected void OnEscapeUpdate(long dt)
        {
            if (mActor.IsMoving()) //if the action is ASApprowithPathfind, it will be true
                return;
            else
            {
                if (mMonster.HasMoved == false)//it will be true after waypoint move.
                {
                    DetermineNextPoint();//try the second direction if the first failed.
                }else
                {
                    GotoState("Idle");//monster have finished moving.
                }
            }
        }
        #endregion        
        
        protected void OnEscapeLeave()
        {

        }

        public override void OnAttacked(IActor attacker, int aggro)
        {   
            Actor attackerActor = attacker as Actor;
            if (attackerActor == null)
                return;

            int attackerPID = attackerActor.GetPersistentID();
            string currentStateName = GetCurrentStateName();
            if (currentStateName == "Escape"|| currentStateName == "Interrupt")
                return; 
            if (!mMonster.HasControlStatus(ControlSEType.Stun)) //this status is updated in time.
                GotoState("Escape");

        }

        public override void OnKilled() 
        {
            //SwitchTarget(null);
            base.OnKilled();
        }        
    }
 
}

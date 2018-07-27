namespace Zealot.Server.Entities
{
    using System.Collections.Generic;
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Datablock;
    using Zealot.Common.Entities;
    using Zealot.Server.Actions;
    using Zealot.Server.SideEffects;
    using Photon.LoadBalancing.GameServer;

    public class PositionSlots
    {
        private static byte[][] posPriorities = new byte[][] { 
            //based on index in PositionSlots
	        //last index of each array if omitted, AI won't go through target to the back of target
	        new byte []{0,7,1,6,2,5,3,4}, 	//top 0
	        new byte []{1,0,2,7,3,6,4,5}, 	//topright 1
	        new byte []{2,1,3,0,4,7,5,6}, 	//right 2
	        new byte []{3,2,4,1,5,0,6,7}, 	//bottomright 3
	        new byte []{4,3,5,2,6,1,7,0}, 	//bottom 4
	        new byte []{5,4,6,3,7,2,0,1}, 	//bottomleft 5
	        new byte []{6,5,7,4,0,3,1,2}, 	//left 6
	        new byte []{7,6,0,5,1,4,2,3} 	//topleft	7
        };

        private static Vector3[] dirs = new Vector3[]{
                   new Vector3(0, 0, 1),
				   new Vector3(0.707f,0, 0.707f),
				   new Vector3(1,0, 0),
				   new Vector3(0.707f,0,-0.707f),
				   new Vector3(0,0,-1),
				   new Vector3(-0.707f,0,-0.707f),
				   new Vector3(-1,0,0),
				   new Vector3(-0.707f,0,0.707f)};

        private Actor mTargetEntity;
        private Actor[] mSlots;

        public PositionSlots(Actor targetEntity)
        {
            mTargetEntity = targetEntity;
            mSlots = new Actor[8];
            Reset();    
        }

        public bool HasAvailableSlot()
        {
            foreach(Actor actor in mSlots)
            {
                if (actor == null || !actor.IsAlive())
                    return true;
            }
            return false;
        }

        public bool IsAttackerInSlots(Actor attacker)
        {
            foreach(Actor actor in mSlots)
            {
                if (actor == attacker)
                    return true;
            }
            return false;
        }

        private Vector3 AllocateSlot(Actor attacker, byte index, float preferredRange)
        {
            //Note: preferredRange should be at least 0.5m bigger than attacker radius
            mSlots[index] = attacker;

            Vector3 pos = mTargetEntity.Position;
            float combinedRadii = attacker.Radius + mTargetEntity.Radius;
            float offsetDist = System.Math.Max(System.Math.Min(preferredRange - 0.2f, combinedRadii + 4), combinedRadii);//at most 4m from target entity
            Vector3 offsetPos = pos + dirs[index] * offsetDist;
            return offsetPos;
        }

        private byte[] GetPosPrioritiesByAttackerPos(Actor attacker)
	    {
		    Vector3 targetPos = mTargetEntity.Position;
		    Vector3 attackerPos = attacker.Position;
		    float left = targetPos.x - attackerPos.x;
		    float top = attackerPos.z - targetPos.z;
		
		    if (left>=0)
		    {
			    if (top>=0) //top left
			    {
				    float ratio = top/left;
				    if (ratio<0.5)
					    return posPriorities[6]; //left centric
				    else if (ratio>2) 
					    return posPriorities[0]; //top centric
				    else 
					    return posPriorities[7]; //topleft centric
			    }
			    else
			    {
				    float ratio = -top/left;
				    if (ratio<0.5)
					    return posPriorities[6]; //left centric
				    else if (ratio>2) 
					    return posPriorities[4]; //bottom centric
				    else 
					    return posPriorities[5]; //bottomleft centric
			    }
		    }
		    else
		    {
			    if (top>=0) //top right
			    {
				    float ratio = top/-left;
				    if (ratio<0.5)
					    return posPriorities[2]; //right centric
				    else if (ratio>2) 
					    return posPriorities[0]; //top centric
				    else 
					    return posPriorities[1]; //topright centric
			    }
			    else //bottom right
			    {
				    float ratio = top/left;
				    if (ratio<0.5)
					    return posPriorities[2]; //right centric
				    else if (ratio>2) 
					    return posPriorities[4]; //bottom centric
				    else 
					    return posPriorities[3]; //bottomright centric
			    }
		    }	
	    }

        public Vector3? AllocateEmptySlot(Actor attacker, float preferredRange)
	    {
		    byte[] posPriorities = GetPosPrioritiesByAttackerPos(attacker);
		    
		    foreach(byte index in posPriorities)
		    {
			    Actor slot = mSlots[index];
			    if (slot == null || slot == attacker || !slot.IsAlive())
			    {
				    return AllocateSlot(attacker, index, preferredRange);                    
			    }
		    }
		    return null;
	    }	

        public bool DeallocateSlot(Actor attacker)
        {
            for(byte i = 0 ; i < mSlots.Length; i++)
            {
                Actor slot = mSlots[i];
                if (slot != null && slot == attacker)
                {
                    mSlots[i] = null;
                    return true;
                }
            }
            return false;
        }

        public void Reset()
        {
            for (byte i = 0; i < 8; i++)
                mSlots[i] = null;
        }
    }

    [System.Flags]
    public enum ControlSEType : byte
    {
        Stun = 1 << 0,
        Root = 1 << 1,
        Fear = 1 << 2,
        Silence = 1 << 3,
        Taunt = 1 << 4,
        //Total //used to determine total number of control side effects
    }

    [System.Flags]
    public enum ImmuneSEType : byte {
        Stun = 1 << 0,
        Root = 1 << 1,
        Fear = 1 << 2,
        Silence = 1 << 3,
        Taunt = 1 << 4,
        AllDamage = 1 << 5,
        AllDebuff = 1 << 6,
        AllControl = 1 << 7,
        
    }

    public class LocalSkillStats
    {
        public int attack;
        public int accuracy;
        public int critical;
        public int criticaldamage;
        public int armor;
        public int evasion = 0;
        public int cocritical = 0;
        public int cocriticaldamage = 0;
    }

    public abstract class Actor : NetEntity, IActor
    {                
        public ActorSynStats PlayerStats { get; set; }        
        public ICombatStats CombatStats { get; set; }
 
        public SkillPassiveCombatStats SkillPassiveStats { get; set; }
        protected LocalSkillStats finalcombatstats = null;
        public int GetHealth()
        {
            if (CombatStats == null)
                return 0;
            return (int)CombatStats.GetField(FieldName.Health);
        }

        public virtual float GetExDamage()
        {
            return 0;
        }

        public virtual void OnEvasion()
        {
            SkillPassiveStats.OnEvasion(GetPersistentID() ,GetHealthMax());
        }

        public virtual void SetHealth(int val)
        {
            int oriHealth = (int)CombatStats.GetField(FieldName.Health);
            CombatStats.SetField(FieldName.Health, val);
            if (oriHealth <= 0 && val > 0)
                PlayerStats.Alive = true;
            if (val == 0)
                PlayerStats.Alive = false;
        }

        public virtual void AddToHealth(int val)
        {
            int health = GetHealth() + val;
            int healthmax = GetHealthMax();
            if (health > healthmax)
                health = healthmax;
            SetHealth(health);
        }

        public int AddHealthPercentage(int percent)
        {
            int heal = GetHealthMax() * percent / 100;
            AddToHealth(heal);
            return heal;  
        }

        public int GetHealthMax()
        {
            return (int)CombatStats.GetField(FieldName.HealthMax);
        }

        public void SetHealthMax(int val)
        {
            CombatStats.SetField(FieldName.HealthBase, val);
            CombatStats.SetField(FieldName.HealthMax, val);
        }

        public bool IsAlive()
        {
            return !Destroyed && GetHealth() > 0;
        }
        public int Team { get { return PlayerStats.Team; } set { PlayerStats.Team = value; } }        
        public abstract bool IsInvalidTarget();
        public abstract bool IsInSafeZone();
        public string Name { get; set; }        

        private PositionSlots mPositionSlots;
        public PositionSlots PositionSlots { get { return mPositionSlots; } }
        private Dictionary<int, Actor> mNPCAttackers; //table of npcs having this actor as the current target
        protected List<SpecailSE> mPersistentSideEffects;
        protected SideEffect[] mSideEffectsPos;
        protected SideEffect[] mSideEffectsNeg;

        protected Kopio.JsonContracts.SideEffectJson mElemental;

        //public ControlStats ControlStats;
         

        protected int mMinDmg;

        public bool InvincibleMode { get { return PlayerStats.invincible; }
            set { PlayerStats.invincible = value; } }

        public Actor() : base()
        {
            mPositionSlots = new PositionSlots(this);
            mNPCAttackers = new Dictionary<int, Actor>();
            Radius = CombatUtils.DEFAULT_ACTOR_RADIUS;

            mPersistentSideEffects = new List<SpecailSE>();
            mSideEffectsPos = new SideEffect[BuffTimeStats.MAX_EFFECTS];
            mSideEffectsNeg = new SideEffect[BuffTimeStats.MAX_EFFECTS];

            //ControlStats = new ControlStats();
            finalcombatstats = new LocalSkillStats();
            mMinDmg = 0;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            mPositionSlots.Reset();
            ClearNPCAttackers();
        }

        public virtual void StartInvincible(float seconds)
        {
            if (seconds <= 0)
                return;
            InvincibleMode = true;
            EntitySystem.Timers.SetTimer((long)(seconds*1000), (arg)=>{
                InvincibleMode = false;
            }, null);
        }

        #region NPCAttackers
        public void AddNPCAttacker(Actor npc)
	    {
		    int pid = npc.GetPersistentID();
		    if (!mNPCAttackers.ContainsKey(pid))
		    {
			    mNPCAttackers.Add(pid, npc);                    
		    }
	    }
	
	    public void RemoveNPCAttacker(Actor npc)
	    {		    
            mNPCAttackers.Remove(npc.GetPersistentID());
	    }
	
	    public Dictionary<int, Actor> GetNPCAttackers()
	    {		
		    return mNPCAttackers;
	    }
	
	    public void ClearNPCAttackers()
	    {
		    mNPCAttackers.Clear();
	    }
        #endregion
        public ShieldSE shieldSE = null;

       
        public virtual void RTStarted()
        {
            RecoverTime =  EntitySystem.Timers.GetSynchronizedTime();
        }

        public bool IsInRT()
        {
            long now = EntitySystem.Timers.GetSynchronizedTime();
            return now < RecoverTime + CombatUtils.RECOVER_FROM_HIT_TIME;
        }

        public virtual void CombatStarted()
        {
             
        }
        public virtual void OnDamage(IActor attacker, AttackResult res, bool pbasicattack)
        {
            if (!IsAlive())
                return;
           
            if (attacker != this) //if player attacks himself, we don't queue as attacker
                attacker.QueueDmgResult(res); //For attacker to see this result at client
            if (shieldSE != null)
            {
                res.RealDamage = shieldSE.OnAttacked(res.RealDamage);
            }else
            {
                res.RealDamage = SkillPassiveStats.OnDamage(res.RealDamage, this);
            }
            
            QueueDmgResult(res);          //For defender to see this result at client
 
            OnAttacked(attacker, res.RealDamage); //currently 1:1  
            
            int temphealth = GetHealth();            
            temphealth -= res.RealDamage;
            if (temphealth <= 0)
            {                                
                SetHealth(0);
                OnKilled(attacker);
                //SetHealth(GetHealthMax());
            }
            else
            {                
                SetHealth(temphealth);
            }
        }

        public virtual void OnRecoverHealth(int origamount)
        {
            if (!IsAlive())
                return;
            if (IsPlayer())
            {
                int rejperc = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.Rej_Increase);
                origamount = (int)(origamount * (1 + rejperc * 0.01f));
            }
            //suppress rej
            float supp = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.RejSupress);
            float finalres = origamount - origamount * supp * 0.01f;
            int amount = (int)finalres;
            int temphealth = GetHealth() + amount;
            
            if (temphealth > GetHealthMax())
                temphealth = GetHealthMax();

            SetHealth(temphealth);
            int mypid = GetPersistentID();
            AttackResult res = new AttackResult(mypid, amount, false, mypid);
            res.IsHeal = true;
            QueueDmgResult(res); //For me to see the heal;
        }
        
        //public void QueueSideEffectHit(int attackerpid, int sideeffectID)
        //{
        //    mSideEffectHits.Add(new SideEffectHit(attackerpid, sideeffectID));
        //}
        
        //public void SendSideEffectHits(GameClientPeer peer)
        //{            
        //    foreach(SideEffectHit sehit in mSideEffectHits)
        //    {                
        //        if (peer.mPlayer == this || peer.mPlayer.GetPersistentID() == sehit.AttackerPID) //If player is suffering the hit or I'm the player attacker, I should see the hit at the client
        //        {
        //            peer.ZRPC.UnreliableCombatRPC.SideEffectHit(GetPersistentID(), sehit.SideEffectID, peer);
        //        }
        //    }                        
        //}

        public virtual bool AddElementalSE(Kopio.JsonContracts.SideEffectJson se) {
            // check if the side effect belongs to current equiped weapon
            // any other side effects that comes that are not will be applied instead


            mElemental = se;

            //return true if this side effect is to be applied for damage
            return true;
        }

        public virtual bool CheckImmuneStatus(ImmuneSEType bitstring) {
            return ((ImmuneSEType)PlayerStats.ImmuneStatus & bitstring) == bitstring;
        }

        public virtual void SetImmune(ImmuneSEType bitstring) {
            PlayerStats.ImmuneStatus |= (byte)bitstring;
        }

        public virtual void RemoveImmune(ImmuneSEType bitstring) {
            PlayerStats.ImmuneStatus ^= (byte)bitstring;
        }

        //public virtual bool CheckImmuneStatusByEffectType(EffectType type) {
        //    ImmuneSEType bitstring;
        //    switch (type) {
                
        //    }
        //}

        public virtual void QueueDmgResult(AttackResult res)
        {            
        }

        public virtual void OnAttacked(IActor attacker, int aggro)
        {
        }

        public virtual void OnKilled(IActor attacker)
        {
            PlayerStats.Alive = false;
            mPositionSlots.Reset();
            ClearNPCAttackers();
        }

        public override void UpdateEntitySyncStats(GameClientPeer peer)
        {
            if (PlayerStats.IsDirty())
                peer.ZRPC.LocalObjectRPC.UpdateLocalObject((byte)LOCATEGORY.EntitySyncStats, GetPersistentID(), PlayerStats, peer);
            
            //SendSideEffectHits(peer);
        }

        public override void AddEntitySyncStats(GameClientPeer peer)
        {
            peer.ZRPC.LocalObjectRPC.AddLocalObject((byte)LOCATEGORY.EntitySyncStats, GetPersistentID(), PlayerStats, peer);            
        }

        public override void ResetSyncStats()
        {
            if (PlayerStats == null)
                return;
            if (PlayerStats.IsDirty())
                PlayerStats.Reset();           
        } 

        public bool IsPerformingApproach()
        {
            System.Type actionType = GetAction().GetType();
            return actionType == typeof(ASApproachWithPathFind) || actionType == typeof(ServerAuthoASApproach);
        }

        public bool IsMoving()
        {
            System.Type actionType = GetAction().GetType();
            return actionType == typeof(ASApproachWithPathFind) || actionType == typeof(ServerAuthoASApproach) ||
                   actionType == typeof(ServerAuthoASWalk);
        }

        public List<SpecailSE> GetPersistentSEList()
        {
            return mPersistentSideEffects;
        }

        public virtual bool AddSpecialSideEffect(SpecailSE se)
        {
            if(mPersistentSideEffects.Count < BuffTimeStats.MAX_EFFECTS)
            {
                mPersistentSideEffects.Add(se);
                return true;
            }
            return false;
        }
       
        public virtual void RemoveSpecialSideEffect(SpecailSE se)
        {
            if (mPersistentSideEffects.Contains(se))
            {
                mPersistentSideEffects.Remove(se);
            }
        }
                     
        public virtual int AddSideEffect(SideEffect se, bool positiveEffect)
        {   
            SideEffect[] sideeffects;
            int slotid = -1; 
            if (se.IsDot())
            {
                SkillPassiveStats.OnDotStart(); 
            } else if (se.IsDeBuff())
            {
                SkillPassiveStats.OnDebuffStart();
            }
            if (positiveEffect)            
                sideeffects = mSideEffectsPos;            
            else            
                sideeffects = mSideEffectsNeg; 
            for (int i = 0; i < sideeffects.Length; i++)
            {
                if (sideeffects[i] == null&&slotid ==-1)
                {
                    sideeffects[i] = se;
                    AddSideEffectVisual(se, positiveEffect);
                    slotid = i;
                    break;
                } 
            } 
            return slotid; 
        }  

         
        public int NumOfStun, NumOfSlience,NumOfRoot,NumOfSlow,NumOfDisarm = 0;//this is need to support multiply control se at the same time
        private void AddSideEffectVisual(SideEffect se, bool positiveEffect)
        {            
            if (se.mSideeffectData.effecttype == EffectType.Control_Stun )
            {
                NumOfStun++;
                if(NumOfStun ==1)
                    PlayerStats.VisualEffectTypes |= (int) EffectVisualTypes.Stun;
            }
            else if(se.mSideeffectData.effecttype == EffectType.Control_Silence )
            {
                NumOfSlience++;
                if(NumOfSlience==1)
                PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Slience;
            }
            else if (se.mSideeffectData.effecttype == EffectType.Control_Root)
            {
                NumOfRoot++;
                if(NumOfRoot ==1)
                PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Root;
            }
            //else if (se.mSideeffectData.effecttype == EffectType.Control_Slow)
            //{
            //    NumOfSlow++;
            //    if(NumOfSlow ==1)
            //    PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Slow;
            //}
            //else if (se.mSideeffectData.effecttype == EffectType.Control_Disarmed)
            //{
            //    NumOfDisarm++;
            //    if(NumOfDisarm==1)
            //    PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Disarmed;
            //}
            else if (se.mSideeffectData.effectpath != "" || se.mSideeffectData.icon != "")
            {           
                if (positiveEffect)
                    SetPositiveVisualSE(se.mSideeffectData.id);
                else
                    SetNegativeVisualSE(se.mSideeffectData.id);
            }
        }
        
        private void StopSideEffectVisual(SideEffect se, bool positiveEffect)
        {             
            if (se.mSideeffectData.effecttype == EffectType.Control_Stun )
            {
                NumOfStun--;
                if(NumOfStun ==0)
                PlayerStats.VisualEffectTypes &= ~( (int)EffectVisualTypes.Stun);
            }
            else if (se.mSideeffectData.effecttype == EffectType.Control_Silence )
            {
                NumOfSlience--;
                if(NumOfSlience==0)
                PlayerStats.VisualEffectTypes &= ~( (int)EffectVisualTypes.Slience);
            }
            else if (se.mSideeffectData.effecttype == EffectType.Control_Root)
            {
                NumOfRoot--;
                if(NumOfRoot==0)
                PlayerStats.VisualEffectTypes &= ~( (int)EffectVisualTypes.Root);
            }
            //else if (se.mSideeffectData.effecttype == EffectType.Control_Slow)
            //{
            //    NumOfSlow--;
            //    if(NumOfSlow==0)
            //    PlayerStats.VisualEffectTypes &= ~((int)EffectVisualTypes.Slow);
            //}
            //else if (se.mSideeffectData.effecttype == EffectType.Control_Disarmed)
            //{
            //    NumOfDisarm--;
            //    if(NumOfDisarm==0)
            //    PlayerStats.VisualEffectTypes &= ~((int)EffectVisualTypes.Disarmed);
            //}
            else if (se.mSideeffectData.effectpath != "" || se.mSideeffectData.icon != "")
            {
                if (positiveEffect)
                    SetPositiveVisualSE(0);
                else
                    SetNegativeVisualSE(0);
            }
        }

        protected virtual void SetPositiveVisualSE(int id)
        {
            PlayerStats.PositiveVisualSE = id;
        }

        protected virtual void SetNegativeVisualSE(int id)
        {
            PlayerStats.NegativeVisualSE = id;
        }

        public virtual int RemoveSideEffect(SideEffect se, bool positiveEffect)
        {
            SideEffect[] sideeffects;
            int slotid = -1; 
            
            if (positiveEffect)
            {
                sideeffects = mSideEffectsPos;            
            }
            else
            {
                sideeffects = mSideEffectsNeg;                
            }
                        
            for (int i = 0; i < sideeffects.Length; i++)
            {
                if (sideeffects[i] == se)
                {                    
                    sideeffects[i] = null;
                    StopSideEffectVisual(se, positiveEffect); 
                    slotid = i;
                    break;
                } 
            }
            if (se.IsDot())
            {
                SkillPassiveStats.OnDotEnd();
            }
            else if(se.IsDeBuff())
            {
                SkillPassiveStats.OnDebuffEnd();
            }
            return slotid;    
        }
        
        public void StopAllControl(ControlSEType ctype)
        {
            switch (ctype)
            {
                case ControlSEType.Stun:
                    StopSideEffect(EffectType.Control_Stun);
                break;
            }
        }

        public virtual void StopSideEffect(EffectType targettype)
        {
            for (int i = 0; i < mSideEffectsPos.Length; i++)
            {
                SideEffect se = mSideEffectsPos[i];
                if (se != null && se.mSideeffectData.effecttype == targettype)
                {
                    se.Stop();
                    break;//stop one at time;
                }
            }

            for (int i = 0; i < mSideEffectsNeg.Length; i++)
            {
                SideEffect se = mSideEffectsNeg[i];
                if (se != null && se.mSideeffectData.effecttype == targettype)
                {
                    se.Stop();
                    break;
                }
            }
        }

        public virtual void StopAllSideEffects()
        {
            for (int i = 0; i < mSideEffectsPos.Length; i++)
            {
                SideEffect se = mSideEffectsPos[i];
                if (se != null)
                {
                    mSideEffectsPos[i].Stop();
                }
            }

            for (int i = 0; i < mSideEffectsNeg.Length; i++)
            {
                SideEffect se = mSideEffectsNeg[i];
                if (se != null)
                {
                    mSideEffectsNeg[i].Stop();
                }
            }
        }

        private long lastUpdatePersistentSE = 0;
        public override void Update(long dt)
        {
            base.Update(dt);

            if (IsAlive())
            {
                foreach (SideEffect se in mSideEffectsPos)
                {
                    if (se != null)
                        se.Update(dt);
                }

                foreach (SideEffect se in mSideEffectsNeg)
                {
                    if (se != null)
                        se.Update(dt);
                }
            }
            lastUpdatePersistentSE += dt;
            if(lastUpdatePersistentSE >= 1000)
            {
                for (int i = mPersistentSideEffects.Count -1; i >=0; i--)
                {
                    SpecailSE se = mPersistentSideEffects[i];
                    if (se!=null)
                        se.OnInterval(lastUpdatePersistentSE);                    
                }
                lastUpdatePersistentSE = 0;
            }
        }
         
        public bool HasSideEffect(int sid)
        {
            foreach(SideEffect se in mSideEffectsPos)
            {
                if (se !=null && se.mSideeffectData.id == sid)
                {
                    return true;
                }
            }
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.mSideeffectData.id == sid)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasDot()
        {
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.IsDot())
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasCtl()
        {
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.IsControl())
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasSideEffectType(EffectType efftype)
        {
            foreach (SideEffect se in mSideEffectsPos)
            {
                if (se != null && se.mSideeffectData.effecttype == efftype)
                {
                    return true;
                }
            }
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.mSideeffectData.effecttype == efftype)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveRandomBuff(bool debuff = false)
        {
            List<SideEffect> bufflist = new List<SideEffect>();
            SideEffect[] sideeffectlist = debuff ? mSideEffectsNeg : mSideEffectsPos;
            foreach (SideEffect se in sideeffectlist)
            {
                if (se !=null && se.IsBuff() && se.GetType() == typeof(StatsSE))
                {
                    bufflist.Add(se);
                }
            }
            if (bufflist.Count > 0)
            {
                bufflist[GameUtils.RandomInt(1, bufflist.Count) - 1].Stop();
            }
        }

        public void RemoveBuff(EffectType setype,bool isPositive = false)
        {
            List<SideEffect> bufflist = new List<SideEffect>();
            SideEffect[] sideeffectlist = isPositive ? mSideEffectsPos : mSideEffectsNeg;
            foreach (SideEffect se in sideeffectlist)
            {
                if(se != null && se.mSideeffectData.effecttype == setype)
                {
                    se.Stop();
                    break;//only remove one
                } 
            } 
        }

        public void RemoveDot()
        {
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.IsDot())
                {
                    se.Stop();
                    break;
                }
            } 
        }

        public List<SideEffect> GetSideEffectOfType(EffectType efftype)
        {
            List<SideEffect> res = new List<SideEffect>();
            foreach (SideEffect se in mSideEffectsPos)
            {
                if (se != null && se.mSideeffectData.effecttype == efftype)
                {
                    res.Add(se);
                }
            }
            foreach (SideEffect se in mSideEffectsNeg)
            {
                if (se != null && se.mSideeffectData.effecttype == efftype)
                {
                    res.Add(se);
                }
            }
            return res;
        }

        public RecoverOnHitSE recoverOnHitSE = null;
        public void onHit(int dmg)
        {
            if (recoverOnHitSE != null)
            {
                recoverOnHitSE.OnHit(dmg);
            }

        }

         
        
        //call this for updating status to client.  
        public virtual void OnControlChanged( )
        {            
             
        }

        public virtual void SetControlStatus(ControlSEType ctype) {
            PlayerStats.ControlStatus |= (byte)ctype;
        }

        public virtual void RemoveControlStatus(ControlSEType ctype) {
            PlayerStats.ControlStatus ^= (byte)ctype;
        }

        /// <summary>
        /// get Immune status
        /// </summary>
        /// <param name="ctype"></param>
        /// <returns></returns>
        public virtual bool IsControlImmune(ControlSEType ctype)
        {
            return (PlayerStats.ImmuneStatus & (byte)ctype) == (byte)ctype;
            //switch (ctype)
            //{
            //    case ControlSEType.Stun:
            //        return ControlStats.StunImmuned;
            //    case ControlSEType.Root:
            //        return ControlStats.RootImmuned;
            //    case ControlSEType.Slow:
            //        return ControlStats.SlowImmuned;
            //    case ControlSEType.Disarmed:
            //        return ControlStats.DisarmImmuned;
            //    case ControlSEType.Silence:
            //        return ControlStats.SilenceImmuned;
            //    default:
            //        return false;
            //}
        }


        public virtual void onDragged(Vector3 pos, float dur, float speed)
        {           
        }

        public virtual void OnStun()
        {
        }

        public virtual void OnRoot()
        {
        }

        public bool HasControlStatus(ControlSEType setype)
        {
            return (PlayerStats.ControlStatus & (byte)setype) == (byte)setype;

            //if (ControlStats ==null)
            //    return false;
            //switch (setype)
            //{
            //    case ControlSEType.Stun:
            //        return ControlStats.Stuned;
                    
            //    case ControlSEType.Disarmed:
            //        return ControlStats.Disarmed;

            //    case ControlSEType.Silence:
            //        return ControlStats.Silenced;
            //    case ControlSEType.Root:
            //        return ControlStats.Rooted;
            //    case ControlSEType.Slow:
            //        return ControlStats.Slowed;

            //}
            //return false;

        }

        public long RecoverTime = 0;
        public long InCombatTime = 0;
        public float InBattleTime;
        public float BattleTime;               
        public float MaxBattleTime = 900;             //MaxBattleTime
        public float ExpRate { get; set; }         //經驗值比例
        //public void SetImmuneControl(ControlSEType settype, bool flag)
        //{
        //    mControlImmune[(int)settype] = flag;
        //}

        public int GetMinDmg()
        {
            return mMinDmg;
        }

        public bool MaxEvasionChance { get; set; }
        public bool MaxCriticalChance { get; set; }

        public virtual int GetAccuracy()
        {
            return finalcombatstats.accuracy;
        }

        public virtual int GetAttack()
        {
            return finalcombatstats.attack;
        }

        public virtual int GetCritical()
        {
            return finalcombatstats.critical;
        }

        public virtual int GetCriticalDamage()
        {
            return finalcombatstats.criticaldamage;
        }

        public virtual int GetArmor()
        {
            return finalcombatstats.armor;
        }

        public virtual int GetEvasion()
        {
            return finalcombatstats.evasion;
        }

        public virtual int GetCocritical()
        {
            return finalcombatstats.cocritical;
        }

        public virtual int GetCocriticalDamage()
        {
            return finalcombatstats.cocriticaldamage;
        }

        public virtual void UpdateLocalSkillPassiveStats()
        {
            finalcombatstats.accuracy = CombatFormula.GetFinalAccuracy(this);
            finalcombatstats.armor = CombatFormula.GetFinalArmor(this);
            finalcombatstats.attack = CombatFormula.GetFinalAttack(this);
            finalcombatstats.evasion = CombatFormula.GetFinalEvasion(this);
            finalcombatstats.critical = CombatFormula.GetFinalCritical(this);
            finalcombatstats.cocritical = CombatFormula.GetFinalCoCritical(this);
            finalcombatstats.criticaldamage = CombatFormula.GetFinalCriticalDamage(this);
            finalcombatstats.cocriticaldamage = CombatFormula.GetFinalCoCriticalDamage(this);
        }

        public virtual void OnComputeCombatStats()
        {
            // only for player
        }

        public virtual int GetParty()
        {
            return -1;
        }
    }
}

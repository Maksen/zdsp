namespace Zealot.Server.Entities
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Datablock;
    using Zealot.Common.Entities;
    using Zealot.Server.Actions;
    using Zealot.Server.SideEffects;
    using Photon.LoadBalancing.GameServer;
    using Photon.LoadBalancing.GameServer.CombatFormula;

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
            float offsetDist = Math.Max(Math.Min(preferredRange - 0.2f, combinedRadii + 4), combinedRadii);//at most 4m from target entity
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

    [Flags]
    public enum ControlSEType : byte
    {
        Stun = 1 << 0,
        Root = 1 << 1,
        Fear = 1 << 2,
        Silence = 1 << 3,
        Taunt = 1 << 4,
        Freeze = 1 << 5,
        //Total //used to determine total number of control side effects
    }

    [Flags]
    public enum ImmuneSEType : int {
        Stun = 1 << 0,
        Root = 1 << 1,
        Fear = 1 << 2,
        Silence = 1 << 3,
        Taunt = 1 << 4,
        Freeze = 1 << 5,
        AllDamage = 1 << 6,
        AllDebuff = 1 << 7,
        AllControl = 1 << 8        
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

        public virtual float GetExDamage()
        {
            return 0;
        }

        public virtual void OnEvasion()
        {
            SkillPassiveStats.OnEvasion(GetPersistentID() ,GetHealthMax());
        }

        public int GetHealth()
        {
            if (CombatStats == null)
                return 0;
            return (int)CombatStats.GetField(FieldName.Health);
        }

        public virtual void SetHealth(int val)
        {
            int origHealth = (int)CombatStats.GetField(FieldName.Health);
            CombatStats.SetField(FieldName.Health, val);
            if (origHealth <= 0 && val > 0)
                PlayerStats.Alive = true;
            else if (val == 0)
                PlayerStats.Alive = false;
        }

        public virtual void AddToHealth(int val)
        {
            int health = GetHealth() + val;
            health = Math.Min(health, GetHealthMax());
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

        public int GetMana()
        {
            if (CombatStats == null)
                return 0;
            return (int)CombatStats.GetField(FieldName.Mana);
        }

        public float GetManaNormalized()
        {
            return (float)GetMana() / GetManaMax();
        }

        public virtual void SetMana(int val)
        {
            CombatStats.SetField(FieldName.Mana, val);
        }

        public virtual void AddToMana(int val)
        {
            int mana = GetMana() + val;
            mana = Math.Min(mana, GetManaMax());
            SetMana(mana);
        }

        public int AddManaPercentage(int percent)
        {
            int mana = GetManaMax() * percent / 100;
            AddToMana(mana);
            return mana;
        }

        public int GetManaMax()
        {
            return (int)CombatStats.GetField(FieldName.ManaMax);
        }

        public void SetManaMax(int val)
        {
            CombatStats.SetField(FieldName.ManaBase, val);
            CombatStats.SetField(FieldName.ManaMax, val);
        }

        public bool IsAlive()
        {
            return !Destroyed && PlayerStats.Alive;
        }
        public int Team { get { return PlayerStats.Team; } set { PlayerStats.Team = value; } }
        public abstract bool IsInvalidTarget();
        public abstract bool IsInSafeZone();
        public string Name { get; set; }

        private PositionSlots mPositionSlots;
        public PositionSlots PositionSlots { get { return mPositionSlots; } }
        private Dictionary<int, Actor> mNPCAttackers; //table of npcs having this actor as the current target
        private Dictionary<int, Actor> mPlayerAttackers; //table of players having this actor as the current target
        protected List<SpecialSE> mPersistentSideEffects;
        protected SideEffect[] mSideEffectsPos;
        protected SideEffect[] mSideEffectsNeg;
        protected Dictionary<int, List<SideEffect>> m_EquipmentSE; // <equipment id , sideeffects>
        protected Dictionary<int, int> m_SideEffectList; // <sideeffect id , stack count>

        // Prototyping test stuff
        protected Dictionary<byte, List<SideEffect>> m_SideEffects;
        protected Dictionary<byte, List<int>> m_RemovedSE;
        // proto

        //public ControlStats ControlStats;

        protected int mMinDmg;

        public ShieldSE shieldSE = null;

        public bool InvincibleMode { get { return PlayerStats.invincible; }
            set { PlayerStats.invincible = value; } }

        public Actor() : base()
        {
            mPositionSlots = new PositionSlots(this);
            mNPCAttackers = new Dictionary<int, Actor>();
            mPlayerAttackers = new Dictionary<int, Actor>();
            Radius = CombatUtils.DEFAULT_ACTOR_RADIUS;

            mPersistentSideEffects = new List<SpecialSE>();
            mSideEffectsPos = new SideEffect[BuffTimeStats.MAX_EFFECTS];
            mSideEffectsNeg = new SideEffect[BuffTimeStats.MAX_EFFECTS];
            m_EquipmentSE = new Dictionary<int, List<SideEffect>>();
            m_SideEffectList = new Dictionary<int, int>();
            m_SideEffects = new Dictionary<byte, List<SideEffect>>();
            m_RemovedSE = new Dictionary<byte, List<int>>();

            //ControlStats = new ControlStats();
            finalcombatstats = new LocalSkillStats();
            mMinDmg = 0;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            mPositionSlots.Reset();
            ClearNPCAttackers();
            ClearPlayerAttackers();
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

        public void AddPlayerAttacker(Actor player)
        {
            int pid = player.GetPersistentID();
            if (!mPlayerAttackers.ContainsKey(pid))
            {
                mPlayerAttackers.Add(pid, player);
            }
        }

        public void RemovePlayerAttacker(Actor player)
        {
            mPlayerAttackers.Remove(player.GetPersistentID());
        }

        public Dictionary<int, Actor> GetPlayerAttackers()
        {
            return mPlayerAttackers;
        }

        public void ClearPlayerAttackers()
        {
            mPlayerAttackers.Clear();
        }
        #endregion

        public virtual void RTStarted()
        {
            RecoverTime =  EntitySystem.Timers.GetSynchronizedTime();
        }

        public bool IsInRT()
        {
            long now = EntitySystem.Timers.GetSynchronizedTime();
            return now < RecoverTime + CombatUtils.RECOVER_FROM_HIT_TIME;
        }

        private long getHitTime = 0;

        public virtual void OnGetHit(long cooldown)
        {
            if (PlayerStats.HeavyStand)
                return;

            if (getHitTime <= 0)
            {
                getHitTime = cooldown;
            }
        }

        public virtual bool IsGettingHit()
        {
            return getHitTime > 0;
        }

        public virtual void OnDamage(IActor attacker, AttackResult res, bool pbasicattack)
        {
            if (!IsAlive())
                return;
           
            if (attacker != this) // If player attacks himself, we don't queue as attacker
                attacker.QueueDmgResult(res); //For attacker to see this result at client

            res.RealDamage = (shieldSE != null) 
                ? shieldSE.OnAttacked(res.RealDamage) : SkillPassiveStats.OnDamage(res.RealDamage, this);        
            QueueDmgResult(res); // For defender to see this result at client
            OnAttacked(attacker, res.RealDamage); //currently 1:1  
            OnGetHit(200);

            int hpAfterDmg = GetHealth() - res.RealDamage;
            if (hpAfterDmg <= 0)
            {                                
                SetHealth(0);
                OnKilled(attacker);
            }
            else          
                SetHealth(hpAfterDmg);
        }

        public virtual void OnRecoverHealth(int origAmount)
        {
            if (!IsAlive())
                return;

            if (IsPlayer())
            {
                int rejperc = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.Rej_Increase);
                origAmount = (int)(origAmount * (1 + rejperc * 0.01f));
            }
            //suppress rej
            float supp = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.RejSupress);
            float finalRes = origAmount - origAmount * supp * 0.01f;
            int amount = (int)finalRes;
            int hpAfterHeal = GetHealth() + amount;          
            if (hpAfterHeal > GetHealthMax())
                hpAfterHeal = GetHealthMax();

            SetHealth(hpAfterHeal);
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
        public virtual Actor GetOwner() { return this; }
        public virtual void QueueDmgResult(AttackResult res){ }
        public virtual void OnAttacked(IActor attacker, int aggro){ }
        public virtual void OnKilled(IActor attacker)
        {
            PlayerStats.Alive = false;
            mPositionSlots.Reset();
            ClearNPCAttackers();
            ClearPlayerAttackers();
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
            Type actionType = GetAction().GetType();
            return actionType == typeof(ASApproachWithPathFind) || actionType == typeof(ServerAuthoASApproach);
        }

        public bool IsMoving()
        {
            Type actionType = GetAction().GetType();
            return actionType == typeof(ASApproachWithPathFind) || actionType == typeof(ServerAuthoASApproach) ||
                   actionType == typeof(ServerAuthoASWalk);
        }

        public List<SpecialSE> GetPersistentSEList()
        {
            return mPersistentSideEffects;
        }

        public virtual bool AddSpecialSideEffect(SpecialSE se)
        {
            if(mPersistentSideEffects.Count < BuffTimeStats.MAX_EFFECTS)
            {
                mPersistentSideEffects.Add(se);
                return true;
            }
            return false;
        }
       
        public virtual void RemoveSpecialSideEffect(SpecialSE se)
        {
            if (mPersistentSideEffects.Contains(se))
            {
                mPersistentSideEffects.Remove(se);
            }
        }

        public virtual int AddSideEffect(SideEffect se, bool positiveEffect)
        {   
            //SideEffect[] sideeffects;
            int slotid = -1;
            if (se.IsDot())
                SkillPassiveStats.OnDotStart();
            else if (se.IsDeBuff())
                SkillPassiveStats.OnDebuffStart();

            bool stack = AddSideEffectToList(se);
            byte type = (byte)SideEffectsUtils.GetEffectHandleType(se.mSideeffectData.effecttype);
            if (!m_SideEffects.ContainsKey(type)) m_SideEffects.Add(type, new List<SideEffect>());
            // find for repeats to stack
            SideEffect old = m_SideEffects[type].Find(x => x.mSideeffectData.id == se.mSideeffectData.id);
            if(old == null) m_SideEffects[type].Add(se);
            else
            {
                // stack the se
                // increase se effect + time refresh
                old.StackTiming(stack);
                return -1;
            }

            AddSideEffectVisual(se, positiveEffect);
            return type;


            //if (positiveEffect)            
            //    sideeffects = mSideEffectsPos;            
            //else            
            //    sideeffects = mSideEffectsNeg;

            //int length = sideeffects.Length;
            //for (int i = 0; i < length; ++i)
            //{
            //    if (sideeffects[i] == null && slotid == -1)
            //    {
            //        sideeffects[i] = se;
            //        AddSideEffectVisual(se, positiveEffect);
            //        slotid = i;
            //        break;
            //    } 
            //} 
        }

        public virtual bool AddEquipmentSideEffect(SideEffect se, int equipid)
        {
            if (AddSideEffectToList(se))
            {
                if (!m_EquipmentSE.ContainsKey(equipid))
                    m_EquipmentSE[equipid] = new List<SideEffect>();

                m_EquipmentSE[equipid].Add(se);
                return true;
            }
            return false;
        }

        public virtual bool RemoveEquipmentSideEffect(SideEffect se, int equipid)
        {
            if (!m_EquipmentSE.ContainsKey(equipid))
                return false;
            else
            {
                int index = m_EquipmentSE[equipid].FindIndex(x => x.mSideeffectData.id == se.mSideeffectData.id);
                if (index == -1)
                    return false;
                RemoveSideEffectFromList(m_EquipmentSE[equipid][index]);
                m_EquipmentSE[equipid].RemoveAt(index);
                return true;
            }
        }

        public virtual bool AddSideEffectToList(SideEffect se)
        {
            if (!m_SideEffectList.ContainsKey(se.mSideeffectData.id))
            {
                m_SideEffectList[se.mSideeffectData.id] = 1;
                return true;
            }
            else
            {
                if (se.mSideeffectData.stackable && (se.mSideeffectData.stackcount >= m_SideEffectList[se.mSideeffectData.id]))
                {
                    ++m_SideEffectList[se.mSideeffectData.id];
                    return true;
                }
                else if (!se.mSideeffectData.stackable && m_SideEffectList[se.mSideeffectData.id] == 0)
                {
                    ++m_SideEffectList[se.mSideeffectData.id];
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public virtual void RemoveSideEffectFromList(SideEffect se)
        {
            if (!m_SideEffectList.ContainsKey(se.mSideeffectData.id))
                return;
            else
            {
                if (m_SideEffectList[se.mSideeffectData.id] > 0)
                    m_SideEffectList[se.mSideeffectData.id] = 0;
            }
        }
         
        public int NumOfStun = 0, NumOfSlience = 0, NumOfRoot = 0, NumOfSlow = 0, NumOfDisarm = 0, NumOfFrozen = 0;//this is need to support multiply control se at the same time
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
                PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Silence;
            }
            else if (se.mSideeffectData.effecttype == EffectType.Control_Root)
            {
                NumOfRoot++;
                if(NumOfRoot ==1)
                PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Root;
            }
            else if (se.mSideeffectData.effecttype == EffectType.SpecialControl_Freeze)
            {
                NumOfFrozen++;
                if (NumOfFrozen == 1)
                    PlayerStats.VisualEffectTypes |= (int)EffectVisualTypes.Frozen;
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
                PlayerStats.VisualEffectTypes &= ~( (int)EffectVisualTypes.Silence);
            }
            else if (se.mSideeffectData.effecttype == EffectType.Control_Root)
            {
                NumOfRoot--;
                if(NumOfRoot==0)
                PlayerStats.VisualEffectTypes &= ~( (int)EffectVisualTypes.Root);
            }
            else if (se.mSideeffectData.effecttype == EffectType.SpecialControl_Freeze)
            {
                NumOfFrozen--;
                if (NumOfFrozen == 0)
                    PlayerStats.VisualEffectTypes &= ~((int)EffectVisualTypes.Frozen);
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
            //SideEffect[] sideeffects = (positiveEffect) ? mSideEffectsPos : mSideEffectsNeg;
            //int slotid = -1;
            //int length = sideeffects.Length;
            //for (int i = 0; i < length; ++i)
            //{
            //    if (sideeffects[i] == se)
            //    {                    
            //        sideeffects[i] = null;
            //        StopSideEffectVisual(se, positiveEffect); 
            //        slotid = i;
            //        break;
            //    } 
            //}
            byte type = (byte)SideEffectsUtils.GetEffectHandleType(se.mSideeffectData.effecttype);
            int index = m_SideEffects[type].FindIndex(x => x == se);
            if (!m_RemovedSE.ContainsKey(type)) m_RemovedSE.Add(type, new List<int>());
            m_RemovedSE[type].Add(index);

            RemoveSideEffectFromList(se);
            StopSideEffectVisual(se, positiveEffect);

            if (se.IsDot())
            {
                SkillPassiveStats.OnDotEnd();
            }
            else if(se.IsDeBuff())
            {
                SkillPassiveStats.OnDebuffEnd();
            }
            return type;    
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
            int length = mSideEffectsPos.Length;
            for (int i = 0; i < length; ++i)
            {
                SideEffect se = mSideEffectsPos[i];
                if (se != null && se.mSideeffectData.effecttype == targettype)
                {
                    se.Stop();
                    break;//stop one at time;
                }
            }

            length = mSideEffectsNeg.Length;
            for (int i = 0; i < length; ++i)
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
            //int length = mSideEffectsPos.Length;
            //for (int i = 0; i < length; ++i)
            //{
            //    SideEffect se = mSideEffectsPos[i];
            //    if (se != null)
            //        se.Stop();
            //}

            //length = mSideEffectsNeg.Length;
            //for (int i = 0; i < length; ++i)
            //{
            //    SideEffect se = mSideEffectsNeg[i];
            //    if (se != null)
            //        se.Stop();
            //}

            foreach(var selist in m_SideEffects)
            {
                foreach(SideEffect se in selist.Value)
                {
                    se.Stop();
                    RemoveSideEffectFromList(se);
                }
                selist.Value.Clear();
            }
        }

        private long lastUpdatePersistentSE = 0;
        public override void Update(long dt)
        {
            base.Update(dt);

            if (IsAlive())
            {
                //foreach (SideEffect se in mSideEffectsPos)
                //{
                //    if (se != null)
                //        se.Update(dt);
                //}

                //foreach (SideEffect se in mSideEffectsNeg)
                //{
                //    if (se != null)
                //        se.Update(dt);
                //}
                foreach(var selist in m_SideEffects)
                {
                    foreach (SideEffect se in selist.Value)
                        se.Update(dt);
                }

                foreach(var set in m_RemovedSE)
                {
                    set.Value.Sort();
                    set.Value.Reverse();
                    foreach(int index in set.Value)
                    {
                        m_SideEffects[set.Key].RemoveAt(index);
                    }
                }
                m_RemovedSE.Clear();

                if (getHitTime > 0)
                {
                    getHitTime -= dt;
                }
            }
            lastUpdatePersistentSE += dt;
            if(lastUpdatePersistentSE >= 1000)
            {
                for (int i = mPersistentSideEffects.Count -1; i >=0; i--)
                {
                    SpecialSE se = mPersistentSideEffects[i];
                    if (se != null)
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

        public SideEffect GetSideEffect(int sid)
        {
            //foreach (SideEffect se in mSideEffectsPos)
            //{
            //    if (se != null && se.mSideeffectData.id == sid)
            //    {
            //        return se;
            //    }
            //}
            //foreach (SideEffect se in mSideEffectsNeg)
            //{
            //    if (se != null && se.mSideeffectData.id == sid)
            //    {
            //        return se;
            //    }
            //}

            foreach(var pair in m_SideEffects)
            {
                foreach(SideEffect se in pair.Value)
                {
                    if (se.mSideeffectData.id == sid) return se;
                }
            }

            return null;
        }

        public bool HasDot()
        {
            //foreach (SideEffect se in mSideEffectsNeg)
            //{
            //    if (se != null && se.IsDot())
            //    {
            //        return true;
            //    }
            //}

            foreach (SideEffect se in m_SideEffects[(byte)SideEffectsUtils.EffectHandleType.NonUpdates])
            {
                if (se.IsDot())
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
            //foreach (SideEffect se in mSideEffectsPos)
            //{
            //    if (se != null && se.mSideeffectData.effecttype == efftype)
            //    {
            //        return true;
            //    }
            //}
            //foreach (SideEffect se in mSideEffectsNeg)
            //{
            //    if (se != null && se.mSideeffectData.effecttype == efftype)
            //    {
            //        return true;
            //    }
            //}

            foreach(var pair in m_SideEffects)
            {
                foreach(SideEffect se in pair.Value)
                {
                    if (se.mSideeffectData.effecttype == efftype)
                        return true;
                }
            }

            return false;
        }

        public List<int> GetAppliedSideEffectsOfGroup(int groupid)
        {
            List<int> result = new List<int>();
            List<int> seids = Zealot.Repository.SideEffectRepo.GetSideEffectsOfGroup(groupid);
            
            foreach(var dic in m_SideEffects)
            {
                foreach(var se in dic.Value)
                {
                    // check for all related SE's
                    if (se.mSideeffectData.id > seids[seids.Count - 1]) // not in the list
                        continue;

                    int start = 0, middle = 0;
                    int end = seids.Count - 1;
                    while (start <= end)
                    {
                        middle = start + ((end - start) >> 1);
                        if (seids[middle] == se.mSideeffectData.id)
                        {
                            result.Add(seids[middle]);
                            break;
                        }
                        if (seids[middle] < se.mSideeffectData.id) start = middle + 1;
                        else end = middle - 1;
 
                    }
                }
            }
            return result;
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
        public virtual void OnControlChanged()
        {
        }

        public virtual void SetControlStatus(ControlSEType ctype)
        {
            PlayerStats.ControlStatus |= (byte)ctype;
        }

        public virtual void RemoveControlStatus(ControlSEType ctype)
        {
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

        public virtual void OnFrozen(float duration)
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

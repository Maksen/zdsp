#define DEBUG_SIDEEFF
namespace Zealot.Server.SideEffects
{
    using Zealot.Common;
    using Zealot.Server.Entities; 
    using Kopio.JsonContracts;
    /// <summary>
    /// control Sideeffect is in charge of the control status change like stun, root, slience.
    /// and also the Immune and remove(instant) of such status. 
    /// the Immune status is more like a buff instead of control.
    /// </summary>
    public abstract class ControlSE : SideEffect
    {
        protected ControlSEType mControlSEType;
        protected bool IsImmune = false;
        protected bool IsRemove = false;
        
        public ControlSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mNeedCaster = false;
        }

        public override bool IsControl()
        {
            return !IsRemove && !IsImmune;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData(); 
            EffectType effecttype = mSideeffectData.effecttype;
            //if (effecttype == EffectType.Control_Silence_Remove || effecttype == EffectType.Control_Root_Remove || effecttype == EffectType.Control_Slow_Remove
            //    || effecttype == EffectType.Control_Stun_Remove)
            //{
            //    IsRemove = true;
            //    mDuration = 0;
            //}
            //if (effecttype == EffectType.Control_Root_Immune || effecttype == EffectType.Control_Silence_Immune || effecttype == EffectType.Control_Stun_Immune ||
            //    effecttype == EffectType.Control_Slow_Immune)
            //{
            //    IsImmune = true;
            //}

            IsImmune = mTarget.IsControlImmune(SideEffectsUtils.EffectTypeToControlSEType(effecttype));

            if (!IsRemove )
            {
                if ( mDuration <= 0)
                {
                    //todo: control sideeffect should not be 0. 
                }
            }
        }

        protected virtual bool IsValidTarget()
        {
            if (mTarget.IsMonster())
            {
                Monster monster = ((Monster)mTarget);
                if (monster.mArchetype.monsterclass != MonsterClass.Normal)
                {
                    return false;
                }
                /*else if ((monster.mArchetype.immunities && mSideeffectData.effecttype == EffectType.Control_Stun)
                    /*|| (monster.mArchetype.immuneroot && mSideeffectData.effecttype == EffectType.Control_Root)
                    || (monster.mArchetype.immunesilence && mSideeffectData.effecttype == EffectType.Control_Silence))
                    return false;*/
            }
            return true;
        }

        protected override bool CheckApplyCondition()
        {
            if (IsControl() && mTarget.PlayerStats.InvincibleCtl)
                return false;
            if (!IsRemove && !IsImmune)
            {
                if (!IsValidTarget())
                    return false;
                if (mTarget.IsControlImmune(mControlSEType))
                    return false;
                return base.CheckApplyCondition();
            }
             //for Immune, it is overrite other instance of Immune.(Immune can only have one instance with one type)
             //e.g. if there is a immune of stun for 10 seconds left, another immune of stun for 5 seconds will not override it,
             //a immune of 15 seconds will override it. this is the normal rules. 
            if (IsImmune)
            {
                if(mTarget.IsControlImmune(mControlSEType))
                {
                    return false;//for now, simpily return false to prevent override.
                }
                return base.CheckApplyCondition();
            }
            return true;
        }

        /// <summary>
        //handle the start of durational control status or control immune stats.
        /// in child class 
        /// </summary>
        protected virtual void OnStart()
        {
            
        }
        
        protected override bool OnApply()
        { 
            if (base.OnApply()) //need for update time
            {
                if (IsDurationalSE())
                {
                    OnStart();
                    mTarget.OnControlChanged();
                    return true;
                }else
                {
                    if (IsRemove)
                    { 
                        mTarget.StopAllControl(mControlSEType);
                        return true;
                    }
                } 
            }
            return false;
        }

        /// <summary>
        //handle the end of durational control status or control immune stats.
        ///   
        /// </summary>
        protected override void OnRemove()
        {
            base.OnRemove();//remove first then call SetControlStatus
            mTarget.OnControlChanged();
             
        }

    }

    public class StunSE : ControlSE 
    {
        public StunSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mControlSEType = ControlSEType.Stun;
        }
        //the start of durational control status or control immune stats.
        protected override void OnStart()
        {
            base.OnStart();
            if (IsImmune)
            {
                //mTarget.ControlStats.StunImmuned  = true;
                mTarget.SetImmune(ImmuneSEType.Stun);
                return; 
            }
            mTarget.OnStun();
            mTarget.SetControlStatus(ControlSEType.Stun);
        }

        //the end of durational control status or control immune stats.
        protected override void OnRemove()
        {
            if (IsImmune) {
                mTarget.RemoveImmune(ImmuneSEType.Stun);
                return;
            }
            else
                mTarget.RemoveControlStatus(ControlSEType.Stun);
            base.OnRemove();
        }
    }

    //public class SlowSE : ControlSE
    //{
    //    float mChangeAmt;

    //    public SlowSE(SideEffectJson sideeffectData)
    //        : base(sideeffectData)
    //    {
    //        mControlSEType = ControlSEType.Slow;
    //        mChangeAmt = 0;
    //    }



    //    protected override void OnStart()
    //    {
    //        base.OnStart();
    //        if (IsImmune)
    //        {
    //            mTarget.ControlStats.SlowImmuned = true;
    //            return;
    //        }
    //        if (mSideeffectData.isrelative)
    //        {
    //            float percent = (float)GameUtils.Random(mSideeffectData.min, mSideeffectData.max) * 0.0001f;
    //            mChangeAmt = mTarget.PlayerStats.moveSpeed * percent;                
    //        }
    //        else
    //        {
    //            mChangeAmt = (float)GameUtils.Random(mSideeffectData.min, mSideeffectData.max);
    //        }
    //        if (mChangeAmt > mTarget.PlayerStats.moveSpeed-1.0f)//min speed is 1.0f;
    //            mChangeAmt = -mTarget.PlayerStats.moveSpeed+1.0f;

    //        //Change movespeed
    //        mTarget.PlayerStats.moveSpeed += mChangeAmt; 
    //    }

    //    protected override void OnRemove()
    //    {
    //        if (IsImmune)
    //        {
    //            mTarget.ControlStats.SlowImmuned = false;
    //        }
    //        else
    //            mTarget.PlayerStats.moveSpeed -= mChangeAmt;
    //        base.OnRemove();
    //    }
    //}

    public class RootSE : ControlSE
    {
        public RootSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mControlSEType = ControlSEType.Root;
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (IsImmune)
            {
                mTarget.SetImmune(ImmuneSEType.Root);
                return;
            }
            mTarget.OnRoot();
            mTarget.SetControlStatus(ControlSEType.Root);
        }

        protected override void OnRemove()
        {
            if (IsImmune)
            {
                mTarget.RemoveImmune(ImmuneSEType.Root); 
            }
            else
                mTarget.RemoveControlStatus(ControlSEType.Root);
            base.OnRemove();
        }
    }

    public class SilenceSE : ControlSE
    {
        public SilenceSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
        {
            mControlSEType = ControlSEType.Silence;
        }

        //Don't need to interrupt skill if already casting

        protected override bool OnApply()
        {
            return base.OnApply();
        }

        protected override void OnStart()
        {
            base.OnStart();
            if (IsImmune)
            {
                mTarget.SetImmune(ImmuneSEType.Silence);
                return;
            }
            //mTarget.OnRoot();
            mTarget.SetControlStatus(ControlSEType.Silence);
        }

        protected override void OnRemove()
        {
            if (IsImmune)
            {
                mTarget.RemoveImmune(ImmuneSEType.Silence);
            }
            else
                mTarget.RemoveControlStatus(ControlSEType.Silence);
            base.OnRemove();
        }
    }

    //public class DisarmedSE: ControlSE
    //{
    //    public DisarmedSE(SideEffectJson sideeffectData) : base(sideeffectData)
    //    {
    //        mControlSEType = ControlSEType.Disarmed;
    //    }
    //    protected override bool OnApply()
    //    {
    //        return base.OnApply();
    //    }

    //    protected override void OnStart()
    //    {
    //        base.OnStart();
    //        if (IsImmune)
    //        {
    //            mTarget.ControlStats.DisarmImmuned = true;
    //            return;
    //        }
    //        mTarget.OnStun();
    //        mTarget.ControlStats.Disarmnum++;
    //    }

    //    protected override void OnRemove()
    //    {
    //        if (IsImmune)
    //        {
    //            mTarget.ControlStats.DisarmImmuned = false;
    //        }
    //        else
    //            mTarget.ControlStats.Disarmnum--;
    //        base.OnRemove();
    //    }
    //}

    //public class DraggedSE : ControlSE
    //{
    //    public DraggedSE(SideEffectJson sideeffectData) : base(sideeffectData)
    //    {
    //        mControlSEType = ControlSEType.Disarmed;
    //    }
         
    //    public override bool IsDurationalSE()
    //    {
    //        return false; //the dur for this SE is for action not for updating. 
    //    }
    //    protected override bool OnApply()
    //    { 
    //        //no need to call base as it is not to be added into the sideeffect list;
    //        if (mTarget.PlayerStats.invincibleCtl)
    //            return false;
    //        if (!IsValidTarget())
    //            return false;
    //        UnityEngine.Vector3 dir = mCaster.Position - mTarget.Position;
             
    //        float dist = dir.magnitude;
    //        if(dist >1f)
    //            dist = dist - (float) GameUtils.Random(0, 1);
    //        UnityEngine.Vector3 pos = dir.normalized * dist + mTarget.Position;
    //        float dur = mSideeffectData.duration < 1f ? 1f : mSideeffectData.duration; //min dur is 1 second
    //        float speed = mSideeffectData.parameter < 1f ? 10f : mSideeffectData.parameter; //speed can not be <= 0;
    //        mTarget.onDragged(pos, dur, speed);  
    //        return true;

    //    }
    //}

    public class FearSE : ControlSE {
        public FearSE(SideEffectJson sideeffectData) : base(sideeffectData) {
            mControlSEType = ControlSEType.Fear;
        }

        protected override bool OnApply() {
            return base.OnApply();
        }
        protected override void OnStart() {
            base.OnStart();
            if (IsImmune) {
                mTarget.SetImmune(ImmuneSEType.Fear);
                return;
            }
            //mTarget.OnRoot();
            mTarget.SetControlStatus(ControlSEType.Fear);
        }

        protected override void OnRemove() {
            if (IsImmune) {
                mTarget.RemoveImmune(ImmuneSEType.Fear);
            }
            else
                mTarget.RemoveControlStatus(ControlSEType.Fear);
            base.OnRemove();
        }
    }

    public class TauntSE : ControlSE {
        public TauntSE(SideEffectJson sideeffectData) : base(sideeffectData) {
            mControlSEType = ControlSEType.Taunt;
        }

        protected override bool OnApply() {
            return base.OnApply();
        }
        protected override void OnStart() {
            base.OnStart();
            if (IsImmune) {
                mTarget.SetImmune(ImmuneSEType.Taunt);
                return;
            }
            //mTarget.OnRoot();
            mTarget.SetControlStatus(ControlSEType.Taunt);
        }

        protected override void OnRemove() {
            if (IsImmune) {
                mTarget.RemoveImmune(ImmuneSEType.Taunt);
            }
            else
                mTarget.RemoveControlStatus(ControlSEType.Taunt);
            base.OnRemove();
        }
    }
}